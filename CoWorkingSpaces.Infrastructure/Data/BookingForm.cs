using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoWorkingSpace.Infrastructure.Data;

public class BookingForm
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public IdentityUser? User { get; set; }

    public List<Booking>? Bookings { get; set; }

    [Required]
    public int Month { get; set; }

    public string? Comment { get; set; }

    [Required]
    public bool IsSubmitted { get; set; } = false;

    [Required]
    public bool IsApproved { get; set; } = false;
}