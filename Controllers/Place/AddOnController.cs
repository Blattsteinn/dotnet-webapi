

using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Services.Places;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Controllers.Places;

[ApiController]
[Route("api/v1/add-on-categories/{addOnCategoryId}/add-ons")]
public class AddOnController : ControllerBase
{
    private readonly AddOnService _addOnService;
    public AddOnController(AddOnService addOnService)
    {
        _addOnService = addOnService;
    }

    // Won't be using these much. 
    // Product GET includes this table.

    [HttpGet]
    public IActionResult GetAll(int addOnCategoryId)
    {   
        List<AddOn>? addOns = _addOnService.GetAll(addOnCategoryId);
        if(addOns == null)
            return NotFound("AddOnCategory does not exists.");

        return Ok(addOns);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id, int addOnCategoryId)
    {
        AddOn? addOn = _addOnService.Get(id, addOnCategoryId);
        if(addOn == null)
            return NotFound();

        return Ok(addOn);
    }

    [HttpPost]
    public IActionResult Add(AddOn addOn, int addOnCategoryId)
    {
        var newAddOn = _addOnService.Add(addOn, addOnCategoryId);

        if(newAddOn == null)
            return NotFound("AddOnCategory does not exists.");

        var response = new AddOnResponse(
            newAddOn.Id,
            newAddOn.AddOnCategoryId,
            newAddOn.Name,
            newAddOn.Price,
            newAddOn.Hidden
        );

        return Ok(response);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, int addOnCategoryId, UpdateAddOn updateAddOnDto)
    {
        AddOn? updatedAddOn = _addOnService.Update(id, addOnCategoryId, updateAddOnDto);
        if (updatedAddOn == null)
            return NotFound("Add-on does not exists.");

        return Ok(new AddOnResponse(
            updatedAddOn.Id,
            updatedAddOn.AddOnCategoryId,
            updatedAddOn.Name,
            updatedAddOn.Price,
            updatedAddOn.Hidden
        ));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id, int addOnCategoryId)
    {   
        bool exists = _addOnService.Delete(id, addOnCategoryId);
        if (!exists)
            return NotFound();

        return NoContent();
    }
}

public record AddOnResponse(
    int Id,
    int AddOnCategoryId,
    string Name,
    decimal Price,
    bool Hidden
);