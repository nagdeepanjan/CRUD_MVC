﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Entities.DB;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<Country> Countries { get; set; }


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

        //Fluent API
        modelBuilder.Entity<Person>().Property(p => p.TIN).HasColumnName("TaxIdentificationNumber").HasColumnType("varchar(8)");

        modelBuilder.Entity<Person>().HasIndex(p => p.TIN).IsUnique();

        //Fluent API for Table Relationships
        modelBuilder.Entity<Country>()
            .HasMany(c => c.Persons)
            .WithOne(p => p.Country)
            .HasForeignKey(p => p.CountryID)
            .OnDelete(DeleteBehavior.NoAction); //CascadeDeleteBehavior.SetNull
    }

    public List<Person> sp_GetAllPersons()
    {
        return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }

    public int sp_InsertPerson(Person person)
    {
        SqlParameter[] parameters = new SqlParameter[]
        {
            new("@PersonID", person.PersonID),
            new("@PersonName", person.PersonName),
            new("@Email", person.Email),
            new("@DateOfBirth", person.DateOfBirth),
            new("@Gender", person.Gender),
            new("@CountryID", person.CountryID),
            new("@Address", person.Address),
            new("@ReceiveNewsLetters", person.ReceiveNewsLetters)
        };

        return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
    }
}