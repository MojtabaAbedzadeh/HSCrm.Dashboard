using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Authorization;
using HSCrm.Dashboard.Filters;
using HSCrm.Dashboard.Services;
using HSCrm.Dashboard.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddTransient<AuthHeaderHandler>();


builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// ذخیره کلیدهای DataProtection روی دیسک برای جلوگیری از لاگ‌اوت کاربران هنگام Recycle شدن App Pool در IIS
var dataProtectionKeysDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysDir);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDir))
    .SetApplicationName("HSCrm.Dashboard");

builder.Services.AddHttpClient("ApiClient", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var apiAddr = (cfg["ApiAddress"] ?? string.Empty).Trim().TrimEnd('/') + "/";
    client.BaseAddress = new Uri(apiAddr);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRoleApiService, RoleApiService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ISidebarMenuProvider, SidebarMenuProvider>();
builder.Services.AddScoped<GetListApi>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.IsEssential = true;
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
