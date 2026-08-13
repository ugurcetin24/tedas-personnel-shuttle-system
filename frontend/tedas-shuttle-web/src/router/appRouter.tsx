import { Navigate, createBrowserRouter } from 'react-router'
import { AppLayout } from '../layouts/AppLayout'
import { AssignmentsPage } from '../pages/AssignmentsPage'
import { DashboardPage } from '../pages/DashboardPage'
import { DriversPage } from '../pages/DriversPage'
import { ImportsPage } from '../pages/ImportsPage'
import { PersonnelPage } from '../pages/PersonnelPage'
import { RoutesPage } from '../pages/RoutesPage'
import { ShuttlesPage } from '../pages/ShuttlesPage'

export const appRouter = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'personnel', element: <PersonnelPage /> },
      { path: 'shuttles', element: <ShuttlesPage /> },
      { path: 'drivers', element: <DriversPage /> },
      { path: 'assignments', element: <AssignmentsPage /> },
      { path: 'routes', element: <RoutesPage /> },
      { path: 'imports', element: <ImportsPage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
])
