import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login as loginRequest } from '../api/auth.js'
import { setUnauthorizedHandler } from '../api/client.js'
import {
  clearStoredAuth,
  readStoredAuth,
  writeStoredAuth,
} from './tokenStore.js'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const navigate = useNavigate()
  // Rehydrate the session from localStorage so a reload keeps the user signed in.
  const [auth, setAuth] = useState(() => readStoredAuth())
  const [sessionExpired, setSessionExpired] = useState(false)

  const logout = useCallback(() => {
    clearStoredAuth()
    setAuth(null)
  }, [])

  // TODO(frontend): a 401 means the token is gone — expired after its 1 hour
  // lifetime, or the security stamp rotated because a role changed. There is no
  // refresh token, so the only recovery is signing in again. The flag is kept in
  // state rather than the URL, because the route guards redirect to /login right
  // after the session is dropped and would strip a query string.
  useEffect(() => {
    setUnauthorizedHandler(() => {
      logout()
      setSessionExpired(true)
      navigate('/login', { replace: true })
    })
    return () => setUnauthorizedHandler(null)
  }, [logout, navigate])

  const login = useCallback(async (email, password) => {
    const session = await loginRequest(email, password)
    const next = {
      token: session.token,
      userId: session.userId,
      email: session.email ?? email,
      roles: session.roles ?? [],
    }
    writeStoredAuth(next)
    setSessionExpired(false)
    setAuth(next)
    return next
  }, [])

  const value = useMemo(
    () => ({
      user: auth,
      isAuthenticated: Boolean(auth?.token),
      roles: auth?.roles ?? [],
      hasRole: (role) => (auth?.roles ?? []).includes(role),
      sessionExpired,
      login,
      logout,
    }),
    [auth, login, logout, sessionExpired],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider')
  }
  return context
}
