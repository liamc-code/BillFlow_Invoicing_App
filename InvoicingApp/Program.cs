/* Program.cs
* Invoicing App
* Liam Conn
* Entry point for web application
*
*/
using Invoicing.Interfaces;
using InvoicingApp.DataAccess;
using InvoicingApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
var connStr = builder.Configuration.GetConnectionString("InvoicingDB");
builder.Services.AddDbContext<InvoicingDbContext>(options => options.UseSqlServer(connStr));

// Add services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable session middleware
app.UseSession(); 

app.UseRouting();

app.UseAuthorization();

// modified MapController to accomodate group route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{group?}");

app.Run();
