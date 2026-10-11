using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Services.Places;

// All opening-hours logic lives behind IsOpen so a later holiday/override rule
// has a single place to go. Times are local wall-clock times in the place's
// zone; the zone is named once and resolved through TimeZoneInfo, which is what
// keeps the summer/winter switch (DST) correct.
public class PlaceScheduleService
{
    // The one place the zone is named.
    private const string TimeZoneId = "Europe/Vilnius";

    private static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    private const int MinutesInDay = 24 * 60;

    private readonly TimeProvider _timeProvider;

    public PlaceScheduleService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool IsOpen(Place place) => IsOpen(place, _timeProvider.GetUtcNow());

    // AcceptingOrders gates the whole schedule: flipping it off closes the
    // place at once without touching the rows. Computed on read, never stored.
    public bool IsOpen(Place place, DateTimeOffset utcNow) =>
        place.AcceptingOrders && IsWithinHours(place, utcNow);

    private static bool IsWithinHours(Place place, DateTimeOffset utcNow)
    {
        DateTimeOffset local = TimeZoneInfo.ConvertTime(utcNow, Zone);
        TimeOnly time = TimeOnly.FromTimeSpan(local.TimeOfDay);
        DayOfWeek today = local.DayOfWeek;
        DayOfWeek yesterday =
            today == DayOfWeek.Sunday ? DayOfWeek.Saturday : today - 1;

        foreach (PlaceOpeningHour row in place.OpeningHours)
        {
            if (row.Day == today)
            {
                if (IsTwentyFourHours(row))
                    return true;

                // Normal interval is [OpensAt, ClosesAt).
                if (row.ClosesAt > row.OpensAt)
                {
                    if (time >= row.OpensAt && time < row.ClosesAt)
                        return true;
                }
                // An overnight interval owns the day it opens, so the part
                // before midnight counts as today.
                else if (time >= row.OpensAt)
                {
                    return true;
                }
            }
            // Yesterday's overnight interval reaches into this morning.
            else if (row.Day == yesterday && IsOvernight(row) && time < row.ClosesAt)
            {
                return true;
            }
        }

        return false;
    }

    // Returns null when the schedule is valid, otherwise a caller-facing
    // message. Everything is checked before a single row is written.

    public static string? ValidateSchedule(IReadOnlyList<OpeningHourDto> hours)
    {
        // Each interval is split into single-day segments, so an overnight
        // row's tail is compared against the next day's rows too.
        var byDay = new List<(int Start, int End)>[7];
        for (int i = 0; i < byDay.Length; i++)
            byDay[i] = [];

        foreach (OpeningHourDto hour in hours)
        {
            if (!Enum.IsDefined(hour.Day))
                return $"'{hour.Day}' is not a valid day of the week.";

            int open = ToMinutes(hour.OpensAt);
            int close = ToMinutes(hour.ClosesAt);
            int day = (int)hour.Day;

            // 00:00-00:00 is the one same-time interval that is legal: 24 hours.
            if (open == 0 && close == 0)
            {
                byDay[day].Add((0, MinutesInDay));
                continue;
            }

            if (open == close)
                return $"An interval on {hour.Day} opens and closes at the same "
                     + "time. Only 00:00-00:00 (open 24 hours) may do that.";

            if (close > open)
            {
                byDay[day].Add((open, close));
            }
            else
            {
                // Overnight: [open, midnight) today, [0, close) tomorrow.
                byDay[day].Add((open, MinutesInDay));
                byDay[(day + 1) % 7].Add((0, close));
            }
        }

        for (int day = 0; day < byDay.Length; day++)
        {
            List<(int Start, int End)> segments = byDay[day];
            for (int i = 0; i < segments.Count; i++)
            {
                for (int j = i + 1; j < segments.Count; j++)
                {
                    if (segments[i].Start < segments[j].End &&
                        segments[j].Start < segments[i].End)
                    {
                        return $"Opening intervals overlap on {(DayOfWeek)day}.";
                    }
                }
            }
        }

        return null;
    }

    private static bool IsTwentyFourHours(PlaceOpeningHour row) =>
        row.OpensAt == TimeOnly.MinValue && row.ClosesAt == TimeOnly.MinValue;

    // ClosesAt <= OpensAt (excluding the 24h case) means it runs past midnight.
    private static bool IsOvernight(PlaceOpeningHour row) =>
        !IsTwentyFourHours(row) && row.ClosesAt <= row.OpensAt;

    private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;
}
