
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Dtos.AddOnCategory;

namespace FoodOrderingApi.Controllers.Places;

[ApiController]
[Route("api/v1/products/{productId}/add-on-categories")]
public class AddOnCategoryController : ControllerBase
{
    private readonly AddOnCategoryService _addOnControllerService;
    public AddOnCategoryController(AddOnCategoryService addOnControllerService)
    {
        _addOnControllerService = addOnControllerService;
    }
    
    [HttpGet]
    public IActionResult GetAll(int productId)
    {
        List<AddOnCategory>? addOnCategories = _addOnControllerService.GetAll(productId);
        if(addOnCategories == null)
            return NotFound("Product does not exists");
            
        return Ok(addOnCategories);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int productId)
    {
        AddOnCategory? addOnCategory = _addOnControllerService.Get(id, productId);
        if(addOnCategory == null)
            return NotFound();
        
        return Ok(addOnCategory);
    }

    [HttpPost]
    public IActionResult Add(int productId, AddOnCategory addOnCategory)
    {
        var newAddOnCategory = _addOnControllerService.Add(productId, addOnCategory);
        if(newAddOnCategory == null)
            return NotFound("Product does not exists");
        
        return Ok(new CreateAddOnCategoryDto(
            newAddOnCategory.Id,
            newAddOnCategory.ProductId,
            newAddOnCategory.Required,
            newAddOnCategory.Multichoice,
            newAddOnCategory.Name
        ));
    }  

    [HttpPatch("{id}")]
    public IActionResult Update(int id, int productId, UpdateAddOnCategory addOnCategory)
    {
        var updatedAddOnCategory = _addOnControllerService.Update(id, productId, addOnCategory);
        if(updatedAddOnCategory == null)
            return NotFound("Product does not exists");

        return Ok(updatedAddOnCategory);
    }
    
    [HttpDelete("{id}")]
    public IActionResult Delete(int id, int productId)
    {
        bool exists = _addOnControllerService.Delete(id, productId);
        if(!exists)
            return NotFound();

        return NoContent();
    }
}
