using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Mappers;
using FoodOrderingApi.Services.Places;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Services.Orders;

public enum AddOrderResult
{
    Success,
    PlaceNotFound,
    PlaceClosed,
    ZeroProducts,
    ProductNotFound,
    AddOnNotFound,
    RequiredAddOnsNotSpecified,
    SingleChoiceAddOnsExceeded,
}

public enum SetOrderStatusResult
{
    Success,
    OrderNotFound,
    InvalidTransition,
}

// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
// TWO SCOPES:
// 1. Customer
// 2. Place Owner
// ---------------------------------------------------------------------------
public class OrderService
{
    private readonly AppDbContext _db;
    private readonly PlaceScheduleService _scheduleService;
    private readonly TimeProvider _timeProvider;

    public OrderService(AppDbContext db, PlaceScheduleService schedule, TimeProvider timeProvider)
    {
        _db = db;
        _scheduleService = schedule;
        _timeProvider = timeProvider;
    }

    private IQueryable<Order> WithItems() =>
        _db.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.OrderItemAddOns);

    private bool OwnerHoldsPlace(string ownerId, int placeId) =>
        _db.PlaceOwnerships.Any(o => o.PlaceId == placeId && o.OwnerId == ownerId);

    // An ownership row only grants control while the caller still holds the
    // Owner role; a Managers row grants control on its own. Gating the owner
    // branch on the role is what makes a demotion revoke access without having
    // to delete the ownership history.
    private bool ManagesPlace(string userId, bool isOwner, int placeId) =>
        (isOwner && OwnerHoldsPlace(userId, placeId)) ||
        _db.Managers.Any(m => m.PlaceId == placeId && m.UserId == userId);

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // CUSTOMER SCOPE
    // ---------------------------------------------------------------------------

    public List<GetOrderDto> GetForUser(string userId)
    {
        return WithItems()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList()
            .Select(OrderDtoMapper.MapToDto)
            .ToList();
    }

    // Returns null when the order is missing OR when it belongs to
    // somebody else.
    public GetOrderDto? GetForUser(string userId, int orderId)
    {
        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : OrderDtoMapper.MapToDto(order);
    }

    // userId here comes from the caller's token.
    public (GetOrderDto?, AddOrderResult) Add(CreateOrderDto orderDto, string? userId)
    {   

        /// CLIENT SENDS: CreateOrderDto
        /// 
        /// We construct Order, OrderItem, OrderItemAddOn 
        /// as we loop through IDs in CreateOrderDto
        /// 
        /// RETURN VALUE: OrderDto

        int placeId = orderDto.PlaceId;

        // If place does not exist return early
        Place? place = _db.Places
            .Include(p => p.OpeningHours)
            .FirstOrDefault(p => p.Id == placeId);

        if(place == null)
            return (null, AddOrderResult.PlaceNotFound);

        // The place must be open right now: accepting orders and within its
        // weekly hours. Checked after the place is validated and before any
        // item is processed. Status updates are not affected by this.
        if (!_scheduleService.IsOpen(place))
            return (null, AddOrderResult.PlaceClosed);
        
        // If no products return early
        List<CreateOrderItemDto> orderItemDto = orderDto.OrderItems;
        if(orderItemDto.Count == 0)
            return(null, AddOrderResult.ZeroProducts);
        
        var newOrder = new Order
        {
            PlaceId = placeId,
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };


        foreach(var orderItem in orderItemDto)
        {   
            // If product does not exist return early
            Product? product = _db.Products
                .Include(p => p.AddOnCategories)
                .FirstOrDefault(p => p.Id == orderItem.ProductId);
            if(product == null)
                return (null, AddOrderResult.ProductNotFound);

            var newOrderItem = new OrderItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = orderItem.Quantity
            };

            // Each product has addOnCategories. Categories having Required must
            // have at least one add-on chosen, and categories that are not
            // Multichoice accept at most one add-on.
            var requiredCategories = product.AddOnCategories
                .Where(c => c.Required)
                .Select(c => c.Id)
                .ToHashSet();

            var singleChoiceCategories = product.AddOnCategories
                .Where(c => !c.Multichoice)
                .Select(c => c.Id)
                .ToHashSet();

            var chosenPerCategory = new Dictionary<int, int>();

            List<CreateOrderItemAddOnDto> orderItemAddOnsDto = orderItem.AddOns;
            foreach (var addOnDto in orderItemAddOnsDto)
            {
                // If addOn does not exist return early
                AddOn? addOn = _db.AddOns.Find(addOnDto.AddOnId);
                if(addOn == null)
                    return (null, AddOrderResult.AddOnNotFound);

                int addOnCategoryId = addOn.AddOnCategoryId;

                chosenPerCategory.TryGetValue(addOnCategoryId, out int chosen);
                chosen++;
                chosenPerCategory[addOnCategoryId] = chosen;

                // A category that is not Multichoice accepts a single add-on.
                if (singleChoiceCategories.Contains(addOnCategoryId) && chosen > 1)
                {
                    return (null, AddOrderResult.SingleChoiceAddOnsExceeded);
                }

                var newOrderItemAddOn = new OrderItemAddOn
                {
                    AddOnId = addOn.Id,
                    Name = addOn.Name,
                    Price = addOn.Price,
                    Quantity = addOnDto.Quantity
                };

                newOrderItem.OrderItemAddOns.Add(newOrderItemAddOn);
            }
            
            // Check if required addOns are here
            if (!requiredCategories.IsSubsetOf(chosenPerCategory.Keys))
            {
                return (null, AddOrderResult.RequiredAddOnsNotSpecified);
            }

            newOrder.OrderItems.Add(newOrderItem);
 
        }

        _db.Orders.Add(newOrder);
        _db.SaveChanges();
        return (OrderDtoMapper.MapToDto(newOrder), AddOrderResult.Success);
  
    }
    

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // MANAGER & OWNER ORDER VIEW
    // ---------------------------------------------------------------------------

    // null == the place does not exist, or is not managed by this caller.
    public List<GetOrderDto>? GetForPlace(string callerId, bool callerIsOwner, int placeId)
    {
        if (!ManagesPlace(callerId, callerIsOwner, placeId))
            return null;

        return WithItems()
            .Where(o => o.PlaceId == placeId)
            .OrderByDescending(o => o.CreatedAt)
            .ToList()
            .Select(OrderDtoMapper.MapToDto)
            .ToList();
    }

    public GetOrderDto? GetForPlace(string callerId, bool callerIsOwner, int placeId, int orderId)
    {
        if (!ManagesPlace(callerId, callerIsOwner, placeId))
            return null;

        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.PlaceId == placeId);

        return order == null ? null : OrderDtoMapper.MapToDto(order);
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // EDITING STATUS FOR THE ORDER.
    // ---------------------------------------------------------------------------

    // The contents of a placed order are immutable; only the status can change.
    // The caller may be the place's owner (while still holding the Owner role)
    // or one of the place's managers.
    public (GetOrderDto?, SetOrderStatusResult) SetStatusForPlace(
        string callerId, bool callerIsOwner, int placeId, int orderId, OrderStatus status)
    {
        if (!ManagesPlace(callerId, callerIsOwner, placeId))
            return (null, SetOrderStatusResult.OrderNotFound);

        Order? order = WithItems()
            .FirstOrDefault(o => o.Id == orderId && o.PlaceId == placeId);

        if (order == null)
            return (null, SetOrderStatusResult.OrderNotFound);

        if (!IsTransitionAllowed(order.Status, status))
            return (null, SetOrderStatusResult.InvalidTransition);

        order.Status = status;
        _db.SaveChanges();

        return (OrderDtoMapper.MapToDto(order), SetOrderStatusResult.Success);
    }

    // Status order: Pending -> Confirmed -> Preparing -> Ready -> Completed
    private static bool IsTransitionAllowed(OrderStatus from, OrderStatus to) => (from, to) switch
    {
        (OrderStatus.Pending,   OrderStatus.Confirmed) => true,
        (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
        (OrderStatus.Preparing, OrderStatus.Ready)     => true,
        (OrderStatus.Ready,     OrderStatus.Completed) => true,

        // cancelling is allowed until the order is completed
        (OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready,
        OrderStatus.Cancelled) => true,

        // everything else, including anything out of Completed or Cancelled
        _ => false
    };

}