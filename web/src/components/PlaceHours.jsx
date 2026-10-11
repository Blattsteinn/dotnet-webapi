import { WEEK_DAYS, formatInterval, toTimeInputValue } from '../lib/hours.js'

// Opening hours are rendered exactly as the server sent them: local wall-clock
// intervals, several per day allowed, closesAt <= opensAt meaning the interval
// runs past midnight, and 00:00–00:00 meaning open 24 hours.
export function PlaceHours({ hours }) {
  const byDay = new Map(WEEK_DAYS.map((day) => [day, []]))
  for (const hour of hours ?? []) {
    if (byDay.has(hour.day)) byDay.get(hour.day).push(hour)
  }

  return (
    <div>
      {!hours || hours.length === 0 ? (
        <p className="text-sm text-slate-600">
          No opening hours published — this place is currently closed.
        </p>
      ) : (
        <dl className="divide-y divide-slate-100 text-sm">
          {WEEK_DAYS.map((day) => {
            const intervals = byDay.get(day)
            return (
              <div key={day} className="flex justify-between gap-4 py-1.5">
                <dt className="text-slate-600">{day}</dt>
                <dd className="text-right text-slate-800">
                  {intervals.length === 0 ? (
                    <span className="text-slate-400">Closed</span>
                  ) : (
                    <ul>
                      {intervals.map((hour, index) => (
                        <li key={`${toTimeInputValue(hour.opensAt)}-${index}`}>
                          {formatInterval(hour)}
                        </li>
                      ))}
                    </ul>
                  )}
                </dd>
              </div>
            )
          })}
        </dl>
      )}
    </div>
  )
}
