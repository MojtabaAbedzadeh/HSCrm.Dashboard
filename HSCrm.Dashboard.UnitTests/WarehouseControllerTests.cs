using System.Security.Claims;
using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Areas.AdminArea.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace HSCrm.Dashboard.UnitTests
{
    public class WarehouseControllerTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<GetListApi> _getListApiMock;

        public WarehouseControllerTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["ApiAddress"]).Returns("https://localhost:7001/api/");

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _getListApiMock = new Mock<GetListApi>(_httpClientFactoryMock.Object);
        }

        private WarehouseController CreateControllerWithClaims()
        {
            var controller = new WarehouseController(_configMock.Object, _getListApiMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Token", "fake-jwt-token"),
                new Claim("TenantId", "1"),
                new Claim(ClaimTypes.Role, "Owner")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact(DisplayName = "1. دریافت موفق انبارها: بازگرداندن ویو همراه با لیست انبارها")]
        public async Task Index_WhenApiResponseIsValid_ReturnsViewWithWarehouses()
        {
            // Arrange
            var expectedList = new List<WarehousesModel>
            {
                new WarehousesModel { Id = 1, Title = "انبار مرکزی", Address = "تهران", IsActive = true },
                new WarehousesModel { Id = 2, Title = "انبار شماره ۲", Address = "کرج", IsActive = true }
            };
            var apiResponse = new ApiResponse<List<WarehousesModel>>
            {
                Status = true,
                Message = "موفق",
                Data = expectedList
            };

            _getListApiMock.Setup(x => x.GetApiList("Warehouse/GetWarehouses"))
                .ReturnsAsync(JsonConvert.SerializeObject(apiResponse));

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehousesModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("انبار مرکزی", model[0].Title);
        }

        [Fact(DisplayName = "2. پاسخ خالی یا نامعتبر از API: عدم پرتاب NullReferenceException و مقداردهی مدل به لیست خالی")]
        public async Task Index_WhenApiResponseIsEmpty_ReturnsViewWithEmptyListAndErrorMessage()
        {
            // Arrange (API returns empty string due to 401/error)
            _getListApiMock.Setup(x => x.GetApiList("Warehouse/GetWarehouses"))
                .ReturnsAsync(string.Empty);

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehousesModel>>(viewResult.Model);
            Assert.Empty(model);
            Assert.NotNull(controller.ViewBag.ErrorMessage);
        }

        [Fact(DisplayName = "3. پاسخ با وضعیت ناموفق از API: نمایش پیام خطا و جلوگیری از کرش")]
        public async Task Index_WhenApiResponseStatusIsFalse_SetsErrorMessage()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WarehousesModel>>
            {
                Status = false,
                Message = "دسترسی شما به انبار مجاز نیست.",
                Data = null
            };

            _getListApiMock.Setup(x => x.GetApiList("Warehouse/GetWarehouses"))
                .ReturnsAsync(JsonConvert.SerializeObject(apiResponse));

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehousesModel>>(viewResult.Model);
            Assert.Empty(model);
            Assert.Equal("دسترسی شما به انبار مجاز نیست.", controller.ViewBag.ErrorMessage);
        }

        [Fact(DisplayName = "4. وقوع استثنا در شبکه: بازگرداندن مدل امن و عدم توقف سیستم")]
        public async Task Index_WhenNetworkExceptionOccurs_HandlesGracefully()
        {
            // Arrange
            _getListApiMock.Setup(x => x.GetApiList(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("خطای شبکه یا قطعی سرور"));

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<WarehousesModel>>(viewResult.Model);
            Assert.Empty(model);
            Assert.Contains("خطای غیرمنتظره", (string)controller.ViewBag.ErrorMessage);
        }
    }
}
