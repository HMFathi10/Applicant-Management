import React from 'react';
import { ApplicantStatus } from '../../types';

export interface StatusBadgeProps {
  status: ApplicantStatus;
  className?: string;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, className = '' }) => {
  const getStatusConfig = (status: ApplicantStatus) => {
    const configs = {
      [ApplicantStatus.NEW]: {
        label: 'New',
        icon: '📋'
      },
      [ApplicantStatus.SCREENING]: {
        label: 'Screening',
        icon: '🔍'
      },
      [ApplicantStatus.INTERVIEW]: {
        label: 'Interview',
        icon: '🗣️'
      },
      [ApplicantStatus.OFFER]: {
        label: 'Offer',
        icon: '📄'
      },
      [ApplicantStatus.HIRED]: {
        label: 'Hired',
        icon: '✅'
      },
      [ApplicantStatus.REJECTED]: {
        label: 'Rejected',
        icon: '❌'
      }
    };
    return configs[status] || { label: status, icon: '❓' };
  };

  const config = getStatusConfig(status);
  const classes = [`status-badge status-badge--${status}`, className].filter(Boolean).join(' ');

  return (
    <span className={classes}>
      <span aria-hidden="true">{config.icon}</span>
      <span>{config.label}</span>
    </span>
  );
};

export default StatusBadge;