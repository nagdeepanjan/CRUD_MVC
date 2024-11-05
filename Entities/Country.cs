using System.ComponentModel.DataAnnotations;

namespace Entities;

/// <summary>
///     Domain model for storing country details
/// </summary>
public class Country
{
    [Key] public Guid CountryID { get; set; }

    public string? CountryName { get; set; }

    //Navigation property
    public virtual ICollection<Person>? Persons { get; set; }
}