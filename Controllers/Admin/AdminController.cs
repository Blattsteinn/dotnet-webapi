
using FoodOrderingApi.Services.Admin;
using Microsoft.AspNetCore.Mvc;

using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Admin;

namespace FoodOrderingApi.Controllers.Admin;

// WHEN AUTH IS A THING PLACE MUST BE UNDER AN OWNER HERE:

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("places")]
    public IActionResult GetAll()
    {
        List<Place> places = _adminService.GetAllPlaces();
        return Ok(places);
    }

    [HttpGet("places/{id}")]
    public IActionResult GetPlace(int id)
    {
        Place? place = _adminService.GetPlace(id);
        if (place == null)
            return NotFound("Place does not exist.");

        return Ok(place);
    }
    
    [HttpPost("places")]
    public IActionResult AddPlace(Place place)
    {
        Place newPlace = _adminService.AddPlace(place);
        return Ok(newPlace);
    }

    [HttpPatch("places/{id}")]
    public IActionResult UpdatePlace(int id, AdminUpdatePlaceDto dto)
    {
        var (result, updated) = _adminService.Update(id, dto);
        return result switch
        {
            AdminUpdatePlaceResult.Success       => Ok(updated),
            AdminUpdatePlaceResult.PlaceNotFound => NotFound("Place does not exist."),
            AdminUpdatePlaceResult.UnknownChildId =>
                BadRequest("Payload references a child id that does not exist under this place."),
            _ => StatusCode(500)
        };
    }
    

    [HttpDelete("places/{id}")]
    public IActionResult Delete(int id)
    {
        bool exists = _adminService.Delete(id);
        if(!exists)
            return NotFound();

        return NoContent();
    }
}