using System.Text.Json.Serialization;

namespace FoodOrderingApi.Dtos.Places;

// Body for PUT /api/v1/owner/places/{placeId}/hours. The list fully replaces
// the place's weekly schedule; an empty list clears it (the place is then
// closed). Several entries may share a day (lunch and dinner).
public record SetOpeningHoursDto(
    List<OpeningHourDto> Hours
);

// Both the request entry and the response entry: a day plus a local
// wall-clock interval. ClosesAt <= OpensAt means the interval runs past
// midnight; 00:00-00:00 means "open 24 hours".
//
// Day is written as its name ("Monday") rather than a number; the converter
// still accepts a number on the way in. TimeOnly is written as "HH:mm:ss".
public record OpeningHourDto(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    DayOfWeek Day,
    TimeOnly OpensAt,
    TimeOnly ClosesAt
);
