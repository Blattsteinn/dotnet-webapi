import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { addManager, listManagers, removeManager } from '../../api/owner.js'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Input } from '../../components/Field.jsx'
import { OwnerPlaceTabs } from '../../components/OwnerPlaceTabs.jsx'
import { LoadingBlock } from '../../components/Spinner.jsx'
import { useApi } from '../../hooks/useApi.js'

export function OwnerManagersPage() {
  const { id } = useParams()
  const { data: managers, error, loading, reload } = useApi(() => listManagers(id), [id])
  const [email, setEmail] = useState('')
  const [actionError, setActionError] = useState(null)
  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState(null)

  async function handleAdd(event) {
    event.preventDefault()
    setBusy(true)
    setActionError(null)
    setNotice(null)
    try {
      // The owner assigns by email; the server resolves the user id.
      await addManager(id, email)
      setNotice(`${email} can now process orders for this place.`)
      setEmail('')
      reload()
    } catch (cause) {
      setActionError(cause)
    } finally {
      setBusy(false)
    }
  }

  async function handleRemove(userId, label) {
    setBusy(true)
    setActionError(null)
    setNotice(null)
    try {
      // Removal is by user id, which is exactly what the list returns.
      await removeManager(id, userId)
      setNotice(`${label} was removed as a manager.`)
      reload()
    } catch (cause) {
      setActionError(cause)
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <LoadingBlock label="Loading managers…" />
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

  return (
    <div className="space-y-4">
      <div>
        <Link to="/owner/places" className="text-sm text-slate-600 hover:underline">
          ← My places
        </Link>
        <h1 className="mt-2 text-2xl font-semibold">Managers</h1>
      </div>

      <OwnerPlaceTabs />
      <ErrorMessage error={actionError} />
      <SuccessMessage>{notice}</SuccessMessage>

      <Card>
        <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
          Add a manager
        </h2>
        <form onSubmit={handleAdd} className="mt-3 flex flex-wrap items-end gap-3">
          <Input
            id="manager-email"
            label="Email"
            type="email"
            required
            className="w-72"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
          <Button type="submit" disabled={busy}>
            {busy ? 'Working…' : 'Add manager'}
          </Button>
        </form>
      </Card>

      <Card>
        <h2 className="text-sm font-semibold tracking-wide text-slate-500 uppercase">
          Current managers
        </h2>
        <div className="mt-3">
          {managers.length === 0 ? (
            <EmptyState>No managers have been added to this place.</EmptyState>
          ) : (
            <ul className="divide-y divide-slate-100">
              {managers.map((manager) => (
                <li key={manager.userId} className="flex items-center justify-between gap-3 py-2">
                  <div>
                    <p className="text-sm font-medium">
                      {manager.email ?? manager.userName ?? manager.userId}
                    </p>
                    <p className="text-xs text-slate-500">{manager.userId}</p>
                  </div>
                  <Button
                    variant="danger"
                    size="sm"
                    disabled={busy}
                    onClick={() =>
                      handleRemove(manager.userId, manager.email ?? manager.userId)
                    }
                  >
                    Remove
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </Card>
    </div>
  )
}
