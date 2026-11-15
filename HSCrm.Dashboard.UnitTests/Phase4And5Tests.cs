using System.Reflection;
using System.Security.Claims;
using Automation.Dashboard.Controllers;
using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Areas.AdminArea.Controllers;
using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Services;
using HSCrm.Dashboard.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HSCrm.Dashboard.UnitTests
{
    public class Phase4And5Tests
    {
        [Fact(DisplayName = "1. پرتال مشتری: کاربر با نقش Customer هنگام ورود/لاگین به /UserArea/Home/Index هدایت می‌شود")]
        public async Task AccountLogin_WhenAuthenticatedCustomer_RedirectsToUserArea()
        {
            var controller = new AccountController(
                new Mock<ILoginService>().Object,
                new Mock<IMemoryCache>().Object,
                new Mock<IConfiguration>().Object,
                new Mock<IRegisterService>().Object,
                new Mock<GetListApi>(new Mock<IHttpClientFactory>().Object).Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "cust-1"),
                            new Claim(ClaimTypes.Role, "Customer")
                        }, "TestAuth"))
                    }
                }
            };

            var result = await controller.Login();
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/UserArea/Home/Index", redirect.Url);
        }

        [Fact(DisplayName = "2. ایزوله‌سازی پنل: مشتری با نقش Customer نمی‌تواند وارد AdminArea شود")]
        public void AdminAreaControllers_RedirectCustomerRoleToUserArea()
        {
            var configMock = new Mock<IConfiguration>();
            var customerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "cust-1"),
                new Claim(ClaimTypes.Role, "Customer")
            }, "TestAuth"));

            var home = new HomeController(configMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = customerPrincipal } }
            };
            var emp = new EmployeeController(configMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = customerPrincipal } }
            };

            var r1 = Assert.IsType<RedirectResult>(home.Index());
            var r2 = Assert.IsType<RedirectResult>(emp.Index());
            Assert.Equal("/UserArea/Home/Index", r1.Url);
            Assert.Equal("/UserArea/Home/Index", r2.Url);
        }

        [Fact(DisplayName = "3. پرتال مشتری: تمام کنترلرهای UserArea دارای Area(UserArea) و Authorize(Roles = Customer) هستند")]
        public void UserAreaControllers_RequireCustomerRole()
        {
            var types = new[]
            {
                typeof(HSCrm.Dashboard.Areas.UserArea.Controllers.HomeController),
                typeof(HSCrm.Dashboard.Areas.UserArea.Controllers.ProjectController),
                typeof(HSCrm.Dashboard.Areas.UserArea.Controllers.InvoiceController),
                typeof(HSCrm.Dashboard.Areas.UserArea.Controllers.StatementController),
                typeof(HSCrm.Dashboard.Areas.UserArea.Controllers.ProfileController)
            };

            foreach (var t in types)
            {
                var areaAttr = t.GetCustomAttribute<AreaAttribute>();
                var authAttrs = t.GetCustomAttributes<AuthorizeAttribute>(inherit: false).ToList();
                Assert.NotNull(areaAttr);
                Assert.Equal("UserArea", areaAttr!.RouteValue);
                Assert.Contains(authAttrs, a => string.Equals(a.Roles, "Customer", StringComparison.Ordinal));
            }
        }

        [Fact(DisplayName = "4. ممیزی و تاریخچه تغییرات: AuditTrailController و آیتم منوی کناری با مجوز auditTrail.view هم‌تراز هستند")]
        public void AuditTrailControllerAndSidebarMenu_RequireAuditTrailViewPermission()
        {
            var authAttrs = typeof(AuditTrailController).GetCustomAttributes<AuthorizeAttribute>(inherit: false).ToList();
            Assert.Contains(authAttrs, a => string.Equals(a.Policy, "Permission:" + Permissions.AuditTrail.View, StringComparison.Ordinal));

            var configMock = new Mock<IConfiguration>();
            var ctrl = new AuditTrailController(configMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim("Permission", Permissions.AuditTrail.View)
                        }, "TestAuth"))
                    }
                }
            };
            Assert.IsType<ViewResult>(ctrl.Index());

            var provider = new SidebarMenuProvider();
            var authorizedMenus = provider.GetMenus(ctrl.User);
            var unauthorizedMenus = provider.GetMenus(new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth")));

            var auditMenu = Assert.Single(authorizedMenus, m => m.Key == "audit_trail");
            Assert.Equal("/AdminArea/AuditTrail/Index", auditMenu.Url);
            Assert.DoesNotContain(unauthorizedMenus, m => m.Key == "audit_trail");
        }
    }
}
