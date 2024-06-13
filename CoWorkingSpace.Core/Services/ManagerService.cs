using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Models;
using CoWorkingSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkingSpaceTest.Infrastructure.Data.Common;

namespace CoWorkingSpace.Core.Services;

public class ManagerService : IManagerService
{
    private readonly IRepository _repo;

    public ManagerService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedList<IdentityUser>> GetAllUsers(int pageNumber, int pageSize)
    {
        var users = await _repo.All<IdentityUser>().ToListAsync();

        var userCount = users.Count;
        var usersPerPage = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedList<IdentityUser>(usersPerPage, userCount, pageNumber, pageSize);
    }

    public async Task<List<BookingViewModel>> GetUserBookings(string userId)
    {
        var bookings = await _repo.AllReadonly<Booking>()
            .Where(b => b.IsDeleted == false && b.Status == "Confirmed")
            .Select(booking => new BookingViewModel
            {
                Id = booking.Id,
                Date = booking.Date.Date,
                StartTime = booking.StartTime.ToString(@"hh\:mm"),
                EndTime = booking.EndTime.ToString(@"hh\:mm"),
                Price = booking.Price,
                UserId = booking.UserId,
                UserEmail = booking.User!.UserName

            }).Where(b => b.UserId == userId).ToListAsync();

        return bookings;
    }

    public async Task<List<BookingViewModel>> GetAllBookings()
    {
        var bookings = await _repo.AllReadonly<Booking>()
            .Select(booking => new BookingViewModel
            {
                Id = booking.Id,
                Date = booking.Date.Date,
                StartTime = booking.StartTime.ToString(),
                EndTime = booking.EndTime.ToString(),
                Price = booking.Price,
                UserId = booking.UserId,
                UserEmail = booking.User!.UserName

            }).ToListAsync();

        return bookings;
    }

    public async Task<(decimal totalPrice, double totalHours)> GetTotalPriceAndHours()
    {
        var bookings = await _repo.AllReadonly<Booking>()
            .Where(b => b.Status == "Confirmed" && !b.IsDeleted)
            .Select(book => new Booking
            {
                StartTime = book.StartTime,
                EndTime = book.EndTime,
                Price = book.Price,
            })
            .ToListAsync();

        var totalPrice = (int)bookings.Sum(book => book.Price);
        var totalHours = bookings.Sum(book => book.EndTime.Subtract(book.StartTime).TotalHours);

        return (totalPrice, totalHours);
    }
}