namespace FoodOrderingApi.Services;

// Development-only clock: freezes "now" so ordering can be exercised at any
// hour of the day. Production never registers this — it uses TimeProvider.System.
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
