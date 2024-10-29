using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//Add services into IOC container
builder.Services.AddSingleton<ICountriesService, CountriesService>(sp => new CountriesService(true));
builder.Services.AddSingleton<IPersonsService, PersonsService>(sp => new PersonsService(true));

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();


app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();