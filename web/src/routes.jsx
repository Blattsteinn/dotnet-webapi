import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { useAuth } from './auth/AuthContext.jsx'
import { Card } from './components/Card.jsx'
import { Layout } from './components/Layout.jsx'
import { AdminUsersPage } from './pages/admin/AdminUsersPage.jsx'
import { OrderDetailPage } from './pages/customer/OrderDetailPage.jsx'
import { OrdersPage } from './pages/customer/OrdersPage.jsx'
import { ManagerOrderDetailPage } from './pages/manager/ManagerOrderDetailPage.jsx'
import { ManagerOrdersPage } from './pages/manager/ManagerOrdersPage.jsx'
import { ManagerPlacesPage } from './pages/manager/ManagerPlacesPage.jsx'
import { OwnerHoursPage } from './pages/owner/OwnerHoursPage.jsx'
import { OwnerManagersPage } from './pages/owner/OwnerManagersPage.jsx'
import { OwnerPlaceEditorPage } from './pages/owner/OwnerPlaceEditorPage.jsx'
import { OwnerPlacesPage } from './pages/owner/OwnerPlacesPage.jsx'
import { LoginPage } from './pages/public/LoginPage.jsx'
import { PlacePage } from './pages/public/PlacePage.jsx'
import { PlacesPage } from './pages/public/PlacesPage.jsx'
import { RegisterPage } from './pages/public/RegisterPage.jsx'
import { ROLES } from './lib/roles.js'

// Guards are UX only — the server enforces every rule. RequireAuth covers "any
// logged-in user"; RequireRole covers the two real roles. Manager access is NOT
// a role: it is per place, so those screens only require authentication and let
// the API answer with a 404 for a place the caller does not manage.
function RequireAuth({ children }) {
  const { isAuthenticated } = useAuth()
  const location = useLocation()
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }
  return children
}

function RequireRole({ role, children }) {
  const { isAuthenticated, hasRole } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (!hasRole(role)) {
    return (
      <Card>
        <h1 className="text-lg font-semibold">Not available</h1>
        <p className="mt-1 text-sm text-slate-600">
          This area is only for accounts with the {role} role.
        </p>
      </Card>
    )
  }

  return children
}

function NotFoundPage() {
  return (
    <Card>
      <h1 className="text-lg font-semibold">Page not found</h1>
      <p className="mt-1 text-sm text-slate-600">
        The page you asked for does not exist.
      </p>
    </Card>
  )
}

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<PlacesPage />} />
        <Route path="places/:id" element={<PlacePage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />

        <Route
          path="orders"
          element={
            <RequireAuth>
              <OrdersPage />
            </RequireAuth>
          }
        />
        <Route
          path="orders/:id"
          element={
            <RequireAuth>
              <OrderDetailPage />
            </RequireAuth>
          }
        />

        <Route
          path="manager/places"
          element={
            <RequireAuth>
              <ManagerPlacesPage />
            </RequireAuth>
          }
        />
        <Route
          path="manager/places/:placeId/orders"
          element={
            <RequireAuth>
              <ManagerOrdersPage />
            </RequireAuth>
          }
        />
        <Route
          path="manager/places/:placeId/orders/:orderId"
          element={
            <RequireAuth>
              <ManagerOrderDetailPage />
            </RequireAuth>
          }
        />

        <Route
          path="owner/places"
          element={
            <RequireRole role={ROLES.Owner}>
              <OwnerPlacesPage />
            </RequireRole>
          }
        />
        <Route
          path="owner/places/:id"
          element={
            <RequireRole role={ROLES.Owner}>
              <OwnerPlaceEditorPage />
            </RequireRole>
          }
        />
        <Route
          path="owner/places/:id/hours"
          element={
            <RequireRole role={ROLES.Owner}>
              <OwnerHoursPage />
            </RequireRole>
          }
        />
        <Route
          path="owner/places/:id/managers"
          element={
            <RequireRole role={ROLES.Owner}>
              <OwnerManagersPage />
            </RequireRole>
          }
        />

        <Route
          path="admin/users"
          element={
            <RequireRole role={ROLES.Admin}>
              <AdminUsersPage />
            </RequireRole>
          }
        />

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
