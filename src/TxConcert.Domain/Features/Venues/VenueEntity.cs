using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Venues;

public sealed class VenueEntity : BaseEntity
{
    private VenueEntity() { } // EF Core

    public static VenueEntity Create(
        string name,
        string slug,
        string address,
        string city,
        string stateId,
        string venueType,
        string country = "US",
        string? zipCode = null,
        int? capacity = null)
    {
        var entity = new VenueEntity
        {
            Name = name,
            Slug = slug,
            Address = address,
            City = city,
            StateId = stateId,
            Country = country,
            ZipCode = zipCode,
            Capacity = capacity,
            VenueType = venueType
        };
        entity.SetId(Constants.IdPrefix.Venue);
        return entity;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string StateId { get; private set; } = default!;
    public string Country { get; private set; } = "US";
    public string? ZipCode { get; private set; }
    public int? Capacity { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string VenueType { get; private set; } = default!;
    public string? WebsiteUrl { get; private set; }
    public string? PhoneNumber { get; private set; }

    public void SetCoordinates(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetContactInfo(string? websiteUrl, string? phoneNumber)
    {
        WebsiteUrl = websiteUrl;
        PhoneNumber = phoneNumber;
    }

    public void UpdateDetails(
        string name,
        string slug,
        string address,
        string city,
        string stateId,
        string venueType,
        string country,
        string? zipCode,
        int? capacity)
    {
        Name = name;
        Slug = slug;
        Address = address;
        City = city;
        StateId = stateId;
        Country = country;
        ZipCode = zipCode;
        Capacity = capacity;
        VenueType = venueType;
    }
}
