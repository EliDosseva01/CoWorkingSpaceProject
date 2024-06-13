using CoWorkingSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoWorkingSpace.Core.Models
{
    public class BookingViewFormModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }

        public List<Booking>? Bookings { get; set; }

        public int FormId { get; set; }

        public bool IsSubmitted { get; set; }
    }
}
