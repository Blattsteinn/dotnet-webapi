using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Places;

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

    public ProductDetailDto? Get(int id, int productCategoryId)
    {
        Product? product = _db.Products
            .Include(p => p.AddOnCategories)
            .ThenInclude(aoc => aoc.AddOns)
            .FirstOrDefault(p => p.Id == id && p.ProductCategoryId == productCategoryId);

        if(product == null || product.ProductCategoryId != productCategoryId)
            return null;

        ProductDetailDto productForReturn = new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Hidden,
            product.AddOnCategories.Select(aoc => new AddOnCategoriesDetailDto(
                aoc.Id,
                aoc.Name,
                aoc.Required,
                aoc.Multichoice,
                aoc.AddOns.Select(ao => new AddOnDetailDto(
                    ao.Id,
                    ao.Name,
                    ao.Price,
                    ao.Hidden
                )).ToList()
            )).ToList()
        );


        return productForReturn;
    }

    public Product? Add(Product product, int productCategoryId)
    {
        ProductCategory? category = _db.ProductCategories.Find(productCategoryId);

        if (category == null)
            return null;

        product.ProductCategoryId = productCategoryId;
        _db.Products.Add(product);
        _db.SaveChanges();

        return product;
    }

    public Product? Update(int id, int productCategoryId, UpdateProduct updateProductDto)
    {
        Product? product = _db.Products.Find(id);
        if(product == null || product.ProductCategoryId != productCategoryId)
            return null;


        if (updateProductDto.Name != null)
            product.Name = updateProductDto.Name;

        if (updateProductDto.Description != null)
            product.Description = updateProductDto.Description;

        if (updateProductDto.Hidden.HasValue)
            product.Hidden = updateProductDto.Hidden.Value;

        if (updateProductDto.Price.HasValue)
            product.Price = updateProductDto.Price.Value;

        _db.SaveChanges();
        return product;
    }

    public bool Delete(int id, int productCategoryId)
    {
        Product? product = _db.Products.Find(id);
        if(product == null || product.ProductCategoryId != productCategoryId)
            return false;

        _db.Products.Remove(product);
        _db.SaveChanges();

        return true;
    }
}