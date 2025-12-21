using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? AdminGroupId { get; set; }
    public AdminGroup? AdminGroup { get; set; }
}
