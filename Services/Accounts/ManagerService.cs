using FoodOrderingApi.Data;
using FoodOrderingApi.Dtos.Accounts.Manager;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Mappers;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Services.Places;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services.Accounts;

// Every operation here is scoped to one owner id, which the controller always
// takes from the caller's token — never from the request body.
public class ManagerService
{
    private readonly AppDbContext _db;

    public ManagerService(AppDbContext db)
    {
        _db = db;
    }



}
