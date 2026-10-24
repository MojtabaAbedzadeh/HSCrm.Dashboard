using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Areas.AdminArea.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace HSCrm.Dashboard.UnitTests
{
    public class ProductAndSupplierControllerTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<GetListApi> _getListApiMock;

        public ProductAndSupplierControllerTests()
        {
            _configMock = new Mock<IConfiguration>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _getListApiMock = new Mock<GetListApi>(_httpClientFactoryMock.Object);
        }

        [Fact(DisplayName = "1. محصولات: عدم پرتاب NullReferenceException هنگام پاسخ تهی وب‌سرویس")]
        public async Task ProductController_Index_WhenApiFails_DoesNotThrowNullReference()
        {
            // Arrange
            _getListApiMock.Setup(x => x.GetApiList("Product/GetProducts"))
                .ReturnsAsync(string.Empty);

            var controller = new ProductController(_configMock.Object, _getListApiMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ProductModel>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact(DisplayName = "2. تأمین‌کنندگان: عدم پرتاب NullReferenceException هنگام پاسخ تهی وب‌سرویس")]
        public async Task SupplierController_Index_WhenApiFails_DoesNotThrowNullReference()
        {
            // Arrange
            _getListApiMock.Setup(x => x.GetApiList("Supplier/GetSupplier"))
                .ReturnsAsync(string.Empty);

            var controller = new SupplierController(_configMock.Object, _getListApiMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<SupplierModel>>(viewResult.Model);
            Assert.Empty(model);
        }
    }
}
