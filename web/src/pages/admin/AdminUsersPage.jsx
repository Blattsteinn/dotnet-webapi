import { useState } from 'react'
import { grantOwner, revokeOwner, searchUsers } from '../../api/admin.js'
import { Button } from '../../components/Button.jsx'
import { Card } from '../../components/Card.jsx'
import { EmptyState } from '../../components/EmptyState.jsx'
import { ErrorMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Input } from '../../components/Field.jsx'

// Granting and revoking Owner is the whole admin surface. Search returns only an
// id and an email — nothing else about the account is exposed.
export function AdminUsersPage() {
  const [search, setSearch] = useState('')
  const [users, setUsers] = useState(null)
  const [searchError, setSearchError] = useState(null)
  const [actionError, setActionError] = useState(null)
  const [loading, setLoading] = useState(false)
  const [busyUserId, setBusyUserId] = useState(null)
  const [notice, setNotice] = useState(null)

  async function handleSearch(event) {
    event.preventDefault()
    setLoading(true)
    setSearchError(null)
    setActionError(null)
    try {
      setUsers(await searchUsers(search))
    } catch (cause) {
      setSearchError(cause)
    } finally {
      setLoading(false)
    }
  }

  async function handleGrant(user) {
    setBusyUserId(user.id)
    setActionError(null)
    setNotice(null)
    try {
      await grantOwner(user.id)
      setNotice(`${user.email} is now an Owner. They must sign in again for the role to take effect.`)
    } catch (cause) {
      setActionError(cause)
    } finally {
      setBusyUserId(null)
    }
  }

  async function handleRevoke(user) {
    setBusyUserId(user.id)
    setActionError(null)
    setNotice(null)
    try {
      await revokeOwner(user.id)
      setNotice(`${user.email} lost the Owner role; their places are hidden.`)
    } catch (cause) {
      setActionError(cause)
    } finally {
      setBusyUserId(null)
    }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Owner rights</h1>

      <Card>
        <form onSubmit={handleSearch} className="flex flex-wrap items-end gap-3">
          <Input
            id="user-search"
            label="Search users"
            className="w-72"
            placeholder="part of an email address"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
          <Button type="submit" disabled={loading}>
            {loading ? 'Searching…' : 'Search'}
          </Button>
        </form>
      </Card>

      <ErrorMessage error={searchError} />
      <ErrorMessage error={actionError} />
      <SuccessMessage>{notice}</SuccessMessage>

      {users !== null && (
        <Card>
          {users.length === 0 ? (
            <EmptyState>No users match “{search}”.</EmptyState>
          ) : (
            <ul className="divide-y divide-slate-100">
              {users.map((user) => (
                <li key={user.id} className="flex flex-wrap items-center justify-between gap-3 py-2">
                  <div>
                    <p className="text-sm font-medium">{user.email ?? '(no email)'}</p>
                    <p className="text-xs text-slate-500">{user.id}</p>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      disabled={busyUserId === user.id}
                      onClick={() => handleGrant(user)}
                    >
                      Grant Owner
                    </Button>
                    <Button
                      size="sm"
                      variant="danger"
                      disabled={busyUserId === user.id}
                      onClick={() => handleRevoke(user)}
                    >
                      Revoke Owner
                    </Button>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Card>
      )}
    </div>
  )
}
