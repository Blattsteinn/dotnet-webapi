import { Link } from 'react-router-dom'
import { listPlaces } from '../../api/places.js'
import { OpenBadge } from '../../components/Badge.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'

export function PlacesPage() {
  const { data: places, error, loading } = useApi(() => listPlaces())

  if (loading) return <LoadingBlock label="Loading places…" />
  if (error) return <ErrorMessage error={error} />

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Places</h1>

      {places.length === 0 ? (
        <EmptyState>No places are available right now.</EmptyState>
      ) : (
        <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {places.map((place) => (
            <li key={place.id}>
              <Link
                to={`/places/${place.id}`}
                className="block h-full rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition hover:border-slate-400"
              >
                <div className="flex items-start justify-between gap-3">
                  <h2 className="font-medium">{place.name}</h2>
                  {/* isOpen is computed by the server; the browser never derives it. */}
                  <OpenBadge isOpen={place.isOpen} acceptingOrders={place.acceptingOrders} />
                </div>
                {place.address && (
                  <p className="mt-1 text-sm text-slate-600">{place.address}</p>
                )}
                {place.description && (
                  <p className="mt-2 line-clamp-3 text-sm text-slate-500">
                    {place.description}
                  </p>
                )}
                <p className="mt-3 text-sm text-slate-500">
                  {place.openingHours?.length
                    ? `${place.openingHours.length} weekly opening intervals`
                    : 'No opening hours published'}
                </p>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
