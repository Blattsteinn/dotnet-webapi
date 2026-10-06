using FoodOrderingApi.Data;

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
