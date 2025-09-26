import { createBrowserRouter } from 'react-router-dom';
import App from './App';
import { ApplicantDashboard } from './components/applicants/ApplicantDashboard';
import { ApplicantForm } from './components/applicants/ApplicantForm';
import { ApplicantDetails } from './components/applicants/ApplicantDetails';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      {
        index: true,
        element: <ApplicantDashboard />
      },
      {
        path: 'dashboard',
        element: <ApplicantDashboard />
      },
      {
        path: 'applicants',
        element: <ApplicantDashboard />
      },
      {
        path: 'applicants/new',
        element: <ApplicantForm />
      },
      {
        path: 'applicants/:id',
        element: <ApplicantDetails />
      },
      {
        path: 'applicants/:id/edit',
        element: <ApplicantForm />
      }
    ]
  }
]);

export default router;