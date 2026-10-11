using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Places;
public class AddOn
{
    public int Id { get; set;}

    public int AddOnCategoryId { get; set; }

    public AddOnCategory? AddOnCategory{ get; set; }
    

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Range(typeof(decimal), "0.00", "10000")]
    public decimal Price { get; set; }

    public bool Hidden { get; set; } = true;

}