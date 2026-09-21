using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;
public class AddOn
{
    public int Id { get; set;}

    public int AddOnCategoryId { get; set; }

    public AddOnCategory? AddOnCategory{ get; set; }
    

    [Required]
    public required string Name { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }

    public bool Hidden { get; set; } = true;

}