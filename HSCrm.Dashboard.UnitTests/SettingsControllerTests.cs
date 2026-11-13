using System.Security.Claims;
using HSCrm.Dashboard.Areas.AdminArea.Controllers;
using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HSCrm.Dashboard.UnitTests
{
    public class SettingsControllerTests
    {
        [Fact(DisplayName = "1. تنظیمات شرکت: اکشن Index باید ViewResult برگرداند")]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ApiAddress"]).Returns("https://localhost:7001/api/");

            var controller = new SettingsController(configMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("Token", "fake-jwt-token"),
                            new Claim("TenantId", "1")
                        }, "TestAuth"))
                    }
                }
            };

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact(DisplayName = "2. منوی کناری: آیتم تنظیمات شرکت فقط با مجوز مشاهده شرکت نمایش داده می‌شود")]
        public void SidebarMenu_CompanySettings_RequiresTenantsViewPermission()
        {
            // Arrange
            var provider = new SidebarMenuProvider();
            var authorizedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Permission", Permissions.Tenants.View)
            }, "TestAuth"));
            var unauthorizedUser = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth"));

            // Act
            var authorizedMenus = provider.GetMenus(authorizedUser);
            var unauthorizedMenus = provider.GetMenus(unauthorizedUser);

            // Assert
            var settingsMenu = Assert.Single(authorizedMenus, m => m.Key == "company_settings");
            Assert.Equal("/AdminArea/Settings/Index", settingsMenu.Url);
            Assert.DoesNotContain(unauthorizedMenus, m => m.Key == "company_settings");
        }
    }
}
