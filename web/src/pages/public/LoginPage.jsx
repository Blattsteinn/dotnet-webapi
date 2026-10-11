import { useState } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/AuthContext.jsx'
import { Button } from '../../components/Button.jsx'
import { ErrorMessage, NoticeMessage, SuccessMessage } from '../../components/ErrorMessage.jsx'
import { Input } from '../../components/Field.jsx'

export function LoginPage() {
  const { isAuthenticated, login, sessionExpired } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  // Already signed in (or just signed in) — go where the guard was heading.
  if (isAuthenticated) {
    const target = location.state?.from?.pathname ?? '/'
    return <Navigate to={target} replace />
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      await login(email, password)
      navigate(location.state?.from?.pathname ?? '/', { replace: true })
    } catch (cause) {
      // 401 here is a normal "wrong password" Problem Details, not an expiry.
      setError(cause)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-sm space-y-4">
      <h1 className="text-2xl font-semibold">Sign in</h1>

      {sessionExpired && (
        <NoticeMessage>
          Your session has ended — either it expired after an hour, or your roles
          changed. Please sign in again.
        </NoticeMessage>
      )}
      {location.state?.message && (
        <SuccessMessage>{location.state.message}</SuccessMessage>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <ErrorMessage error={error} />

        <Input
          id="email"
          label="Email"
          type="email"
          autoComplete="username"
          required
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
        <Input
          id="password"
          label="Password"
          type="password"
          autoComplete="current-password"
          required
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />

        <Button type="submit" className="w-full" disabled={submitting}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </Button>
      </form>

      <p className="text-sm text-slate-600">
        No account?{' '}
        <Link to="/register" className="font-medium text-slate-900 underline">
          Register
        </Link>
      </p>
    </div>
  )
}
