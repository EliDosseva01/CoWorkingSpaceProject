using CoWorkingSpace.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CoWorkingSpace.Tests.Controllers
{
    [TestClass]
    public class ManagerControllerTests : Mock
    {
        [TestMethod]
        public async Task ShowAllMadeBookings_Returns_CorrectView()
        {
            //Arrange
            var userId = "someID";

            BookingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            BookingController.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId) // Mock the user ID claim
            }));

            var pageNumber = 3;
            var pageSize = 10;
            int userCount = 3;
            var usersPerPage = new List<IdentityUser>();
            ManagerServiceMock.Setup(x => x.GetAllUsers(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PaginatedList<IdentityUser>(usersPerPage, userCount, pageNumber, pageSize));

            //Act
            var result = await ManagerController.ShowUsers(It.IsAny<int>());

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));
        }
    }
}
