import { useState } from 'react'
import { Link } from 'react-router-dom'
import { createOwnedPlace, listOwnedPlaces } from '../../api/owner.js'
import { HiddenBadge, OpenBadge } from '../../components/Badge.jsx'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Input, Textarea } from '../../components/Field.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'

const EMPTY_FORM = {
  name: '',
  latitude: '',
  longtitude: '',
  description: '',
  address: '',
}

export function OwnerPlacesPage() {
  const { data: places, error, loading, reload } = useApi(() => listOwnedPlaces())
  const [form, setForm] = useState(EMPTY_FORM)
  const [createError, setCreateError] = useState(null)
  const [creating, setCreating] = useState(false)
  const [created, setCreated] = useState(null)
  const [showForm, setShowForm] = useState(false)

  function setField(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  async function handleCreate(event) {
    event.preventDefault()
    setCreating(true)
    setCreateError(null)
    setCreated(null)
    try {
      // Coordinates are required by the API and are sent as numbers.
      const place = await createOwnedPlace({
        name: form.name,
        latitude: Number(form.latitude),
        longtitude: Number(form.longtitude),
        description: form.description || null,
        address: form.address || null,
      })
      setCreated(place)
      setForm(EMPTY_FORM)
      reload()
    } catch (cause) {
      setCreateError(cause)
    } finally {
      setCreating(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading your places…" />
  if (error) return <ErrorMessage error={error} />

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">My places</h1>
        <Button variant="secondary" size="sm" onClick={() => setShowForm((value) => !value)}>
          {showForm ? 'Hide new place form' : 'New place'}
        </Button>
      </div>

      <SuccessMessage>
        {created ? `Place “${created.name}” created (id ${created.id}).` : null}
      </SuccessMessage>

      {showForm && (
        <Card>
          <form onSubmit={handleCreate} className="space-y-3">
            <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
              New place
            </h2>
            <ErrorMessage error={createError} />
            <Input
              id="new-name"
              label="Name"
              required
              maxLength={200}
              value={form.name}
              onChange={(event) => setField('name', event.target.value)}
            />
            <div className="grid gap-3 sm:grid-cols-2">
              <Input
                id="new-latitude"
                label="Latitude"
                type="number"
                step="any"
                min={-90}
                max={90}
                required
                value={form.latitude}
                onChange={(event) => setField('latitude', event.target.value)}
              />
              <Input
                id="new-longtitude"
                label="Longitude"
                type="number"
                step="any"
                min={-180}
                max={180}
                required
                value={form.longtitude}
                onChange={(event) => setField('longtitude', event.target.value)}
              />
            </div>
            <Textarea
              id="new-description"
              label="Description"
              maxLength={500}
              value={form.description}
              onChange={(event) => setField('description', event.target.value)}
            />
            <Textarea
              id="new-address"
              label="Address"
              maxLength={500}
              value={form.address}
              onChange={(event) => setField('address', event.target.value)}
            />
            <Button type="submit" disabled={creating}>
              {creating ? 'Creating…' : 'Create place'}
            </Button>
            <p className="text-xs text-slate-500">
              A new place has no menu and no opening hours, so it is closed until you add them.
            </p>
          </form>
        </Card>
      )}

      {places.length === 0 ? (
        <EmptyState>You do not own any place yet.</EmptyState>
      ) : (
        <ul className="grid gap-4 sm:grid-cols-2">
          {places.map((place) => (
            <li key={place.id}>
              <Card>
                <div className="flex flex-wrap items-center gap-2">
                  <h2 className="font-medium">{place.name}</h2>
                  <HiddenBadge hidden={place.hidden} />
                  <OpenBadge isOpen={place.isOpen} acceptingOrders={place.acceptingOrders} />
                </div>
                {place.address && (
                  <p className="mt-1 text-sm text-slate-600">{place.address}</p>
                )}
                <div className="mt-2 flex gap-3 text-sm">
                  <Link to={`/owner/places/${place.id}`} className="font-medium underline">
                    Edit
                  </Link>
                  <Link to={`/owner/places/${place.id}/hours`} className="underline">
                    Hours
                  </Link>
                  <Link to={`/owner/places/${place.id}/managers`} className="underline">
                    Managers
                  </Link>
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
