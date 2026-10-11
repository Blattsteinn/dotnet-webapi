// Opening hours are local wall-clock times for the place's own time zone; the
// browser only ever renders what the server sent. OpeningHourDto.Day is the
// .NET day name, so the week is ordered explicitly (Monday first).
export const WEEK_DAYS = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday',
]

// Server format is "HH:mm:ss"; <input type="time"> needs "HH:mm".
export function toTimeInputValue(time) {
  return typeof time === 'string' ? time.slice(0, 5) : ''
}

// <input type="time" value="HH:mm"> travels back as "HH:mm", which TimeOnly
// parses fine, but seconds are appended so the saved payload matches the shape
// the API itself returns.
export function toTimeApiValue(time) {
  if (!time) return ''
  return time.length === 5 ? `${time}:00` : time
}

export function formatInterval(hour) {
  const opens = toTimeInputValue(hour.opensAt)
  const closes = toTimeInputValue(hour.closesAt)
  if (opens === '00:00' && closes === '00:00') return 'Open 24 hours'
  if (closes <= opens) return `${opens} – ${closes} (next day)`
  return `${opens} – ${closes}`
}
