using CoWorkingSpace.Controllers;
using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Infrastructure.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using WorkingSpaceTest.Infrastructure.Data.Common;

namespace CoWorkingSpace.Tests
{
    public class Mock
    {
        public HomeController HomeController { get; }

        public static readonly Mock<IBookingService> BookingServiceMock = new();
        public BookingController BookingController { get; }

        public static readonly Mock<IManagerService> ManagerServiceMock = new();
        public ManagerController ManagerController { get; }

        public static readonly Mock<IRepository> RepositoryMock = new();
        public static readonly Mock<HttpClient> HttpClientMock = new();

        public Mock()
        {
            // Инициализация на UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var identityOptions = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
            var userValidators = new List<IUserValidator<IdentityUser>> { new Mock<IUserValidator<IdentityUser>>().Object };
            var passwordValidators = new List<IPasswordValidator<IdentityUser>> { new Mock<IPasswordValidator<IdentityUser>>().Object };
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<IdentityUser>>>();

            var userManager = new UserManager<IdentityUser>(
                userStoreMock.Object,
                identityOptions.Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                services.Object,
                logger.Object);

            // Инициализация на HomeController (ако има зависимости, те трябва да се добавят тук)
            HomeController = new HomeController();

            // Инициализация на BookingController с Moq обекти
            BookingController = new BookingController(BookingServiceMock.Object)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };

            // Инициализация на ManagerController с ръчно инициализиран UserManager
            ManagerController = new ManagerController(userManager, ManagerServiceMock.Object, BookingServiceMock.Object);
        }
    }
}
