import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext.jsx'
import { AppRoutes } from './routes.jsx'

// AuthProvider sits inside the router so it can use navigate() when a 401 forces
// a return to the login screen.
export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}
