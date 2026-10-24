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
    public class InvoiceControllerTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<GetListApi> _getListApiMock;

        public InvoiceControllerTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["ApiAddress"]).Returns("https://localhost:7001/api/");

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _getListApiMock = new Mock<GetListApi>(_httpClientFactoryMock.Object);
        }

        private InvoiceController CreateControllerWithClaims()
        {
            var controller = new InvoiceController(_configMock.Object, _getListApiMock.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Token", "fake-jwt-token"),
                new Claim("TenantId", "1")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact(DisplayName = "1. فاکتورهای فروش: پاسخ خالی نباید کرش کند و باید مدل خالی برگرداند")]
        public async Task SalesInvoice_WhenApiReturnsEmpty_ReturnsEmptyModelSafely()
        {
            // Arrange
            _getListApiMock.Setup(x => x.GetApiList("SalesInvoice/GetSalesInvoices"))
                .ReturnsAsync(string.Empty);

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.SalesInvoice();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<SalesInvoiceModel>>(viewResult.Model);
            Assert.Empty(model);
            Assert.NotNull(controller.ViewBag.ErrorMessage);
        }

        [Fact(DisplayName = "2. فاکتورهای خرید: فیلترها و مقادیر تهی با موفقیت و بدون خطای Null برمی‌گردند")]
        public async Task PurchaseInvoice_WhenApiReturnsData_ReturnsViewWithData()
        {
            // Arrange
            var invoices = new List<PurchaseInvoiceModel>
            {
                new PurchaseInvoiceModel { Id = 10, Total = 5000000m, Number = "PI-100" }
            };
            var apiResponse = new ApiResponse<List<PurchaseInvoiceModel>>
            {
                Status = true,
                Data = invoices
            };

            _getListApiMock.Setup(x => x.GetApiList(It.Is<string>(s => s.StartsWith("PurchaseInvoice/GetPurchaseInvoices"))))
                .ReturnsAsync(JsonConvert.SerializeObject(apiResponse));

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.PurchaseInvoice(supplierFullName: "آسانسور هگمتانه");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<PurchaseInvoiceModel>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(5000000m, model[0].Total);
        }
    }
}
