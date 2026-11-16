using System.ComponentModel.DataAnnotations;

namespace DotNetMastery.WebApi.Models;

/// <summary>
/// Data Transfer Object (DTO) for creating a user, demonstrating Model Binding & DataAnnotations validation.
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120.")]
    public int Age { get; set; }
}

public record UserResponse(int Id, string Name, string Email, int Age, DateTime CreatedAtUtc);
