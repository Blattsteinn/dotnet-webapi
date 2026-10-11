import { Link, useParams } from 'react-router-dom'
import { listManagedPlaces, listPlaceOrders } from '../../api/manager.js'
import { StatusBadge } from '../../components/Badge.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'
import { formatDateTime, orderItemCount, orderTotal } from '../../lib/format.js'

export function ManagerOrdersPage() {
  const { placeId } = useParams()

  // The place name is not part of the order payload, so it is looked up from the
  // managed-places list. A place the caller does not manage returns 404 here.
  const { data, error, loading, reload } = useApi(
    async () => {
      const [places, orders] = await Promise.all([
        listManagedPlaces(),
        listPlaceOrders(placeId),
      ])
      return { places, orders }
    },
    [placeId],
  )

  if (loading) return <LoadingBlock label="Loading orders…" />
  if (error) return <ErrorMessage error={error} />

  const place = data.places.find((candidate) => String(candidate.id) === String(placeId))

  return (
    <div className="space-y-4">
      <Link to="/manager/places" className="text-sm text-slate-600 hover:underline">
        ← Places you manage
      </Link>

      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">
          Orders{place ? ` for ${place.name}` : ''}
        </h1>
        <button
          type="button"
          onClick={reload}
          className="text-sm text-slate-600 hover:underline"
        >
          Refresh
        </button>
      </div>

      {data.orders.length === 0 ? (
        <EmptyState>No orders at this place yet.</EmptyState>
      ) : (
        <ul className="space-y-3">
          {data.orders.map((order) => (
            <li key={order.id}>
              <Card>
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <Link
                      to={`/manager/places/${placeId}/orders/${order.id}`}
                      className="font-medium hover:underline"
                    >
                      Order #{order.id}
                    </Link>
                    <p className="text-xs text-slate-500">
                      {formatDateTime(order.createdAt)} · {orderItemCount(order)} item(s)
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
