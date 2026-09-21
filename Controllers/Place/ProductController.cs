

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
        Product? product = _productService.Get(id, productCategoryId);
        if(product == null)
            return NotFound();

        return Ok(product);
    }

    
    [HttpPost]
    public IActionResult Add(Product product, int productCategoryId)
    {   
        var newProduct = _productService.Add(product, productCategoryId);

        if(newProduct == null)
            return NotFound("Product category does not exist.");

        return Ok(new CreateProductDto(
            newProduct.Id,
            newProduct.Name,
            newProduct.Description,
            newProduct.Hidden,
            newProduct.Price
        ));
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, int productCategoryId, UpdateProduct updateProduct)
    {
        Product? product = _productService.Update(id, productCategoryId, updateProduct);
        if(product == null)
            return NotFound();

        return Ok(product);  
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id, int productCategoryId)
    {
        bool exists = _productService.Delete(id, productCategoryId);
        if (!exists)
            return NotFound();

        return NoContent();
    }
}