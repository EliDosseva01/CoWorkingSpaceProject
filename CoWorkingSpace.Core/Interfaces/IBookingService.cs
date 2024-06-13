using CoWorkingSpace.Core.Models;
using CoWorkingSpace.Infrastructure.Data;
using System.Security.Claims;

namespace CoWorkingSpace.Core.Interfaces;

public interface IBookingService
{
    /// <summary>
    /// Creates new booking in the database.
    /// </summary>
    /// <param name="booking">Gets the data entered by the user.</param>
    /// <param name="userId">Gets the logged in user id and adds it to the booking object.</param>
    /// <param name="bookingPrice">The price for the current booking.</param>
    /// <returns>True if the booking is successfully created.</returns>
    Task<bool> BookASpace(BookingViewModel booking, int bookingPrice, string userId);


    /// <summary>
    /// Retrieves a paginated list of bookings for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve the bookings.</param>
    /// <param name="pageNumber">The page number of the results to retrieve.</param>
    /// <param name="pageSize">The maximum number of bookings per page. It is set to 10 by default.</param>
    /// <returns>A paginated list of booking view models.</returns>
    Task<PaginatedList<BookingViewModel>> GetMyBookings(string userId, int pageNumber, int pageSize = 10);

    /// <summary>
    /// Retrieves a booking by its ID.
    /// </summary>
    /// <param name="bookingId">The ID of the booking to retrieve.</param>
    /// <returns>The booking view model corresponding to the provided ID.</returns>
    Task<BookingViewModel> GetBookingById(int bookingId);

    /// <summary>
    /// Takes all bookings by the logged in user for the current month - 1. 
    /// </summary>
    /// <param name="userId">the logged in user id.</param>
    /// <returns>The user bookings.</returns>
    Task<List<Booking>> GetPreviousMonthBookings(string userId);

    /// <summary>
    /// Updates the details of a booking.
    /// </summary>
    /// <param name="bookingId">The ID of the booking to edit.</param>
    /// <param name="model">The updated booking information.</param>
    /// <returns>Updates the specified booking with the provided model.</returns>
    Task EditBooking(int bookingId, BookingViewModel model);

    /// <summary>
    /// Removes selected booking from the list of all bookings.
    /// </summary>
    /// <param name="bookingId">The id of the selected booking.</param>
    /// <param name="user">The user data of which is the selected booking.</param>
    /// <exception>Throws Exception if trying to delete past bookings.</exception>
    Task Delete(int bookingId, ClaimsPrincipal user);

    /// <summary>
    /// Calculates the price for the booking based on start - end time period and type of day(workday/holiday).
    /// </summary>
    /// <param name="startTime">The start time entered by the user</param>
    /// <param name="endTime">The end time entered by the user</param>
    /// <param name="bookingDate"></param>
    /// <returns>Calculated price for the entered time period.</returns>
    Task<int> CalculateBookingPrice(string startTime, string endTime, DateTime bookingDate);

    /// <summary>
    /// Creates an empty form for the logged in user. This method can be used only between 1st nad 5th day of the month.
    /// </summary>
    /// <param name="userId">The logged in user id.</param>
    /// <exception cref="Exception">Message: Form can be created only between 1st and 5th of the month.</exception>
    /// <returns>True/False</returns>
    Task<bool> CreateForm(string userId);

    /// <summary>
    /// Takes the already created user empty form and adds all bookings of the current month to the form.
    /// </summary>
    /// <param name="userId">The logged in user id.</param>
    /// <param name="bookingForm">The created empty form.</param>
    /// <returns></returns>
    Task<BookingForm> UpdateFormWithBookings(string userId, BookingForm bookingForm);


    /// <summary>
    /// Retrieves the booking form for the specified by id user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<BookingForm> GetBookingFormByUserId(string userId);

    /// <summary>
    /// Takes a booking form as it comes from the database and converts it as a "BookingViewForm".
    /// </summary>
    /// <param name="bookingForm"></param>
    /// <returns>The converted booking form.</returns>
    BookingViewFormModel ConvertToViewForm(BookingForm bookingForm);

    /// <summary>
    /// Updates current user form isSubmitted property to true so it can be viewed from the admin account.
    /// </summary>
    /// <param name="bookingForm"></param>
    /// <exception cref="Exception">Throws an error if you try to submit again an already submitted form.</exception>
    /// <returns>The value of the isSubmitted property.</returns>
    Task<bool> SubmitForm(BookingForm bookingForm);

    Task<Booking> GetBookingByIdAsync(int id);
    Task UpdateBookingAsync(Booking booking);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<IEnumerable<Booking>> GetPendingBookingsAsync();
    Task<IEnumerable<BookingViewModel>> GetPendingBookingsViewModelAsync();

    Task<(decimal totalPrice, double totalHours)> GetTotalPriceAndHours();
}