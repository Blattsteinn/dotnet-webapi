
using FoodOrderingApi.Services.Admin;
using Microsoft.AspNetCore.Mvc;

using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Dtos.Admin;

namespace FoodOrderingApi.Controllers.Admin;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok();
    }

    [HttpGet("places/{id}")]
    public IActionResult GetPlace(int id)
    {
        Place? place = _adminService.GetPlace(id);
        if (place == null)
            return NotFound("Place does not exist.");

        return Ok(place);
    }
    
    [HttpPatch("places/{id}")]
    public IActionResult UpdatePlace(int id, AdminUpdatePlaceDto dto)
    {
        var updated = _adminService.Update(id, dto);
        if (updated == null)
            return NotFound("Place does not exist.");

        return Ok(updated);

    }
    
    [HttpDelete("places{id}")]
    public IActionResult Delete(int id)
    {
        return Ok();
    }
}