using CoWorkingSpace.Core.Constants;
using CoWorkingSpace.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CoWorkingSpace.Infrastructure.Data;

namespace CoWorkingSpace.Controllers;

[Authorize(Roles = "Admin")]
public class ManagerController : BaseController
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IManagerService _managerService;
    private readonly IBookingService _bookingService;

    public ManagerController(UserManager<IdentityUser> userManager, IManagerService managerService, IBookingService bookingService)
    {
        _userManager = userManager;
        _managerService = managerService;
        _bookingService = bookingService;
    }

    /// <summary>
    /// Displays all users who have booked for the current month and shows total income generated and total hours used
    /// </summary>
    /// <param name="pageNumber">Display the number of the current page when there are more than 1 page of bookings.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ShowUsers(int? pageNumber)
    {
        try
        {
            var users = await _managerService.GetAllUsers(pageNumber ?? 1);
            var adminData = users.Find(u => u.Id == GetLoggedUserId());
            users.Remove(adminData!);

            var (totalPrice, totalHours) = await _managerService.GetTotalPriceAndHours();

            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalHours = totalHours;

            return View(users);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return View();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <returns>A view with all bookings for the specified user.</returns>
    [HttpGet]
    public async Task<IActionResult> UserBookings(string id)
    {
        try
        {
            var bookings = await _managerService.GetUserBookings(id);

            return View(bookings);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return View();
        }
    }

    public async Task<IActionResult> MonthlyForm(string id)
    {
        try
        {
            var form = await _bookingService.GetBookingFormByUserId(id);

            if (form == null || !form.IsSubmitted)
            {
                throw new Exception("Current user does not have any submitted forms.");
            }

            await _bookingService.UpdateFormWithBookings(id, form);

            var formWithBookings = await _bookingService.GetBookingFormByUserId(id);

            var viewForm = _bookingService.ConvertToViewForm(formWithBookings);

            return View(viewForm);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return RedirectToAction("ShowUsers");
        }
    }
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("ShowUsers"); // Redirect to a method that lists users
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Error"); // Return an error view if delete fails
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ShowAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return View(bookings);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ShowPendingBookings()
    {
        var pendingBookings = await _bookingService.GetPendingBookingsViewModelAsync();
        return View(pendingBookings);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = "Confirmed";
        await _bookingService.UpdateBookingAsync(booking);

        var (totalPrice, totalHours) = await _managerService.GetTotalPriceAndHours();

        ViewBag.TotalPrice = totalPrice;
        ViewBag.TotalHours = totalHours;


        TempData["SuccessMessage"] = $"Booking on {booking.Date.ToShortDateString()} has been confirmed.";

        var pendingBookings = await _bookingService.GetPendingBookingsViewModelAsync();
        return View("ShowPendingBookings", pendingBookings);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RejectBooking(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = "Rejected";
        await _bookingService.UpdateBookingAsync(booking);

        TempData["ErrorMessage"] = $"Booking on {booking.Date.ToShortDateString()} has been rejected.";

        var pendingBookings = await _bookingService.GetPendingBookingsViewModelAsync();
        return View("ShowPendingBookings", pendingBookings);
    }

}