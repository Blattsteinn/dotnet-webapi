using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;
public class AddOn
{
    public int Id { get; set;}

    public int AddOnCategoryId { get; set; }

    public AddOnCategory? AddOnCategory{ get; set; }
    

    [Required]
    public required string Name { get; set; }

    [Required]
    [Range(0.00, double.MaxValue)]
    public decimal Price { get; set; }

    public bool Hidden { get; set; } = true;

}