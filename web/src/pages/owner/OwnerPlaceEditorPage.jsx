import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getOwnedPlace, updateOwnedPlace } from '../../api/owner.js'
import { getProduct } from '../../api/places.js'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Checkbox, Input, Textarea } from '../../components/Field.jsx'
import { OwnerPlaceTabs } from '../../components/OwnerPlaceTabs.jsx'
import { PlaceGraphEditor } from '../../components/PlaceGraphEditor.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { toEditorCategories, toUpdatePayload } from '../../lib/placeGraph.js'

// The place response stops at the product, so the add-on tree is loaded from the
// public product endpoint, one call per product. A 404 there means the product
// (or its category) is hidden, which the editor records as "unavailable".
async function loadEditorState(placeId) {
  const place = await getOwnedPlace(placeId)
  const addOnsByProductId = new Map()

  await Promise.all(
    place.productCategories.flatMap((category) =>
      category.products.map(async (product) => {
        try {
          const detail = await getProduct(category.id, product.id)
          addOnsByProductId.set(product.id, {
            unavailable: false,
            addOnCategories: detail.productAddOnCategories,
          })
        } catch (cause) {
          if (cause?.status !== 404) throw cause
          addOnsByProductId.set(product.id, { unavailable: true, addOnCategories: [] })
        }
      }),
    ),
  )

  return { place, addOnsByProductId }
}

export function OwnerPlaceEditorPage() {
  const { id } = useParams()
  const { data, error, loading, reload } = useApi(() => loadEditorState(id), [id])
  const [form, setForm] = useState(null)
  const [saveError, setSaveError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

  // Rebuild the editable copy whenever a fresh load completes (including after a
  // save, so the new ids come back from the server).
  useEffect(() => {
    if (!data) return
    setForm({
      name: data.place.name,
      description: data.place.description ?? '',
      address: data.place.address ?? '',
      latitude: data.place.latitude,
      longtitude: data.place.longtitude,
      hidden: data.place.hidden,
      acceptingOrders: data.place.acceptingOrders,
      productCategories: toEditorCategories(data.place, data.addOnsByProductId),
    })
  }, [data])

  function setField(field, value) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  async function handleSave(event) {
    event.preventDefault()
    setSaving(true)
    setSaveError(null)
    setSaved(false)
    try {
      await updateOwnedPlace(id, toUpdatePayload(form))
      setSaved(true)
      reload()
    } catch (cause) {
      setSaveError(cause)
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading place…" />
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
  if (!form) return null

  return (
    <div className="space-y-4">
      <div>
        <Link to="/owner/places" className="text-sm text-slate-600 hover:underline">
          ← My places
        </Link>
        <h1 className="mt-2 text-2xl font-semibold">{data.place.name}</h1>
      </div>

      <OwnerPlaceTabs />
      <ErrorMessage error={saveError} />
      <SuccessMessage>{saved ? 'Place saved.' : null}</SuccessMessage>

      <form onSubmit={handleSave} className="space-y-4">
        <Card className="space-y-3">
          <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
            Details
          </h2>

          <Input
            id="place-name"
            label="Name"
            required
            maxLength={200}
            value={form.name}
            onChange={(event) => setField('name', event.target.value)}
          />

          <div className="grid gap-3 sm:grid-cols-2">
            <Input
              id="place-latitude"
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
              id="place-longtitude"
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
            id="place-description"
            label="Description"
            maxLength={500}
            value={form.description}
            onChange={(event) => setField('description', event.target.value)}
          />
          <Textarea
            id="place-address"
            label="Address"
            maxLength={500}
            value={form.address}
            onChange={(event) => setField('address', event.target.value)}
          />

          <div className="flex flex-wrap gap-6">
            <Checkbox
              id="place-hidden"
              label="Hidden from the public list"
              checked={Boolean(form.hidden)}
              onChange={(event) => setField('hidden', event.target.checked)}
            />
            {/* Accepting orders is the manual "closed right now" switch; it is
                sent explicitly so saving the form cannot flip it by accident. */}
            <Checkbox
              id="place-accepting"
              label="Accepting orders"
              checked={Boolean(form.acceptingOrders)}
              onChange={(event) => setField('acceptingOrders', event.target.checked)}
            />
          </div>
        </Card>

        <Card>
          <PlaceGraphEditor
            categories={form.productCategories}
            setCategories={(updater) =>
              setForm((current) => ({
                ...current,
                productCategories:
                  typeof updater === 'function'
                    ? updater(current.productCategories)
                    : updater,
              }))
            }
          />
        </Card>

        <Button type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save place'}
        </Button>
      </form>
    </div>
  )
}
