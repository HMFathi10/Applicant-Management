import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Card } from '../common';
import { X, Mail, Phone, Calendar, Edit, Trash2 } from 'lucide-react';
import { useApplicantContext } from '../../context/ApplicantContext';
import { formatDate, formatDateTime } from '../../utils/formatters';

export const ApplicantDetails: React.FC = () => {
  const { state, dispatch } = useApplicantContext();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const applicant = id ? state.applicants.find(app => app.id.toString() === id) : undefined;

  if (!applicant) {
    return (
      <div
        className="modal-overlay"
        style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000,
          padding: '1rem'
        }}
        role="dialog"
        aria-label="Applicant not found"
      >
        <div
          className="modal-content"
          style={{
            backgroundColor: 'var(--color-background)',
            borderRadius: 'var(--border-radius-lg)',
            boxShadow: 'var(--shadow-lg)',
            maxWidth: '400px',
            width: '100%',
            padding: '2rem',
            textAlign: 'center'
          }}
        >
          <h2 style={{ marginBottom: '1rem' }}>Applicant Not Found</h2>
          <p style={{ color: 'var(--color-text-secondary)', marginBottom: '1.5rem' }}>
            The applicant you're looking for doesn't exist.
          </p>
          <Button onClick={() => navigate('/')}>
            Back to Dashboard
          </Button>
        </div>
      </div>
    );
  }

  // Professional details functions removed for new system

  const handleEdit = () => {
    navigate(`/applicants/${applicant.id}/edit`);
  };

  const handleDelete = async () => {
    if (window.confirm(`Are you sure you want to delete ${applicant.name}?`)) {
      dispatch({ type: 'DELETE_APPLICANT', payload: applicant.id });
      navigate('/');
    }
  };

  return (
    <div
      className="modal-overlay"
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
        padding: '1rem'
      }}
      onClick={() => navigate('/')}
      role="dialog"
      aria-labelledby="applicant-details-title"
      aria-describedby="applicant-details-content"
    >
      <div
        className="modal-content"
        style={{
          backgroundColor: 'var(--color-background)',
          borderRadius: 'var(--border-radius-lg)',
          boxShadow: 'var(--shadow-lg)',
          maxWidth: '800px',
          width: '100%',
          maxHeight: '90vh',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column'
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            padding: '1.5rem',
            borderBottom: '1px solid var(--color-border)'
          }}
        >
          <div>
            <h2 id="applicant-details-title" style={{ margin: 0, fontSize: '1.5rem' }}>{applicant.name} {applicant.familyName}</h2>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <button
              onClick={() => navigate('/')}
              style={{
                background: 'none',
                border: 'none',
                padding: '0.5rem',
                cursor: 'pointer',
                borderRadius: 'var(--border-radius-sm)',
                color: 'var(--color-text-secondary)',
                transition: 'all var(--transition-fast)'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.backgroundColor = 'var(--color-surface)';
                e.currentTarget.style.color = 'var(--color-text-primary)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = 'transparent';
                e.currentTarget.style.color = 'var(--color-text-secondary)';
              }}
            >
              <X size={24} />
            </button>
          </div>
        </div>

        {/* Content */}
        <div
          style={{
            flex: 1,
            overflowY: 'auto',
            padding: '1.5rem'
          }}
        >
          <div style={{ display: 'grid', gap: '2rem' }}>
            {/* Contact Information */}
            <Card>
              <div id="applicant-details-content" style={{ padding: '2rem' }}>
                <h3 style={{ marginBottom: '1rem', fontSize: '1.125rem', fontWeight: '600' }}>
                  Contact Information
                </h3>
                <div style={{ display: 'grid', gap: '1rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <Mail size={20} style={{ color: 'var(--color-text-secondary)' }} />
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Email Address</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.emailAddress}
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <Phone size={20} style={{ color: 'var(--color-text-secondary)' }} />
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Phone</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.phone}
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <div style={{ width: '20px', height: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <span style={{ fontWeight: 'bold', color: 'var(--color-text-secondary)' }}>#</span>
                    </div>
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Age</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.age} years old
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <div style={{ width: '20px', height: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <span style={{ fontWeight: 'bold', color: 'var(--color-text-secondary)' }}>🌍</span>
                    </div>
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Country of Origin</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.countryOfOrigin}
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <div style={{ width: '20px', height: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <span style={{ fontWeight: 'bold', color: 'var(--color-text-secondary)' }}>📍</span>
                    </div>
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Address</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.address}
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <div style={{ width: '20px', height: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <span style={{ fontWeight: 'bold', color: 'var(--color-text-secondary)' }}>💼</span>
                    </div>
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Hired</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {applicant.hired ? 'Yes' : 'No'}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </Card>

            {/* Timeline */}
            <Card>
              <div style={{ padding: '1.5rem' }}>
                <h3 style={{ marginBottom: '1rem', fontSize: '1.125rem', fontWeight: '600' }}>
                  Timeline
                </h3>
                <div style={{ display: 'grid', gap: '1rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <Calendar size={20} style={{ color: 'var(--color-text-secondary)' }} />
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Application Date</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {formatDate(applicant.appliedDate)}
                      </p>
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <Calendar size={20} style={{ color: 'var(--color-text-secondary)' }} />
                    <div>
                      <p style={{ fontWeight: '500', margin: 0 }}>Last Modified Date</p>
                      <p style={{ color: 'var(--color-text-secondary)', margin: '0.25rem 0 0' }}>
                        {formatDateTime(applicant.updatedAt)}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </Card>
          </div>
        </div>

        {/* Footer */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            padding: '1.5rem',
            borderTop: '1px solid var(--color-border)',
            backgroundColor: 'var(--color-surface)'
          }}
        >
          <div style={{ display: 'flex', gap: '1rem' }}>
            <Button
              variant="outline"
              icon={<Edit size={16} />}
              onClick={handleEdit}
              aria-label={`Edit ${applicant.name}`}
            >
              Edit
            </Button>
            <Button
              variant="outline"
              icon={<Trash2 size={16} />}
              onClick={handleDelete}
              style={{ color: 'var(--color-error)' }}
              aria-label={`Delete ${applicant.name}`}
            >
              Delete
            </Button>
          </div>
          <Button variant="outline" onClick={() => navigate('/')}>
            Close
          </Button>
        </div>
      </div>
    </div>
  );
};