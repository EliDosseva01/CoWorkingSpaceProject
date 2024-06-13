using Microsoft.AspNetCore.Mvc;

namespace CoWorkingSpace.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests : Mock
    {
        [TestMethod]
        public void Index_GetRequest_Returns_CorrectView()
        {
            //Act
            var result = HomeController.Index();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IActionResult));
        }
    }
}