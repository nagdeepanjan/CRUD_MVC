namespace Entities;

/// <summary>
///     Domain model for storing country details
/// </summary>
public class Country
{
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }
}