import { useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { cancelMyOrder, getMyOrder } from '../../api/orders.js'
import { StatusBadge } from '../../components/Badge.jsx'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { OrderItemsTable } from '../../components/OrderItemsTable.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { formatDateTime } from '../../lib/format.js'
import { OrderStatus } from '../../lib/orderStatus.js'

export function OrderDetailPage() {
  const { id } = useParams()
  const location = useLocation()
  const { data: order, error, loading, reload } = useApi(() => getMyOrder(id), [id])
  const [cancelError, setCancelError] = useState(null)
  const [cancelling, setCancelling] = useState(false)
  const [justCancelled, setJustCancelled] = useState(false)

  async function handleCancel() {
    setCancelling(true)
    setCancelError(null)
    try {
      await cancelMyOrder(order.id)
      setJustCancelled(true)
      reload()
    } catch (cause) {
      // 409 once the place has moved the order past Pending.
      setCancelError(cause)
    } finally {
      setCancelling(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading order…" />
  if (error) return <ErrorMessage error={error} />

  return (
    <div className="space-y-4">
      <Link to="/orders" className="text-sm text-slate-600 hover:underline">
        ← My orders
      </Link>

      {location.state?.justPlaced && (
        <SuccessMessage>
          Order #{order.id} was placed. It is now waiting for the place to confirm it.
        </SuccessMessage>
      )}
      {justCancelled && <SuccessMessage>Order #{order.id} was cancelled.</SuccessMessage>}

      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-2xl font-semibold">Order #{order.id}</h1>
        <StatusBadge status={order.status} />
      </div>

      <p className="text-sm text-slate-600">
        Placed {formatDateTime(order.createdAt)}
        {order.placeId != null && (
          <>
            {' · '}
            <Link to={`/places/${order.placeId}`} className="underline">
              View place
            </Link>
          </>
        )}
      </p>

      <Card>
        <OrderItemsTable order={order} />
      </Card>

      <ErrorMessage error={cancelError} />

      {/* Cancelling is only possible while the order is still Pending. */}
      {order.status === OrderStatus.Pending && (
        <Button variant="danger" onClick={handleCancel} disabled={cancelling}>
          {cancelling ? 'Cancelling…' : 'Cancel order'}
        </Button>
      )}
    </div>
  )
}
