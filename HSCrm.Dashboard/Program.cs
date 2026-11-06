using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Authorization;
using HSCrm.Dashboard.Filters;
using HSCrm.Dashboard.Services;
using HSCrm.Dashboard.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Persist cookie-encryption keys outside the deployment directory.
// Configure DataProtection__KeysPath to a durable folder writable only by this app pool.
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    throw new InvalidOperationException(
        "Production requires DataProtection:KeysPath to point to a persistent, protected directory.");
}

if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("HSCrm.Dashboard");
}

var apiAddress = builder.Configuration["ApiAddress"];
if (string.IsNullOrWhiteSpace(apiAddress) ||
    !apiAddress.EndsWith("/", StringComparison.Ordinal) ||
    !Uri.TryCreate(apiAddress, UriKind.Absolute, out var apiUri) ||
    (apiUri.Scheme != Uri.UriSchemeHttp && apiUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException(
        "API endpoint is not configured correctly. Set ApiAddress to an absolute HTTP(S) API URL ending with / (for example, https://api.example.com/api/).");
}

if (!builder.Environment.IsDevelopment() &&
    (apiUri.Scheme != Uri.UriSchemeHttps || apiUri.IsLoopback))
{
    throw new InvalidOperationException(
        "Production requires ApiAddress to use a public HTTPS API endpoint; localhost and HTTP are not allowed.");
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddTransient<AuthHeaderHandler>();


builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiAddress"]);
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
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
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
