using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Places;

using FoodOrderingApi.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services.Places;

public class ProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db)
    {
        _db = db;
    }
    
    // Either endpoint is a public read, so they apply the same visibility rules
    // as the public place endpoints: a hidden category is reported exactly like
    // a missing one, and hidden products never appear.
    public List<ProductSummaryDto>? GetAll(int productCategoryId)
    {   
        if (!VisibleCategoryExists(productCategoryId))
            return null;

        return _db.Products
        .Where(p => p.ProductCategoryId == productCategoryId && !p.Hidden)
        .OrderBy(p => p.Id)
        .ToList()
        .Select(ProductDtoMapper.MapToSummaryDto)
        .ToList();
    }

    public GetProductDto? Get(int id, int productCategoryId)
    {
        if (!VisibleCategoryExists(productCategoryId))
            return null;

        var product = _db.Products
            .Include(p => p.AddOnCategories)
            .ThenInclude(aoc => aoc.AddOns.Where(ao => !ao.Hidden))
            .FirstOrDefault(p => p.Id == id
                && p.ProductCategoryId == productCategoryId
                && !p.Hidden);

        if(product == null)
            return null;

        return ProductDtoMapper.MapToGetDto(product);
    }

    private bool VisibleCategoryExists(int productCategoryId) =>
        _db.ProductCategories.Any(pc => pc.Id == productCategoryId && !pc.Hidden);

}