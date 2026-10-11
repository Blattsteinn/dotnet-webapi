import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getPlace } from '../../api/places.js'
import { OpenBadge } from '../../components/Badge.jsx'
import { Card } from '../../components/Card.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { OrderCartPanel } from '../../components/OrderCartPanel.jsx'
import { OrderItemEditor } from '../../components/OrderItemEditor.jsx'
import { PlaceHours } from '../../components/PlaceHours.jsx'
import { PlaceMenu } from '../../components/PlaceMenu.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'

function lineKey(line) {
  const addOns = line.addOns
    .map((addOn) => `${addOn.addOnId}x${addOn.quantity}`)
    .sort()
    .join(',')
  return `${line.productId}|${addOns}`
}

export function PlacePage() {
  const { id } = useParams()
  const { data: place, error, loading } = useApi(() => getPlace(id), [id])
  const [cart, setCart] = useState([])
  const [selection, setSelection] = useState(null)

  function addLine(line) {
    const key = lineKey(line)
    setCart((current) => {
      const existing = current.find((candidate) => candidate.key === key)
      if (existing) {
        return current.map((candidate) =>
          candidate.key === key
            ? { ...candidate, quantity: candidate.quantity + line.quantity }
            : candidate,
        )
      }
      return [...current, { ...line, key }]
    })
    setSelection(null)
  }

  if (loading) return <LoadingBlock label="Loading place…" />

  if (error) {
    return (
      <div className="space-y-3">
        <ErrorMessage error={error} />
        <Link to="/" className="text-sm text-slate-700 underline">
          Back to places
        </Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <Link to="/" className="text-sm text-slate-600 hover:underline">
          ← All places
        </Link>
        <div className="mt-2 flex flex-wrap items-center gap-3">
          <h1 className="text-2xl font-semibold">{place.name}</h1>
          <OpenBadge isOpen={place.isOpen} acceptingOrders={place.acceptingOrders} />
        </div>
        {place.address && <p className="text-sm text-slate-600">{place.address}</p>}
        {place.description && (
          <p className="mt-2 max-w-2xl text-sm text-slate-700">{place.description}</p>
        )}
      </div>

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <Card>
          <h2 className="mb-4 text-sm font-semibold tracking-wide text-slate-500 uppercase">
            Menu
          </h2>
          <PlaceMenu
            categories={place.productCategories}
            onAdd={(categoryId, product) => setSelection({ categoryId, product })}
            disabled={!place.isOpen || !place.acceptingOrders}
          />
        </Card>

        <div className="space-y-6">
          <OrderCartPanel place={place} cart={cart} setCart={setCart} />
          <Card>
            <h2 className="mb-3 text-sm font-semibold tracking-wide text-slate-500 uppercase">
              Opening hours
            </h2>
            <PlaceHours hours={place.openingHours} />
          </Card>
        </div>
      </div>

      <OrderItemEditor
        selection={selection}
        onClose={() => setSelection(null)}
        onConfirm={addLine}
      />
    </div>
  )
}
