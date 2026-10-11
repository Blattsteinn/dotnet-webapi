namespace FoodOrderingApi.Models.Places;

// One row per opening interval. May have several rows for the same
// day (lunch and dinner), there is no unique index on (PlaceId, Day).
//
// A row belongs to the day it opens: ClosesAt <= OpensAt means the interval
// runs past midnight into the next day. The single exception is
// 00:00-00:00, which means "open 24 hours".
public class PlaceOpeningHour
{
    public int Id { get; set; }

    public int PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public DayOfWeek Day { get; set; }

    public TimeOnly OpensAt { get; set; }

    public TimeOnly ClosesAt { get; set; }
}
