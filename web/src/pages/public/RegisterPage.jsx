import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register } from '../../api/auth.js'
import { Button } from '../../components/Button.jsx'
import { ErrorMessage } from '../../components/ErrorMessage.jsx'
import { Input } from '../../components/Field.jsx'

export function RegisterPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      // Registration returns 200 with an empty body; it does not sign the user
      // in, so the next step is always the login screen.
      await register(email, password)
      navigate('/login', {
        replace: true,
        state: { message: 'Account created. Sign in to continue.', from: { pathname: '/' } },
      })
    } catch (cause) {
      setError(cause)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-sm space-y-4">
      <h1 className="text-2xl font-semibold">Register</h1>

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
          autoComplete="new-password"
          required
          hint="At least 6 characters, with an uppercase letter, a digit and a symbol."
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />

        <Button type="submit" className="w-full" disabled={submitting}>
          {submitting ? 'Creating account…' : 'Create account'}
        </Button>
      </form>

      <p className="text-sm text-slate-600">
        Already registered?{' '}
        <Link to="/login" className="font-medium text-slate-900 underline">
          Sign in
        </Link>
      </p>
    </div>
  )
}
