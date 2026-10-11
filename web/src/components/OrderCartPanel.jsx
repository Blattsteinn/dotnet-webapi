import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createOrder } from '../api/orders.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { formatPrice } from '../lib/format.js'
import { Button } from './Button.jsx'
import { ErrorMessage } from './ErrorMessage.jsx'
import { QuantityStepper } from './QuantityStepper.jsx'

function lineUnitPrice(line) {
  return (
    line.price +
    line.addOns.reduce((sum, addOn) => sum + addOn.price * addOn.quantity, 0)
  )
}

// Holds everything the customer picked until a single POST /api/v1/orders is
// sent — there is no server-side cart.
export function OrderCartPanel({ place, cart, setCart }) {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState(null)

  const closed = !place.isOpen || !place.acceptingOrders
  const total = cart.reduce((sum, line) => sum + lineUnitPrice(line) * line.quantity, 0)

  function setLineQuantity(key, quantity) {
    setCart((current) =>
      current.map((line) => (line.key === key ? { ...line, quantity } : line)),
    )
  }

  function removeLine(key) {
    setCart((current) => current.filter((line) => line.key !== key))
  }

  async function handleSubmit() {
    setSubmitting(true)
    setError(null)
    try {
      const order = await createOrder({
        placeId: place.id,
        orderItems: cart.map((line) => ({
          productId: line.productId,
          quantity: line.quantity,
          addOns: line.addOns.map((addOn) => ({
            addOnId: addOn.addOnId,
            quantity: addOn.quantity,
          })),
        })),
      })
      setCart([])
      navigate(`/orders/${order.id}`, { state: { justPlaced: true } })
    } catch (cause) {
      // Includes the 409 the server sends when the place closed in the meantime.
      setError(cause)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
        Your order
      </h2>

      {cart.length === 0 ? (
        <p className="mt-3 text-sm text-slate-600">Nothing added yet.</p>
      ) : (
        <ul className="mt-3 divide-y divide-slate-100">
          {cart.map((line) => (
            <li key={line.key} className="py-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-medium">{line.name}</p>
                  {line.addOns.length > 0 && (
                    <ul className="mt-0.5 text-xs text-slate-500">
                      {line.addOns.map((addOn) => (
                        <li key={addOn.addOnId}>
                          {addOn.quantity} × {addOn.name} ({formatPrice(addOn.price)})
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
                <span className="shrink-0 text-sm tabular-nums">
                  {formatPrice(lineUnitPrice(line) * line.quantity)}
                </span>
              </div>
              <div className="mt-2 flex items-center justify-between">
                <QuantityStepper
                  size="sm"
                  label={`${line.name} quantity`}
                  value={line.quantity}
                  onChange={(next) => setLineQuantity(line.key, next)}
                />
                <Button variant="ghost" size="sm" onClick={() => removeLine(line.key)}>
                  Remove
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {cart.length > 0 && (
        <div className="mt-3 flex items-center justify-between border-t border-slate-200 pt-3 text-sm font-medium">
          <span>Total</span>
          <span className="tabular-nums">{formatPrice(total)}</span>
        </div>
      )}

      <ErrorMessage error={error} className="mt-3" />

      {closed && (
        <p className="mt-3 rounded-md bg-amber-50 p-2 text-sm text-amber-900">
          This place is closed right now, so orders cannot be placed.
        </p>
      )}

      <div className="mt-3">
        {isAuthenticated ? (
          <Button
            className="w-full"
            disabled={cart.length === 0 || closed || submitting}
            onClick={handleSubmit}
          >
            {submitting ? 'Placing order…' : 'Place order'}
          </Button>
        ) : (
          <p className="text-sm text-slate-600">
            <Link to="/login" className="font-medium text-slate-900 underline">
              Sign in
            </Link>{' '}
            to place an order.
          </p>
        )}
      </div>
    </div>
  )
}
