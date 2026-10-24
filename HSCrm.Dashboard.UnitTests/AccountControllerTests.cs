using Automation.Dashboard.Controllers;
using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HSCrm.Dashboard.UnitTests
{
    public class AccountControllerTests
    {
        private readonly Mock<ILoginService> _loginServiceMock;
        private readonly Mock<IRegisterService> _registerServiceMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<GetListApi> _getListApiMock;

        public AccountControllerTests()
        {
            _loginServiceMock = new Mock<ILoginService>();
            _registerServiceMock = new Mock<IRegisterService>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _configMock = new Mock<IConfiguration>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _getListApiMock = new Mock<GetListApi>(_httpClientFactoryMock.Object);
        }

        private AccountController CreateController()
        {
            var controller = new AccountController(
                _loginServiceMock.Object,
                _memoryCacheMock.Object,
                _configMock.Object,
                _registerServiceMock.Object,
                _getListApiMock.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact(DisplayName = "1. صفحه لاگین: در صورت قطعی وب‌سرویس سال‌های مالی، صفحه نباید با NullReference کرش کند")]
        public async Task Login_WhenFiscalYearApiFails_DoesNotThrowNullReferenceException()
        {
            // Arrange (API returns empty string)
            _getListApiMock.Setup(x => x.GetApiList("FiscalYear/FiscalYearDropdownList"))
                .ReturnsAsync(string.Empty);

            var controller = CreateController();

            // Act
            var result = await controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(controller.ViewBag.FiscalsYearList);
            var fiscals = Assert.IsAssignableFrom<List<FiscalYearDropDown>>(controller.ViewBag.FiscalsYearList);
            Assert.Empty(fiscals);
        }

        [Fact(DisplayName = "2. بررسی کاربر و سال‌های مالی: اگر کاربر نامعتبر بود، پاسخ خطای JSON مناسب برگردانده شود")]
        public async Task CheckUserAndGetFiscals_WhenUserInvalid_ReturnsErrorJson()
        {
            // Arrange
            _loginServiceMock.Setup(x => x.Login(It.IsAny<LoginModel>()))
                .ReturnsAsync((UserLoginDto?)null);

            var controller = CreateController();

            // Act
            var result = await controller.CheckUserAndGetFiscals("unknown_user", "wrong_pass");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic data = jsonResult.Value;
            Assert.False((bool)data.success);
        }
    }
}
