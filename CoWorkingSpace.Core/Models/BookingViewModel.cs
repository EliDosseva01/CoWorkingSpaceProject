using System.ComponentModel.DataAnnotations;

namespace CoWorkingSpace.Core.Models
{
    /// <summary>
    /// Model for all bookings which will be visualized on the client
    /// </summary>
    public class BookingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [RegularExpression(@"^(0[8-9]|1[0-9]|2[0-1]):00$", ErrorMessage = "Start Time must be between 08:00 and 19:00 and format HH:mm")]
        [Display(Name = "Start Time")]
        public string? StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [RegularExpression(@"^(0[9-9]|1[0-9]|2[0-0]):00$", ErrorMessage = "End Time must be between 09:00 and 20:00 and format HH:mm")]
        [Display(Name = "End Time")]
        public string? EndTime { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        public decimal Price { get; set; }

        public string? UserId { get; set; }

        [Display(Name = "User Email")]
        public string? UserEmail { get; set; }
        public string? Status { get; set; }
    }
}
