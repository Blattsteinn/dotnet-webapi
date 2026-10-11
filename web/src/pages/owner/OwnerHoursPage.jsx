import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getOwnedPlace, replaceOpeningHours } from '../../api/owner.js'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Input, Select } from '../../components/Field.jsx'
import { OwnerPlaceTabs } from '../../components/OwnerPlaceTabs.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { WEEK_DAYS, toTimeApiValue, toTimeInputValue } from '../../lib/hours.js'
import { newKey } from '../../lib/placeGraph.js'

function toRows(hours) {
  return (hours ?? []).map((hour) => ({
    key: newKey(),
    day: hour.day,
    opensAt: toTimeInputValue(hour.opensAt),
    closesAt: toTimeInputValue(hour.closesAt),
  }))
}

export function OwnerHoursPage() {
  const { id } = useParams()
  const { data: place, error, loading, reload } = useApi(() => getOwnedPlace(id), [id])
  const [rows, setRows] = useState([])
  const [saveError, setSaveError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    if (place) setRows(toRows(place.openingHours))
  }, [place])

  function updateRow(key, patch) {
    setRows((current) =>
      current.map((row) => (row.key === key ? { ...row, ...patch } : row)),
    )
  }

  function addRow() {
    setRows((current) => [
      ...current,
      { key: newKey(), day: 'Monday', opensAt: '09:00', closesAt: '17:00' },
    ])
  }

  async function handleSave(event) {
    event.preventDefault()
    setSaving(true)
    setSaveError(null)
    setSaved(false)
    try {
      // Full replace of the weekly schedule: whatever is on screen is what the
      // place ends up with. The server validates every interval (the 24h rule,
      // the overnight rule and overlaps) and rejects a bad schedule with 400.
      await replaceOpeningHours(
        id,
        rows.map((row) => ({
          day: row.day,
          opensAt: toTimeApiValue(row.opensAt),
          closesAt: toTimeApiValue(row.closesAt),
        })),
      )
      setSaved(true)
      reload()
    } catch (cause) {
      setSaveError(cause)
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading opening hours…" />
  if (error) {
    return (
      <div className="space-y-3">
        <ErrorMessage error={error} />
        <Link to="/owner/places" className="text-sm text-slate-700 underline">
          Back to my places
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div>
        <Link to="/owner/places" className="text-sm text-slate-600 hover:underline">
          ← My places
        </Link>
        <h1 className="mt-2 text-2xl font-semibold">{place.name}</h1>
      </div>

      <OwnerPlaceTabs />
      <ErrorMessage error={saveError} />
      <SuccessMessage>{saved ? 'Opening hours saved.' : null}</SuccessMessage>

      <Card>
        <form onSubmit={handleSave} className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
              Weekly schedule
            </h2>
            <div className="flex gap-2">
              <Button variant="secondary" size="sm" onClick={addRow}>
                Add interval
              </Button>
              <Button variant="ghost" size="sm" onClick={() => setRows([])}>
                Remove all
              </Button>
            </div>
          </div>

          <p className="rounded-md bg-slate-50 p-3 text-xs text-slate-600">
            Times are local wall-clock. An interval whose close time is not after its
            open time runs past midnight into the next day, and 00:00–00:00 means open
            24 hours. Several intervals per day are allowed; a place with no intervals
            is closed.
          </p>

          {rows.length === 0 ? (
            <p className="text-sm text-slate-600">
              No intervals — the place is always closed.
            </p>
          ) : (
            <ul className="space-y-2">
              {rows.map((row) => (
                <li key={row.key} className="flex flex-wrap items-end gap-3">
                  <Select
                    id={`day-${row.key}`}
                    label="Day"
                    className="w-40"
                    value={row.day}
                    onChange={(event) => updateRow(row.key, { day: event.target.value })}
                  >
                    {WEEK_DAYS.map((day) => (
                      <option key={day} value={day}>
                        {day}
                      </option>
                    ))}
                  </Select>
                  <Input
                    id={`opens-${row.key}`}
                    label="Opens"
                    type="time"
                    required
                    className="w-32"
                    value={row.opensAt}
                    onChange={(event) => updateRow(row.key, { opensAt: event.target.value })}
                  />
                  <Input
                    id={`closes-${row.key}`}
                    label="Closes"
                    type="time"
                    required
                    className="w-32"
                    value={row.closesAt}
                    onChange={(event) => updateRow(row.key, { closesAt: event.target.value })}
                  />
                  <Button
                    variant="ghost"
                    size="sm"
                    className="mb-1"
                    onClick={() =>
                      setRows((current) => current.filter((item) => item.key !== row.key))
                    }
                  >
                    Remove
                  </Button>
                </li>
              ))}
            </ul>
          )}

          <Button type="submit" disabled={saving}>
            {saving ? 'Saving…' : 'Save opening hours'}
          </Button>
        </form>
      </Card>
    </div>
  )
}
