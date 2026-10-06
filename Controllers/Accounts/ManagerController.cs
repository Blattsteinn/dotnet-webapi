using System.Security.Claims;

using FoodOrderingApi.Authorization;
using FoodOrderingApi.Services.Orders;
using FoodOrderingApi.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingApi.Controllers.Accounts;


// Conditions for callers:
// 1) the caller must hold the Owner role (they are allowed to run places).
// 2) every operation below is then scoped to the places they actually own.
[ApiController]
[Route("api/v1/owner")]
[Authorize(Roles = Roles.Owner)]
public class ManagerController : ControllerBase
{
    private readonly ManagerService _managerService;
    private readonly OrderService _orderService;

    public ManagerController(ManagerService managerService, OrderService orderService)
    {
        _managerService = managerService;
        _orderService = orderService;
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // Manager control actions regarding place

    // This finds an user from auth token.
    private string CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


}
