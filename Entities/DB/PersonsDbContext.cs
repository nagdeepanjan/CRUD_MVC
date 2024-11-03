using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Entities.DB;

public class PersonsDbContext : DbContext
{
    public PersonsDbContext(DbContextOptions<PersonsDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; }
    public DbSet<Country> Countries { get; set; }


    //This allows you to customize the EF Core model by specifying configurations that cannot be expressed using data annotations.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>().ToTable("Countries");
        modelBuilder.Entity<Person>().ToTable("Persons");

        //Seeding
        //modelBuilder.Entity<Country>().HasData(
        //    new Country { CountryID = Guid.NewGuid(), CountryName = "India" },
        //    new Country { CountryID = Guid.NewGuid(), CountryName = "USA" },
        //    new Country { CountryID = Guid.NewGuid(), CountryName = "UK" },
        //    new Country { CountryID = Guid.NewGuid(), CountryName = "Canada" }
        //);

        //Seeding from JSON
        string countriesJson = File.ReadAllText("countries.json");
        var countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);
        countries?.ForEach(c => modelBuilder.Entity<Country>().HasData(c));

        string personsJson = File.ReadAllText("persons.json");
        var persons = JsonSerializer.Deserialize<List<Person>>(personsJson);
        persons?.ForEach(p => modelBuilder.Entity<Person>().HasData(p));
    }

    public List<Person> sp_GetAllPersons()
    {
        return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }
}