using System.ComponentModel.DataAnnotations;

namespace BookHub.Mvc.Models.Admin;

public class AdminResetPasswordViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? DisplayName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}