using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Controllers.Places;

[ApiController]
[Route("api/v1/places/{placeId}/product-categories")]
public class ProductCategoryController : ControllerBase
{
    private readonly ProductCategoryService _productCategoryService;
    public ProductCategoryController(ProductCategoryService productCategoryService)
    {
        _productCategoryService = productCategoryService;
    }

    [HttpGet]
    public IActionResult GetAll(int placeId)
    {
        List<ProductCategory>? productCategories = _productCategoryService.GetAll(placeId);
        if(productCategories == null)
            return NotFound("Place does not exist.");

        return Ok(productCategories);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int placeId)
    {
        ProductCategory? productCategory = _productCategoryService.Get(id, placeId);
        if (productCategory == null)
            return NotFound();

        return Ok(productCategory);
    }

    [HttpPost]
    public IActionResult Add(ProductCategory productCategory, int placeId)
    {
        ProductCategory? newProductCategory = _productCategoryService.Add(productCategory, placeId);

        if (newProductCategory == null)
            return NotFound("Place does not exist.");

        return Ok(newProductCategory);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, int placeId, UpdateProductCategory productCategory)
    {
        ProductCategory? updatedProductCategory = _productCategoryService.Update(id, placeId, productCategory);
        if(updatedProductCategory == null)
            return NotFound();

        return Ok(updatedProductCategory);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id, int placeId)
    {   
        bool exists = _productCategoryService.Delete(id, placeId);
        if (!exists)
            return NotFound();

        return NoContent();
    }
}