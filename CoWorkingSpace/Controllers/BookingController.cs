using CoWorkingSpace.Core.Constants;
using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Models;
using CoWorkingSpace.Core.Services;
using CoWorkingSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CoWorkingSpace.Controllers;

[Authorize]
public class BookingController : BaseController
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    ///  </summary>
    /// <returns>Returns the page with form for booking a time period.</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Saves the booking data to the database and calculates the price for the booked hours.
    /// </summary>
    /// <param name="bookingData">The booking object created by filling the booking form.</param>
    /// <returns>A view with all current user bookings.</returns>
    [HttpPost]
    public async Task<IActionResult> AddBooking(BookingViewModel bookingData)
    {
        try
        {
            if (GetLoggedUserId() == null)
            {
                throw new Exception("Please log in first.");
            }

            int bookingPrice = await _bookingService.CalculateBookingPrice(bookingData.StartTime!, bookingData.EndTime!, bookingData.Date!);

            if (bookingPrice < 1)
            {
                ModelState.AddModelError("StartTime", errorMessage: "Time period is invalid.");
                ModelState.AddModelError("EndTime", errorMessage: "Time period is invalid.");
            }

            DateTime currentDateTime = DateTime.Now;
            DateTime bookingDateTime = bookingData.Date.Date + TimeSpan.Parse(bookingData.StartTime!);

            if (bookingData.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Date", errorMessage: "Date cannot be in the past.");
            }

            else if (bookingDateTime < currentDateTime)
            {
                ModelState.AddModelError("StartTime", errorMessage: "Booking time cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                TempData[MessageConstant.ErrorMessage] = "Booking unsuccessful.";
                return View("Index");
            }

            await _bookingService.BookASpace(bookingData, bookingPrice, GetLoggedUserId());

            //TempData[MessageConstant.SuccessMessage] = "Booking successful.";

            return RedirectToAction("MyBookings");
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return View("Index");
        }
    }

    /// <summary>
    /// Shows current user bookings.
    /// </summary>
    /// <param name="pageNumber">Indicates the current page you want to be shown. It is 1 by default. Optional parameter.</param>
    /// <returns>A view with current user bookings.</returns>
    [HttpGet]
    public async Task<IActionResult> MyBookings(int? pageNumber)
    {
        try
        {
            var userId = GetLoggedUserId();
            var bookings = await _bookingService.GetMyBookings(userId, pageNumber ?? 1);

            return View(bookings);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;

            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="id">The id of the specified booking.</param>
    /// <returns>A view with the specified booking information available for editing.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var currentBooking = await _bookingService.GetBookingById(id);

            if (currentBooking == null)
            {
                return NotFound();
            }

            var model = new BookingViewModel
            {
                Id = currentBooking.Id,
                Date = currentBooking.Date,
                StartTime = currentBooking.StartTime,
                EndTime = currentBooking.EndTime,
                Price = currentBooking.Price,
                Status = currentBooking.Status,
                UserId = currentBooking.UserId,
                //UserEmail = currentBooking
            };
            return View(currentBooking);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;

            return View("Index");
        }
    }

    /// <summary>
    /// Edit the booking details and updates it in the database.
    /// </summary>
    /// <param name="id">The id of the specified booking.</param>
    /// <param name="model">the edited booking object.</param>
    /// <returns>
    /// If successful, redirects to current user bookings, else returns to edit booking view with the specified error message.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, BookingViewModel model)
    {
        try
        {
            var bookingPrice = await _bookingService.CalculateBookingPrice(model.StartTime!, model.EndTime!, model.Date);

            if (bookingPrice < 1)
            {
                ModelState.AddModelError("StartTime", errorMessage: "Time period is invalid.");
                ModelState.AddModelError("EndTime", errorMessage: "Time period is invalid.");
            }
            if (!ModelState.IsValid)
            {
                TempData[MessageConstant.ErrorMessage] = "Edit unsuccessful";
                var currentBooking = await _bookingService.GetBookingById(id);

                return View(currentBooking);
            }

            model.Status = "Pending";

            await _bookingService.EditBooking(id, model);

            TempData[MessageConstant.SuccessMessage] = "Edit successful";

            return RedirectToAction("MyBookings");
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return View("Edit");
        }
    }

    /// <summary>
    /// Removes selected booking from the list by setting it isDeleted property to true.
    /// </summary>
    /// <param name="id">The id of the selected booking.</param>
    /// <param name="model"></param>
    /// <returns>Re-renders MyBookings view without deleted booking.</returns>
    [HttpPost]
    public async Task<IActionResult> Delete(int id, BookingViewModel model)
    {
        try
        {
            await _bookingService.Delete(id, User);

            (decimal totalPrice, double totalHours) = await _bookingService.GetTotalPriceAndHours();
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalHours = totalHours;


            if (User.IsInRole("Admin"))
            {
                TempData[MessageConstant.SuccessMessage] = "Booking deleted successfully.";
                return RedirectToAction("UserBookings", "Manager", new { id = model.UserId });
            }

            TempData[MessageConstant.SuccessMessage] = "Booking deleted successfully.";
            return RedirectToAction("MyBookings");
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return RedirectToAction("MyBookings");
        }
    }

    /// <summary>
    /// Creates form for the logged in user bookings for previous month.
    /// This method can be accessed only between 1st and 5th of the month and creates form for previous month bookings.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">
    /// Trow error if trying to create form and there are no bookings for the specified month or
    /// if the form is already created.
    /// </exception>>
    public async Task<IActionResult> CreateForm()
    {
        try
        {
            var userId = GetLoggedUserId();

            var previousMonthBookings = await _bookingService.GetPreviousMonthBookings(userId);

            if (previousMonthBookings.Count == 0)
            {
                throw new Exception("There are no bookings for previous month.");
            }

            var existingForm = await _bookingService.GetBookingFormByUserId(userId);

            if (existingForm != null && existingForm.Month == DateTime.Now.Month - 1)
            {
                throw new Exception("Form is already created.");
            }

            await _bookingService.CreateForm(userId);

            TempData[MessageConstant.SuccessMessage] = "Form created.";

            return RedirectToAction("MyBookings");
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return RedirectToAction("MyBookings");
        }
    }

    /// <summary>
    /// Shows the monthly form of the currently logged in user for the previous month.
    /// </summary>
    /// <exception cref="Exception">Throw error if no bookings are made or if the form is still not created.</exception>
    /// <returns></returns>
    public async Task<IActionResult> MonthlyForm()
    {
        try
        {
            var userId = GetLoggedUserId();

            var form = await _bookingService.GetBookingFormByUserId(userId);

            if (form == null)
            {
                return View();
            }

            await _bookingService.UpdateFormWithBookings(userId, form);

            var formWithBookings = await _bookingService.GetBookingFormByUserId(userId);

            var viewForm = _bookingService.ConvertToViewForm(formWithBookings);

            return View(viewForm);
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return RedirectToAction("MyBookings");
        }
    }

    //forms
    public async Task<IActionResult> SubmitForm()
    {
        try
        {
            var userId = GetLoggedUserId();
            var bookingForm = await _bookingService.GetBookingFormByUserId(userId);

            if (bookingForm == null)
            {
                throw new Exception("Invalid form ID.");
            }

            if (bookingForm.IsSubmitted)
            {
                throw new Exception("The form has already been submitted.");
            }

            await _bookingService.SubmitForm(bookingForm);

            TempData[MessageConstant.SuccessMessage] = "Form successfully submitted.";
            return RedirectToAction("MonthlyForm");
        }
        catch (Exception error)
        {
            TempData[MessageConstant.ErrorMessage] = error.Message;
            return RedirectToAction("MonthlyForm");
        }
    }

}