using CoWorkingSpace.Controllers;
using CoWorkingSpace.Core.Constants;
using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Models;
using CoWorkingSpace.Core.Services;
using CoWorkingSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Drawing.Printing;
using System.Security.Claims;
using WorkingSpaceTest.Infrastructure.Data.Common;

namespace CoWorkingSpace.Tests.Controllers
{
    [TestClass]
    public class BookingControllerTests : Mock
    {

        [TestMethod]
        public async Task AddBooking_SuccessfullyAddsBookingAndRedirects()
        {

            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var userId = "user123";

            BookingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            BookingController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId) // Mock the user ID claim
            }));

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            // Create an instance of ITempDataDictionary mock
            var tempDataMock = new Mock<ITempDataDictionary>();

            // Set up TempData values
            var tempDataDictionaryMock = new Mock<ITempDataDictionary>();
            tempDataDictionaryMock.SetupSet(tempData => tempData[MessageConstant.SuccessMessage] = "Booking successful.").Verifiable();

            // Arrange
            var bookingData = new BookingViewModel
            {
                Id = 1,
                Date = new DateTime(2024, 06, 22) /*new DateTime(2024, 06, 22)*/,
                StartTime = TimeSpan.Parse("08:00").ToString(),
                EndTime = TimeSpan.Parse("09:00").ToString(),
                Price = 15,
                UserId = "user123"
            };
            var bookingPrice = 10;

            BookingServiceMock.Setup(x => x.CalculateBookingPrice(bookingData.StartTime, bookingData.EndTime, bookingData.Date))
                .ReturnsAsync(bookingPrice);
            BookingServiceMock.Setup(x => x.BookASpace(bookingData, bookingPrice, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act

            var result = await BookingController.AddBooking(bookingData);

            // Assert

            Assert.AreEqual("MyBookings", (result as RedirectToActionResult)?.ActionName);
        }

        [TestMethod]
        public async Task AddBooking_UnSuccessfullyAddsBookingAndRedirects()
        {

            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var userId = "user123";

            BookingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            BookingController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId) // Mock the user ID claim
            }));

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            // Create an instance of ITempDataDictionary mock
            var tempDataMock = new Mock<ITempDataDictionary>();

            // Set up TempData values
            var tempDataDictionaryMock = new Mock<ITempDataDictionary>();
            tempDataDictionaryMock.SetupSet(tempData => tempData[MessageConstant.SuccessMessage] = "Booking successful.").Verifiable();

            // Arrange
            var bookingData = new BookingViewModel
            {
                Id = 1,
                Date = new DateTime(2023, 05, 22),
                StartTime = TimeSpan.Parse("08:00").ToString(),
                EndTime = TimeSpan.Parse("09:00").ToString(),
                Price = 15,
                UserId = "user123"
            };
            var bookingPrice = 10;

            BookingServiceMock.Setup(x => x.CalculateBookingPrice(bookingData.StartTime, bookingData.EndTime, bookingData.Date))
                .ReturnsAsync(bookingPrice);
            BookingServiceMock.Setup(x => x.BookASpace(bookingData, bookingPrice, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act

            var result = await BookingController.AddBooking(bookingData);

            // Assert

            Assert.AreEqual("Index", (result as ViewResult)?.ViewName);
        }

        [TestMethod]
        public void Index_Returns_CorrectView()
        {
            //Act
            var result = BookingController.Index() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));

        }

        [TestMethod]
        public async Task Test_BookASpace_Return_True()
        {
            string userId = "user123";

            var bookingData = new BookingViewModel
            {
                Id = 1,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.Parse("08:00").ToString(),
                EndTime = TimeSpan.Parse("09:00").ToString(),
                UserId = userId
            };
            var price = 10;

            BookingServiceMock.Setup(x => x.BookASpace(bookingData, price, userId));
            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            // Act
            var result = await bookingService.BookASpace(bookingData, price, userId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task AddBooking_ShouldNotAddToDatabase_MissingData()
        {
            //Arrange           
            BookingController.ModelState.AddModelError("Date", "Date is required!");

            var bookingWithoutDate = new BookingViewModel
            {
                StartTime = "12:00",
                EndTime = "13:00",
                Price = 10
            };

            //Act
            await BookingController.AddBooking(bookingWithoutDate);

            //Assert
            BookingServiceMock.Verify(x =>
                x.BookASpace(It.IsAny<BookingViewModel>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task AddBooking_ShouldNotAddToDatabase_IfIncorrectTimePeriod()
        {
            //Arrange           
            BookingController.ModelState.AddModelError("StartTime", "Invalid time period.");
            BookingController.ModelState.AddModelError("EndTime", "Invalid time period.");

            var bookingWithoutDate = new BookingViewModel
            {
                Date = new DateTime(2023, 05, 22),
                StartTime = "15:00",
                EndTime = "10:00",
                Price = 10
            };

            //Act
            await BookingController.AddBooking(bookingWithoutDate);

            //Assert
            BookingServiceMock.Verify(x =>
                    x.BookASpace(It.IsAny<BookingViewModel>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public async Task AddBooking_ShouldNotAddToDatabase_IfNoLoggedUser()
        {
            //Arrange   

            var booking = new BookingViewModel()
            {
                Date = new DateTime(2023, 05, 16),
                StartTime = "12:00",
                EndTime = "13:00",
                Price = 10,
            };
            //Act
            await BookingController.AddBooking(booking);

            //Assert
            BookingServiceMock.Verify(x =>
                x.BookASpace(It.IsAny<BookingViewModel>(), It.IsAny<int>(), null), Times.Never);
        }

        [TestMethod]
        public async Task Calculate_Booking_Price()
        {
            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            var userId = "user123";
            int bookingId = 1;

            var booking = new BookingViewModel
            {
                Id = bookingId,
                Date = new DateTime(2023, 09, 07),
                StartTime = "13:00",
                EndTime = "14:00",
                UserId = userId,
            };

            // Act
            var calcPrice = await bookingService.CalculateBookingPrice(booking.StartTime, booking.EndTime, booking.Date);


            Assert.AreEqual(calcPrice, 10);
        }

        [TestMethod]
        public async Task Calculate_Booking_Price_With_Holiday_Day()
        {
            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            var userId = "user123";
            int bookingId = 1;

            var booking = new BookingViewModel
            {
                Id = bookingId,
                Date = new DateTime(2023, 09, 06),
                StartTime = "13:00",
                EndTime = "14:00",
                UserId = userId,
            };

            // Act
            var calcPrice = await bookingService.CalculateBookingPrice(booking.StartTime, booking.EndTime, booking.Date);

            //Assert
            Assert.AreEqual(calcPrice, 15);
        }

        [TestMethod]
        public async Task AddBooking_ShouldReturnFalse_If_TimePeriod_Is_NegativeOrZero()
        {
            //Arrange   
            var bookingWithNegativeTime = new BookingViewModel
            {
                Date = new DateTime(2023, 05, 16),
                StartTime = "14:00",
                EndTime = "13:00",
                Price = 10
            };

            var bookingWithZeroHours = new BookingViewModel()
            {
                Date = new DateTime(2023, 05, 16),
                StartTime = "14:00",
                EndTime = "14:00",
                Price = 10
            };
            //Act
            var resultWhenNegativeTime = await BookingServiceMock.Object.BookASpace
                (
                bookingWithNegativeTime,
                It.IsAny<int>(),
                It.IsAny<string>()
                );
            var resultWhenZeroTime = await BookingServiceMock.Object.BookASpace
                (
                bookingWithZeroHours,
                It.IsAny<int>(),
                It.IsAny<string>()
                );

            //Assert
            Assert.AreEqual(resultWhenNegativeTime, false);
            Assert.AreEqual(resultWhenZeroTime, false);
            BookingServiceMock.Verify(x =>
                x.BookASpace(It.IsAny<BookingViewModel>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Exactly(2));
        }

        [TestMethod]
        public async Task MyBookings_ReturnsPaginatedListOfBookings()
        {
            // Arrange
            var userId = "user123";
            var pageNumber = 1;
            var pageSize = 10;

            BookingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            BookingController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId) // Mock the user ID claim
            }));

            var bookings = new List<BookingViewModel>
            {
                new BookingViewModel { Id = 1, Date = DateTime.Now, StartTime = "10:00", EndTime = "12:00", Price = 50, UserId = userId },
                new BookingViewModel { Id = 2, Date = DateTime.Now.AddDays(1), StartTime = "14:00", EndTime = "16:00", Price = 60, UserId = userId }
            };

            BookingServiceMock.Setup(x => x.GetMyBookings(userId, pageNumber, pageSize))
                .ReturnsAsync(new PaginatedList<BookingViewModel>(bookings, bookings.Count, pageNumber, pageSize));

            // Act
            var result = await BookingController.MyBookings(pageNumber);
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as PaginatedList<BookingViewModel>;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.IsNotNull(model);
            Assert.AreEqual(bookings.Count, model.Count);
        }

        [TestMethod]
        public async Task GetBookingById_ReturnsBookingById()
        {
            // Arrange
            var bookingId = 1;

            var booking = new BookingViewModel
            {
                Id = bookingId,
                Date = DateTime.Now,
                StartTime = "10:00",
                EndTime = "12:00",
                Price = 50
            };

            BookingServiceMock.Setup(x => x.GetBookingById(bookingId)).ReturnsAsync(booking);

            // Act
            var result = await BookingController.Edit(bookingId);
            var viewResult = result as ViewResult;
            var model = viewResult?.Model as BookingViewModel;

            // Assert
            Assert.IsNotNull(viewResult);
            Assert.IsNotNull(model);
            Assert.AreEqual(bookingId, model.Id);
        }

        [TestMethod]
        public async Task EditBooking_UpdatesBookingSuccessfully()
        {
            // Arrange
            var bookingId = 1;

            var booking = new Booking
            {
                Id = bookingId,
                Date = DateTime.Now.AddDays(1), // Ensure the date is in the future
                StartTime = new TimeSpan(10, 00, 00),
                EndTime = new TimeSpan(13, 00, 00),
                Price = 30
            };

            var model = new BookingViewModel
            {
                Id = bookingId,
                Date = DateTime.Now.AddDays(2), // Ensure the date is in the future
                StartTime = "11:00",
                EndTime = "13:00",
                Price = 20,
            };

            // Настройка на мок обектите
            RepositoryMock.Setup(x => x.GetByIdAsync<Booking>(bookingId)).ReturnsAsync(booking);
            RepositoryMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            BookingServiceMock.Setup(x => x.EditBooking(bookingId, model)).Returns(Task.CompletedTask);
            BookingServiceMock.Setup(x => x.CalculateBookingPrice(model.StartTime!, model.EndTime!, model.Date))
                              .ReturnsAsync(20); // ensure the booking price is valid

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "user123")
            }, "mock"));

            BookingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await BookingController.Edit(bookingId, model);
            var redirectToActionResult = result as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual(nameof(BookingController.MyBookings), redirectToActionResult?.ActionName);
        }

        [TestMethod]
        public async Task Test_Edit_Booking()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository>();
            var httpMock = new Mock<HttpClient>();

            var bookingService = new BookingService(repositoryMock.Object, httpMock.Object);

            int bookingId = 1;
            var model = new BookingViewModel
            {
                Id = bookingId,
                Date = DateTime.Now.AddDays(1).Date, // Ensure the date is in the future
                StartTime = "10:00",
                EndTime = "12:00",
                Price = 20,
            };

            Booking booking = new Booking
            {
                Id = bookingId,
                Date = DateTime.Now.AddDays(2).Date, // Original date also needs to be in the future
                StartTime = TimeSpan.Parse(model.StartTime),
                EndTime = TimeSpan.Parse(model.EndTime),
                Price = 0
            };

            repositoryMock
                .Setup(x => x.GetByIdAsync<Booking>(bookingId))
                .ReturnsAsync(booking);

            repositoryMock
                .Setup(x => x.SaveChangesAsync());

            await bookingService.EditBooking(bookingId, model);


            Assert.AreEqual(model.Date.Date, booking.Date);
            Assert.AreEqual(model.Price, booking.Price);
        }
    }
}