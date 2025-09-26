import { Outlet } from 'react-router-dom';
import { ApplicantProvider } from './context/ApplicantContext';
import './App.css';

function AppLayout() {
  return (
    <div className="app">
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}

function App() {
  return (
    <ApplicantProvider>
      <AppLayout />
    </ApplicantProvider>
  );
}

export default App;
