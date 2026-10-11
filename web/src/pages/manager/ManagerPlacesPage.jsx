import { Link } from 'react-router-dom'
import { listManagedPlaces } from '../../api/manager.js'
import { HiddenBadge, OpenBadge } from '../../components/Badge.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'

// `GET /api/v1/manager/places` returns the places the caller owns (while still
// holding the Owner role) plus the ones assigned to them. "Manager" is not a
// role, so this list — not the token — decides where order processing appears.
export function ManagerPlacesPage() {
  const { data: places, error, loading } = useApi(() => listManagedPlaces())

  if (loading) return <LoadingBlock label="Loading places you manage…" />
  if (error) return <ErrorMessage error={error} />

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Places you manage</h1>

      {places.length === 0 ? (
        <EmptyState>
          You do not manage any place. An owner of a place can add you as its manager.
        </EmptyState>
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
                <p className="mt-2">
                  <Link
                    to={`/manager/places/${place.id}/orders`}
                    className="text-sm font-medium underline"
                  >
                    Process orders
                  </Link>
                </p>
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
