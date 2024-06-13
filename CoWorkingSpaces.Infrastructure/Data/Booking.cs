using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CoWorkingSpace.Infrastructure.Data;

/// <summary>
/// Model for all bookings which will be saved in database
/// </summary>
public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$")]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public IdentityUser? User { get; set; }

    public int FormId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public string Status { get; set; } = "Pending";
}