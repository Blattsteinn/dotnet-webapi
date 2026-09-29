

using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Controllers.Places;

[ApiController]
[Route("api/v1/product-categories/{productCategoryId}/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;
    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult GetAll(int productCategoryId)
    {
        List<Product>? products = _productService.GetAll(productCategoryId);
        if(products == null)
            return NotFound("Product category does not exist.");

        return Ok(products);
    }   

    [HttpGet("{id}")]
    public IActionResult Get(int id, int productCategoryId)
    {
        GetProductDto? product = _productService.Get(id, productCategoryId);
        if(product == null)
            return NotFound("Product does not exist.");

        return Ok(product);
    }
    
}