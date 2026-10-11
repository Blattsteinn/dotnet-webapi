import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getPlaceOrder, setOrderStatus } from '../../api/manager.js'
import { StatusBadge } from '../../components/Badge.jsx'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { OrderItemsTable } from '../../components/OrderItemsTable.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { formatDateTime } from '../../lib/format.js'
import {
  OrderStatus,
  isCancellable,
  nextStatus,
  nextStatusAction,
} from '../../lib/orderStatus.js'

export function ManagerOrderDetailPage() {
  const { placeId, orderId } = useParams()
  const { data: order, error, loading, reload } = useApi(
    () => getPlaceOrder(placeId, orderId),
    [placeId, orderId],
  )
  const [actionError, setActionError] = useState(null)
  const [pending, setPending] = useState(false)

  async function moveTo(status) {
    setPending(true)
    setActionError(null)
    try {
      await setOrderStatus(placeId, orderId, status)
      reload()
    } catch (cause) {
      // An illegal or replayed transition comes back as 409 Problem Details.
      setActionError(cause)
    } finally {
      setPending(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading order…" />
  if (error) return <ErrorMessage error={error} />

  const next = nextStatus(order.status)
  const canCancel = isCancellable(order.status)

  return (
    <div className="space-y-4">
      <Link
        to={`/manager/places/${placeId}/orders`}
        className="text-sm text-slate-600 hover:underline"
      >
        ← Orders at this place
      </Link>

      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-2xl font-semibold">Order #{order.id}</h1>
        <StatusBadge status={order.status} />
      </div>
      <p className="text-sm text-slate-600">Placed {formatDateTime(order.createdAt)}</p>

      <Card>
        <OrderItemsTable order={order} />
      </Card>

      <ErrorMessage error={actionError} />

      {/* Only the legal next step is offered; the server rejects anything else
          with 409, whose message is shown above. */}
      {(next !== null || canCancel) && (
        <div className="flex flex-wrap gap-3">
          {next !== null && (
            <Button onClick={() => moveTo(next)} disabled={pending}>
              {pending ? 'Updating…' : nextStatusAction(order.status)}
            </Button>
          )}
          {canCancel && (
            <Button
              variant="danger"
              onClick={() => moveTo(OrderStatus.Cancelled)}
              disabled={pending}
            >
              Cancel order
            </Button>
          )}
        </div>
      )}

      {next === null && !canCancel && (
        <p className="text-sm text-slate-600">
          This order reached a final state and can no longer be changed.
        </p>
      )}
    </div>
  )
}
