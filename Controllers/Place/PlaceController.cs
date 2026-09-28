using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Controllers.Places;

[ApiController]
[Route("api/v1/places")]
public class PlaceController : ControllerBase
{   
    private readonly PlaceService _placeService;
    public PlaceController(PlaceService service)
    {
        _placeService = service;
    }


    // Not nested on purpose.
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Place> places = _placeService.GetAll();
        return Ok(places);
    }
    
    // Returns Place + ProductCategories + Product
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        GetPlaceDto? place = _placeService.Get(id);
        if (place == null)
        {
            return NotFound("Place does not exist.");
        }

        return Ok(place);
    }

    // Not nested on purpose. 
    [HttpPost]
    public IActionResult Add(Place place)
    {
        Place newPlace = _placeService.Add(place);
        return Ok(newPlace);
    }

    // Returns Place + ProductCategories + Product
    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdatePlaceDto place)
    {
        GetPlaceDto? updatedProduct = _placeService.Update(id, place);
        if(updatedProduct == null)
            return NotFound();

        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        bool exists = _placeService.Delete(id);
        if(!exists)
            return NotFound();

        return NoContent();
    }
}