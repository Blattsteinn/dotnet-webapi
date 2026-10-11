import { Link } from 'react-router-dom'
import { listMyOrders } from '../../api/orders.js'
import { StatusBadge } from '../../components/Badge.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { formatDateTime, orderItemCount, orderTotal } from '../../lib/format.js'

export function OrdersPage() {
  const { data: orders, error, loading, reload } = useApi(() => listMyOrders())

  if (loading) return <LoadingBlock label="Loading your orders…" />
  if (error) return <ErrorMessage error={error} />

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">My orders</h1>
        <button
          type="button"
          onClick={reload}
          className="text-sm text-slate-600 hover:underline"
        >
          Refresh
        </button>
      </div>

      {orders.length === 0 ? (
        <EmptyState>You have not placed any orders yet.</EmptyState>
      ) : (
        <ul className="space-y-3">
          {orders.map((order) => (
            <li key={order.id}>
              <Card>
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <Link
                      to={`/orders/${order.id}`}
                      className="font-medium hover:underline"
                    >
                      Order #{order.id}
                    </Link>
                    <p className="text-xs text-slate-500">
                      Placed {formatDateTime(order.createdAt)} · {orderItemCount(order)} item(s)
                    </p>
                  </div>
                  <div className="flex items-center gap-4">
                    <StatusBadge status={order.status} />
                    <span className="text-sm font-medium tabular-nums">
                      {orderTotal(order).toFixed(2)}
                    </span>
                  </div>
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
