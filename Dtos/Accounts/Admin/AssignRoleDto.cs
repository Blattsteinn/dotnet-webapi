using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Accounts.Admin;

// Body for POST /api/v1/admin/users/{userId}/roles
public record AssignRoleDto(
    [Required]
    [MaxLength(50)]
    string Role
);
