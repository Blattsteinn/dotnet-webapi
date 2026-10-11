import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import { ROLES } from '../lib/roles.js'
import { Button } from './Button.jsx'

function NavItem({ to, children }) {
  return (
    <NavLink
      to={to}
      end={to === '/'}
      className={({ isActive }) =>
        `rounded-md px-3 py-1.5 text-sm font-medium transition ${
          isActive
            ? 'bg-slate-900 text-white'
            : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
        }`
      }
    >
      {children}
    </NavLink>
  )
}

// The links are only a convenience — `roles` decides what to show, never what is
// allowed. Every request is re-checked by the server.
export function Layout() {
  const { user, isAuthenticated, hasRole, logout } = useAuth()
  const navigate = useNavigate()

  function handleSignOut() {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-3 px-4 py-3">
          <Link to="/" className="mr-2 text-base font-semibold">
            Food Ordering
          </Link>

          <nav className="flex flex-wrap items-center gap-1">
            <NavItem to="/">Places</NavItem>
            {isAuthenticated && <NavItem to="/orders">My orders</NavItem>}
            {isAuthenticated && <NavItem to="/manager/places">Managed places</NavItem>}
            {hasRole(ROLES.Owner) && <NavItem to="/owner/places">My places</NavItem>}
            {hasRole(ROLES.Admin) && <NavItem to="/admin/users">Admin</NavItem>}
          </nav>

          <div className="ml-auto flex items-center gap-3">
            {isAuthenticated ? (
              <>
                <span className="hidden text-sm text-slate-600 sm:inline">
                  {user.email}
                  {user.roles?.length > 0 && (
                    <span className="ml-2 rounded bg-slate-100 px-1.5 py-0.5 text-xs font-medium text-slate-600">
                      {user.roles.join(', ')}
                    </span>
                  )}
                </span>
                <Button variant="secondary" size="sm" onClick={handleSignOut}>
                  Sign out
                </Button>
              </>
            ) : (
              <>
                <NavItem to="/login">Sign in</NavItem>
                <Button size="sm" onClick={() => navigate('/register')}>
                  Register
                </Button>
              </>
            )}
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
