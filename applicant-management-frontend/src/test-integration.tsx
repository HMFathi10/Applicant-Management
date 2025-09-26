import React from 'react';
import { ApplicantProvider } from './context/ApplicantContext';
import { ApplicantDashboard } from './components/applicants/ApplicantDashboard';
import { ApplicantForm } from './components/applicants/ApplicantForm';
import { ApplicantDetails } from './components/applicants/ApplicantDetails';

// Integration test component to verify all components work together
export const IntegrationTest: React.FC = () => {
  // State variables for testing interactions
  // const [showForm, setShowForm] = React.useState(false);
  // const [selectedApplicant, setSelectedApplicant] = React.useState<string | null>(null);
  // const [showDetails, setShowDetails] = React.useState(false);

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Applicant Management System - Integration Test</h1>
      
      <ApplicantDashboard />

      <ApplicantForm />

      <ApplicantDetails />
    </div>
  );
};

// Wrapper component with provider
const AppIntegrationTest: React.FC = () => {
  return (
    <ApplicantProvider>
      <IntegrationTest />
    </ApplicantProvider>
  );
};

export default AppIntegrationTest;