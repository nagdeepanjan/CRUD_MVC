﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

public class Person
{
    [Key] public Guid PersonID { get; set; }

    [StringLength(40)] //nvarchar(40)    
    public string? PersonName { get; set; }

    [StringLength(40)] //nvarchar(40)
    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10)] //nvarchar(10)
    public string? Gender { get; set; }

    //unique identifier
    public Guid? CountryID { get; set; }

    [StringLength(200)] //nvarchar(200)
    public string? Address { get; set; }

    //bit
    public bool ReceiveNewsLetters { get; set; }

    public string? TIN { get; set; }

    //Navigation property
    [ForeignKey("CountryID")] public Country? Country { get; set; }
}