using Entities.DB;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//Add services into IOC container
builder.Services.AddSingleton<ICountriesService, CountriesService>(sp => new CountriesService(true));
builder.Services.AddSingleton<IPersonsService, PersonsService>(sp => new PersonsService(true));

//EF
//DBContext will be injected as a service
builder.Services.AddDbContext<PersonsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();


app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();