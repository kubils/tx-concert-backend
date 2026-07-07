using Bogus;
using NodaTime;
using TxConcert.Domain.Features.Concerts;

namespace TxConcert.UnitTests.TestSupport.Fakers;

public sealed class TicketmasterEventDtoFaker : Faker<TicketmasterEventDto>
{
    public TicketmasterEventDtoFaker()
    {
        CustomInstantiator(f => new TicketmasterEventDto
        {
            TicketmasterId = $"tm_{f.Random.AlphaNumeric(10)}",
            Name = f.Music.Genre() + " Night",
            Url = f.Internet.Url(),
            EventDate = new LocalDate(2026, f.Random.Int(1, 12), f.Random.Int(1, 28)),
            StartTime = new LocalTime(f.Random.Int(18, 22), 0),
            TimeZone = "America/Chicago",
            ImageUrl = f.Image.PicsumUrl(),
            VenueName = $"{f.Address.City()} Arena",
            VenueAddress = f.Address.StreetAddress(),
            VenueCity = f.Address.City(),
            VenueStateCode = "TX",
            VenueZipCode = f.Address.ZipCode(),
            VenueLatitude = (decimal)f.Address.Latitude(),
            VenueLongitude = (decimal)f.Address.Longitude(),
            PriceMin = f.Random.Decimal(20, 60),
            PriceMax = f.Random.Decimal(80, 200),
            SeatmapUrl = f.Internet.Url(),
            TicketLimitInfo = "Limit 8 per order",
            Attractions =
            [
                new TicketmasterAttractionDto
                {
                    Name = f.Name.FullName(),
                    GenreName = "Rock",
                    SubGenreName = "Alternative Rock",
                    SpotifyUrl = f.Internet.Url(),
                    ImageUrl = f.Image.PicsumUrl()
                }
            ],
            SegmentName = "Music",
            GenreName = "Rock",
            SubGenreName = "Alternative Rock",
            Info = "Age 18+",
            PleaseNote = "No cameras allowed",
            SalesStatus = "onsale"
        });
    }

    public TicketmasterEventDtoFaker InState(string stateCode)
    {
        RuleFor(x => x.VenueStateCode, stateCode);
        return this;
    }

    public TicketmasterEventDtoFaker WithTicketmasterId(string id)
    {
        RuleFor(x => x.TicketmasterId, id);
        return this;
    }

    public TicketmasterEventDtoFaker WithName(string name)
    {
        RuleFor(x => x.Name, name);
        return this;
    }

    public TicketmasterEventDtoFaker WithAttractions(params string[] artistNames)
    {
        RuleFor(x => x.Attractions, _ => artistNames
            .Select(a => new TicketmasterAttractionDto { Name = a, GenreName = "Rock" })
            .ToList());
        return this;
    }

    public TicketmasterEventDtoFaker WithNoAttractions()
    {
        RuleFor(x => x.Attractions, _ => []);
        return this;
    }

    public TicketmasterEventDtoFaker WithoutVenue()
    {
        RuleFor(x => x.VenueName, (string?)null);
        RuleFor(x => x.VenueCity, (string?)null);
        return this;
    }
}
