using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Mvc;

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
        List<PlaceSummaryDto> places = _placeService.GetAll();
        return Ok(places);
    }
    
    // Returns Place + ProductCategories + Product
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        PublicPlaceDto? place = _placeService.Get(id);
        if (place == null)
        {
            return NotFound("Place does not exist.");
        }

        return Ok(place);
    }

}