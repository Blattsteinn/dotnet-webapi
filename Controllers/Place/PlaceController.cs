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


    [HttpGet]
    public IActionResult GetAll()
    {
        List<Place> places = _placeService.GetAll();
        return Ok(places);
    }
    
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var place = _placeService.Get(id);
        if (place == null)
        {
            return NotFound();
        }

        return Ok(place);
    }

    [HttpPost]
    public IActionResult Add(Place place)
    {
        Place newPlace = _placeService.Add(place);
        return Ok(newPlace);
    }

    [HttpPatch("{id}")]
    public IActionResult Update(int id, UpdatePlaceDto place)
    {
        var updatedProduct = _placeService.Update(id, place);
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