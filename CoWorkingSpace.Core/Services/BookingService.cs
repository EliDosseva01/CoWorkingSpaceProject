using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Models;
using CoWorkingSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkingSpaceTest.Infrastructure.Data.Common;
using System.Net.Http;
using Nager.Date;
using Nager.Date.Model;
using Tiny.RestClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Identity;

namespace CoWorkingSpace.Core.Services;

public class BookingService : IBookingService
{
    private readonly IRepository _repo; 
    private readonly HttpClient _httpClient;

    public BookingService(IRepository repo, HttpClient httpClient)
    {
        _repo = repo; 
        _httpClient = httpClient;
    }

    public async Task<bool> BookASpace(BookingViewModel bookingData, int price, string userId)
    {
        Booking booking = new()
        {
            Id = bookingData.Id,
            Date = bookingData.Date.Date,
            StartTime = TimeSpan.Parse(bookingData.StartTime!),
            EndTime = TimeSpan.Parse(bookingData.EndTime!),
            Price = price,
            UserId = userId
        };

        await _repo.AddAsync(booking);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<PaginatedList<BookingViewModel>> GetMyBookings(string userId, int pageNumber, int pageSize)
    {
        var bookings = await _repo.AllReadonly<Booking>()
            .Where(b => b.User!.Id == userId && b.IsDeleted == false)
            .Select(booking => new BookingViewModel
            {
                Id = booking.Id,
                Date = booking.Date,
                StartTime = booking.StartTime.ToString(@"hh\:mm"),
                EndTime = booking.EndTime.ToString(@"hh\:mm"),
                Price = booking.Price,
                Status = booking.Status,
                UserId = userId
            })
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        var count = bookings.Count;
        var items = bookings.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedList<BookingViewModel>(items, count, pageNumber, pageSize);
    }

    public async Task<BookingViewModel> GetBookingById(int bookingId)
    {
        var booking = await _repo.All<Booking>()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingViewModel
            {
                Id = b.Id,
                Date = b.Date.Date,
                StartTime = b.StartTime.ToString(@"hh\:mm"),
                EndTime = b.EndTime.ToString(@"hh\:mm"),
                Price = b.Price,
            }).FirstAsync();

        return booking;
    }

    public async Task<List<Booking>> GetPreviousMonthBookings(string userId)
    {
        var bookings = await _repo.AllReadonly<Booking>()
            .Where(b => b.UserId == userId)
            .Where(b => b.Date.Month == DateTime.Now.Month - 1)
            .ToListAsync();

        return bookings;
    }

    public async Task EditBooking(int bookingId, BookingViewModel model)
    {
        var booking = await _repo.GetByIdAsync<Booking>(bookingId);
        if (model.Date < DateTime.Now)
        {
            throw new Exception("Edited date cannot be in the past.");
        }
        var price = CalculateBookingPrice(model.StartTime!, model.EndTime!, model.Date);

        booking.Date = model.Date.Date;
        booking.StartTime = TimeSpan.Parse(model.StartTime!);
        booking.EndTime = TimeSpan.Parse(model.EndTime!);
        //booking.Price = await price;
        booking.Status = model.Status!;

        await UpdateBookingAsync(booking);
    }

    public async Task Delete(int bookingId, ClaimsPrincipal user)
    {
        var booking = await _repo.GetByIdAsync<Booking>(bookingId);

        if (booking.Date > DateTime.Now || user.IsInRole("Admin"))
        {
            booking.IsDeleted = true;

            await _repo.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Cannot delete past bookings.");
        }
    }

    public async Task<int> CalculateBookingPrice(string startTime, string endTime, DateTime bookingDate)
    {
        var startHour = TimeSpan.Parse(startTime).Hours;
        var endHour = TimeSpan.Parse(endTime).Hours;
        var price = 0;

        bool isHoliday = await CheckDayIsHoliday(bookingDate.Date);

        for (int hour = startHour; hour < endHour; hour++)
        {
            if (bookingDate.DayOfWeek >= DayOfWeek.Monday && bookingDate.DayOfWeek <= DayOfWeek.Friday && hour >= 9 && hour < 18 && !isHoliday)
            {
                price += 5;
            }
            else
            {
                price += 7;
            }
        }
        return price;
    }

    protected async Task<bool> CheckDayIsHoliday(DateTime currentDate)
    {
        bool isHoliday = false;

        string apiUrl = "https://openholidaysapi.org/";
        string dateFormatted = currentDate.Date.ToString(@"yyyy\-MM\-dd");

        string queryParams = $"PublicHolidays?countryIsoCode=BG&languageIsoCode=BG&validFrom={dateFormatted}&validTo={dateFormatted}";

        //Request
        string requestUrl = apiUrl + queryParams;

        // Get Response
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
            if (responseContent.Length > 10)
            {
                isHoliday = true;
            }
        }
        else
        {
            Console.WriteLine("Request failed with status code: " + response.StatusCode);
        }

        return isHoliday;
    }

    public async Task<bool> CreateForm(string userId)
    {
        if (DateTime.Now.Day > 5)
        {
            throw new Exception("Form can be created only between 1st and 5th of the month.");
        }
        BookingForm bookingForm = new()
        {
            UserId = userId,
            Bookings = new List<Booking>(),
            Month = DateTime.Now.Month - 1,
            IsSubmitted = false,
            IsApproved = false,
        };

        await _repo.AddAsync(bookingForm);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<BookingForm> UpdateFormWithBookings(string userId, BookingForm bookingForm)
    {
        var bookings = await _repo.All<Booking>()
            .Where(b => b.User!.Id == userId && b.IsDeleted == false)
            .Where(b => b.Date.Month == DateTime.Now.Month - 1)
            .Select(booking => new Booking
            {
                Id = booking.Id,
                Date = booking.Date,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Price = booking.Price,
                FormId = bookingForm.Id,
                UserId = userId
            })
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        bookingForm.Bookings = bookings;

        await _repo.SaveChangesAsync();

        return bookingForm;
    }

    public async Task<BookingForm> GetBookingFormByUserId(string userId)
    {
        return (await _repo.All<BookingForm>().FirstOrDefaultAsync(f => f.UserId == userId))!;
    }

    public BookingViewFormModel ConvertToViewForm(BookingForm bookingForm)
    {
        return new BookingViewFormModel
        {
            Id = bookingForm.Id,
            UserId = bookingForm.UserId,
            Bookings = bookingForm.Bookings!,
            IsSubmitted = bookingForm.IsSubmitted,
        };
    }

    public async Task<bool> SubmitForm(BookingForm bookingForm)
    {
        if (bookingForm == null)
        {
            throw new Exception("Invalid form ID.");
        }

        if (bookingForm.IsSubmitted)
        {
            throw new Exception("The form has already been submitted.");
        }

        bookingForm.IsSubmitted = true;

        await _repo.SaveChangesAsync();

        return bookingForm.IsSubmitted;
    }

    public async Task AddBookingAsync(BookingViewModel bookingData)
    {
        var booking = new Booking
        {
            Date = bookingData.Date,
            StartTime = TimeSpan.Parse(bookingData.StartTime!),
            EndTime = TimeSpan.Parse(bookingData.EndTime!),
            Price = bookingData.Price,
            UserId = bookingData.UserId
        };

        await _repo.AddAsync(booking);
        await _repo.SaveChangesAsync();
    }

    public async Task<Booking> GetBookingByIdAsync(int id)
    {
        return await _repo.GetByIdAsync<Booking>(id);
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        _repo.Update(booking);
        await _repo.SaveChangesAsync();
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _repo.GetAllAsync<Booking>();
    }
    public async Task<IEnumerable<Booking>> GetPendingBookingsAsync()
    {
        return await _repo.All<Booking>().Where(b => b.Status == "Pending").ToListAsync();
    }

    private BookingViewModel ToViewModel(Booking booking)
    {
        return new BookingViewModel
        {
            Id = booking.Id,
            Date = booking.Date,
            StartTime = booking.StartTime.ToString(@"hh\:mm"),
            EndTime = booking.EndTime.ToString(@"hh\:mm"),
            Price = booking.Price,
            Status = booking.Status,
            UserId = booking.UserId,
            UserEmail = booking.User?.UserName
        };
    }

    public async Task<IEnumerable<BookingViewModel>> GetPendingBookingsViewModelAsync()
    {
        var bookings = await GetPendingBookingsAsync();
        // Извличане на всички уникални потребителски ID от резервациите
        var userIds = bookings.Select(b => b.UserId).Distinct().ToList();

        // Извличане на потребителските данни за тези ID-та
        var users = await _repo.AllReadonly<IdentityUser>()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        // Комбиниране на резервациите с потребителските данни
        var bookingViewModels = bookings
            .Where(b => b.Status == "Pending")
            .Select(book => new BookingViewModel
            {
                Id = book.Id,
                Date = book.Date,
                StartTime = book.StartTime.ToString(@"hh\:mm"),
                EndTime = book.EndTime.ToString(@"hh\:mm"),
                Price = book.Price,
                Status = book.Status,
                UserEmail = users.FirstOrDefault(u => u.Id == book.UserId)?.UserName
            })
            .ToList();

        return bookingViewModels;
    }

    public async Task<IEnumerable<BookingViewModel>> GetBookingsByUserIdAsync(string userId)
    {
        var bookings = await _repo.All<Booking>(b => b.UserId == userId).ToListAsync();

        return bookings.Select(b => new BookingViewModel
        {
            Id = b.Id,
            Date = b.Date,
            StartTime = b.StartTime.ToString(@"hh\:mm"),
            EndTime = b.EndTime.ToString(@"hh\:mm"),
            Price = b.Price,
            Status = b.Status,
            UserId = b.UserId,
            UserEmail = b.User?.UserName
        }).ToList();
    }

    public async Task<(decimal totalPrice, double totalHours)> GetTotalPriceAndHours()
{
    var bookings = await _repo.All<Booking>()
        .Where(b => !b.IsDeleted)
        .ToListAsync();

    var totalPrice = bookings.Sum(b => b.Price);
    var totalHours = bookings.Sum(b => (b.EndTime - b.StartTime).TotalHours);

    return (totalPrice, totalHours);
}
}