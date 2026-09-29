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
    
    public List<Product>? GetAll(int productCategoryId)
    {   
        bool productCategoryExists = _db.ProductCategories.Any(pc => pc.Id == productCategoryId);
        if(!productCategoryExists)
            return null;

        List<Product> products = _db.Products
        .Where(p => p.ProductCategoryId == productCategoryId)
        .ToList();

        return products;
    }

    public GetProductDto? Get(int id, int productCategoryId)
    {
        var product = _db.Products
            .Include(p => p.AddOnCategories)
            .ThenInclude(aoc => aoc.AddOns.Where(ao => !ao.Hidden))
            .FirstOrDefault(p => p.Id == id && p.ProductCategoryId == productCategoryId);

        if(product == null || product.ProductCategoryId != productCategoryId)
            return null;

        return ProductDtoMapper.MapToGetDto(product);
    }

}