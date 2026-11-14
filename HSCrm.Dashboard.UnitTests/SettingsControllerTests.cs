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

        [Fact(DisplayName = "3. منوی کناری: منوهای والد بدون زیرمنوی مجاز نباید نمایش داده شوند")]
        public void SidebarMenu_HidesParentGroupsWhenUserHasNoChildPermissions()
        {
            var provider = new SidebarMenuProvider();
            var employeeWithoutAdminPerms = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "User"),
                new Claim("Permission", Permissions.WorkLogs.View),
                new Claim("Permission", Permissions.WorkLogs.Create)
            }, "TestAuth"));

            var menus = provider.GetMenus(employeeWithoutAdminPerms);

            Assert.DoesNotContain(menus, m => m.Key == "users");
            Assert.DoesNotContain(menus, m => m.Key == "customers");
            Assert.DoesNotContain(menus, m => m.Key == "suppliers");
            Assert.DoesNotContain(menus, m => m.Key == "inventories");
            Assert.Contains(menus, m => m.Key == "employee_panel");
            var projectsGroup = Assert.Single(menus, m => m.Key == "projects");
            Assert.Single(projectsGroup.Children, c => c.Key == "project_worklogs");
        }

        [Fact(DisplayName = "4. منوی کناری: منوی کاربران فقط برای نقش Owner قابل مشاهده است")]
        public void SidebarMenu_UsersMenu_RequiresOwnerRole()
        {
            var provider = new SidebarMenuProvider();
            var nonOwnerWithUsersClaim = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "User"),
                new Claim("Permission", Permissions.Users.View),
                new Claim("Permission", Permissions.Roles.View)
            }, "TestAuth"));

            var ownerUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Owner"),
                new Claim("Permission", Permissions.Users.View),
                new Claim("Permission", Permissions.Roles.View)
            }, "TestAuth"));

            Assert.DoesNotContain(provider.GetMenus(nonOwnerWithUsersClaim), m => m.Key == "users");
            var ownerUsersMenu = Assert.Single(provider.GetMenus(ownerUser), m => m.Key == "users");
            Assert.Contains(ownerUsersMenu.Children, c => c.Key == "users_list");
        }

        [Fact(DisplayName = "5. ثابت‌های مجوز داشبورد باید دقیقاً با وب‌سرویس هم‌تراز باشند")]
        public void PermissionsConstants_MatchWebApiNames()
        {
            Assert.Equal("Permissions.Receipt.View", Permissions.Receipt.View);
            Assert.Equal("Permissions.Receipt.Create", Permissions.Receipt.Create);
            Assert.Equal("Permissions.BankAccounts.View", Permissions.BankAccounts.View);
            Assert.Equal("Permissions.Cheques.View", Permissions.Cheques.View);
            Assert.Equal("Permissions.Cheques.ChangeStatus", Permissions.Cheques.ChangeStatus);
            Assert.Equal("Permissions.AccountingAccounts.View", Permissions.AccountingAccounts.View);
            Assert.Equal("Permissions.JournalVouchers.View", Permissions.JournalVouchers.View);
            Assert.Equal("Permissions.AccountingReports.View", Permissions.AccountingReports.View);
            Assert.Equal("Permissions.SalesReturnInvoices.View", Permissions.SalesReturnInvoices.View);
            Assert.Equal("Permissions.PurchaseReturnInvoices.View", Permissions.PurchaseReturnInvoices.View);
        }

        [Fact(DisplayName = "6. کاربر با مجوز payments.view فقط زیرمنوی پرداخت‌ها را می‌بیند نه داشبورد و حسابداری")]
        public void SidebarMenu_PaymentsViewOnly_ShowsOnlyPaymentsMenu()
        {
            var provider = new SidebarMenuProvider();
            var paymentViewer = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "User"),
                new Claim("Permission", Permissions.Payments.View)
            }, "TestAuth"));

            var menus = provider.GetMenus(paymentViewer);

            Assert.DoesNotContain(menus, m => m.Key == "dashboard");
            Assert.DoesNotContain(menus, m => m.Key == "accounting");
            var treasuryGroup = Assert.Single(menus, m => m.Key == "treasury");
            var onlyChild = Assert.Single(treasuryGroup.Children);
            Assert.Equal("payments", onlyChild.Key);
        }
    }
}
