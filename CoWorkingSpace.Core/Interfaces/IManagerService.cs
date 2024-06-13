using CoWorkingSpace.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace CoWorkingSpace.Core.Interfaces
{
    public interface IManagerService
    {
        /// <summary>
        /// Sends a get request to the database to retrieve all made bookings.
        /// </summary>
        /// <returns>All bookings.</returns>
        Task<List<BookingViewModel>> GetAllBookings();

        /// <summary>
        /// Sends a get request to the database to retrieve all registered users.
        /// </summary>
        /// <param name="pageNumber">Indicates current page.</param>
        /// <param name="pageSize">Indicates the possible number of entries on 1 page. It is set to 10 by default.</param>
        /// <returns>All users who have an account in the database.</returns>
        Task<PaginatedList<IdentityUser>> GetAllUsers(int pageNumber, int pageSize = 7);

        /// <summary>
        /// Sends a get request to retrieve all bookings made in database.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>All bookings made by specified user.</returns>
        Task<List<BookingViewModel>> GetUserBookings(string userId);

        /// <summary>
        /// Retrieves all bookings from the database and calculate the total used hours and total income for that time.
        /// </summary>
        /// <returns>Total price and total hours tuple for all made bookings.</returns>
        Task<(decimal totalPrice, double totalHours)> GetTotalPriceAndHours();
    }
}
