using CRUDExample.Middleware;
using Entities.DB;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();


//Add services into IOC container
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>(); //(sp => new CountriesService(true));
builder.Services.AddScoped<IPersonsService, PersonsService>(); // (sp => new PersonsService(true));

//EF
//DBContext will be injected as a service
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();
app.Logger.LogInformation("Application Started");


if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); //Build-in to redirect to Error page
    app.UseExceptionHandlingMiddleware(); //Custom Middleware to log errors
}


app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

public partial class Program //Makes the automatically generated Program class accessible programatically
{
}