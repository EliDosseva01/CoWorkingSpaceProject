using CoWorkingSpace.Core.Interfaces;
using CoWorkingSpace.Core.Services;
using CoWorkingSpace.Extensions;
using CoWorkingSpace.Infrastructure.Data;
using CoWorkingSpace.Infrastructure.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkingSpaceTest.Infrastructure.Data.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
    connectionOption =>
    connectionOption.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddDbContext<ApplicationDbContext>(option => option.EnableSensitiveDataLogging());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//builder.Services.AddScoped<SignInManager<IdentityUser>>();
//builder.Services.AddScoped<UserManager<IdentityUser>>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddApplicationService();
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
