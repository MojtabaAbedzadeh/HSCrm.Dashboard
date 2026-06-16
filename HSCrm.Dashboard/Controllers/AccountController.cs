using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Automation.Dashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILoginService _login;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AccountController(ILoginService login, IMemoryCache cache, IConfiguration config)
        {
            _login = login;
            _cache = cache;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/AdminArea/Home");
            }

            string apiUrl = _config["ApiAddress"] + "FiscalYear/FiscalYearDropdownList";

            GetListApi getList = new GetListApi();

            string jsonFullModel = await getList.GetApiList(apiUrl, "");

            var jsonDataParse = JsonConvert.DeserializeObject<dynamic>(jsonFullModel);

            ViewBag.FiscalsYearList = JsonConvert.DeserializeObject<List<FiscalYearDropDown>>(jsonDataParse.data.ToString());

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = await _login.Login(model);

            if (result == null)
            {
                ViewBag.ErrorMessage = "نام کاربری یا رمز عبور صحیح نیست!";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim("Token", result.Token ?? ""),
                new Claim("TenantId", result.TenantId ?? ""),
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim("UserId", result.UserId.ToString()),
                new Claim(ClaimTypes.Name, result.UserName ?? ""),
                new Claim(ClaimTypes.Email, result.Email ?? ""),
                new Claim("FiscalYearStatus", result.FiscalYearStatus.ToString())
            };

            claims.AddRange(result.Roles.Select(role => new Claim(ClaimTypes.Role, role)));


            // ✅ Roles
            if (result.Roles != null)
            {
                foreach (var role in result.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // ✅ Permissions (مهم برای سیستم تو)
            if (result.Permissions != null)
            {
                foreach (var permission in result.Permissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(6),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            return Redirect("/AdminArea/Home/Index");
        }

        public async Task<IActionResult> Logout()
        {
            _cache.Remove("UserInfo");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
