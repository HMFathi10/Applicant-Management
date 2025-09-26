import { ApplicantStatus } from '../types/applicant';

export const formatDate = (date: Date | string): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return dateObj?.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
};

export const formatDateTime = (date: Date | string): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  return dateObj?.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

export const formatExperience = (years: number): string => {
  if (years === 0) {
    return 'Entry Level';
  } else if (years === 1) {
    return '1 year';
  } else if (years < 5) {
    return `${years} years`;
  } else if (years < 10) {
    return `${years} years (Senior)`;
  } else {
    return `${years} years (Lead)`;
  }
};

export const formatSkills = (skills: string[], limit: number = 3): string => {
  if (skills.length === 0) return 'No skills listed';
  
  const displaySkills = skills.slice(0, limit);
  const remaining = skills.length - limit;
  
  if (remaining > 0) {
    return `${displaySkills.join(', ')} +${remaining} more`;
  }
  
  return displaySkills.join(', ');
};

export const formatRating = (rating: number): string => {
  return `${rating.toFixed(1)}/5.0`;
};

export const formatStatus = (status: ApplicantStatus): {
  label: string;
  color: string;
  backgroundColor: string;
  icon: string;
} => {
  switch (status) {
    case ApplicantStatus.NEW:
      return {
        label: 'New Application',
        color: '#f59e0b',
        backgroundColor: '#fef3c7',
        icon: '⏳'
      };
    case ApplicantStatus.SCREENING:
      return {
        label: 'In Screening',
        color: '#8b5cf6',
        backgroundColor: '#ede9fe',
        icon: '🔍'
      };
    case ApplicantStatus.INTERVIEW:
      return {
        label: 'In Interview',
        color: '#3b82f6',
        backgroundColor: '#dbeafe',
        icon: '🎤'
      };
    case ApplicantStatus.OFFER:
      return {
        label: 'Offer Extended',
        color: '#10b981',
        backgroundColor: '#d1fae5',
        icon: '🎉'
      };
    case ApplicantStatus.HIRED:
      return {
        label: 'Hired',
        color: '#059669',
        backgroundColor: '#ccfbf1',
        icon: '✅'
      };
    case ApplicantStatus.REJECTED:
      return {
        label: 'Not Selected',
        color: '#ef4444',
        backgroundColor: '#fee2e2',
        icon: '❌'
      };
    default:
      return {
        label: status,
        color: '#6b7280',
        backgroundColor: '#f3f4f6',
        icon: '❓'
      };
  }
};

export const formatDepartment = (department: string): string => {
  const departmentMap: Record<string, string> = {
    'Engineering': 'Engineering',
    'Product': 'Product Management',
    'Design': 'Design',
    'Marketing': 'Marketing',
    'Sales': 'Sales',
    'Operations': 'Operations',
    'Finance': 'Finance',
    'HR': 'Human Resources',
    'Legal': 'Legal',
    'Data Science': 'Data Science',
    'QA': 'Quality Assurance',
    'DevOps': 'DevOps'
  };
  
  return departmentMap[department] || department;
};

export const formatWorkAuthorization = (authorization: string): string => {
  const authMap: Record<string, string> = {
    'US Citizen': 'US Citizen',
    'Green Card': 'Permanent Resident',
    'H1-B': 'H1-B Visa',
    'OPT': 'Optional Practical Training',
    'CPT': 'Curricular Practical Training',
    'TN': 'TN Visa',
    'O-1': 'O-1 Visa',
    'F-1': 'F-1 Student Visa',
    'J-1': 'J-1 Exchange Visa',
    'B-1': 'B-1 Business Visa',
    'L-1': 'L-1 Intracompany Transfer'
  };
  
  return authMap[authorization] || authorization;
};

export const generateInitials = (name: string): string => {
  const words = name.trim().split(' ');
  if (words.length === 0) return '?';
  
  if (words.length === 1) {
    return words[0].charAt(0).toUpperCase();
  }
  
  const firstInitial = words[0].charAt(0).toUpperCase();
  const lastInitial = words[words.length - 1].charAt(0).toUpperCase();
  
  return `${firstInitial}${lastInitial}`;
};

export const getAvatarColor = (name: string): string => {
  const colors = [
    '#f59e0b', // amber
    '#10b981', // emerald
    '#3b82f6', // blue
    '#8b5cf6', // violet
    '#ef4444', // red
    '#06b6d4', // cyan
    '#84cc16', // lime
    '#f97316', // orange
    '#ec4899', // pink
    '#6366f1'  // indigo
  ];
  
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  
  return colors[Math.abs(hash) % colors.length];
};

export const truncateText = (text: string, maxLength: number): string => {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength - 3) + '...';
};

export const capitalizeWords = (text: string): string => {
  return text.replace(/\w\S*/g, (txt) => {
    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
  });
};

export const formatSalary = (salary: number): string => {
  if (salary >= 1000000) {
    return `$${(salary / 1000000).toFixed(1)}M`;
  } else if (salary >= 1000) {
    return `$${(salary / 1000).toFixed(0)}K`;
  }
  return `$${salary}`;
};

export const formatTimeAgo = (date: Date | string): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffInMs = now.getTime() - dateObj.getTime();
  const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
  
  if (diffInDays === 0) {
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    if (diffInHours === 0) {
      const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
      return diffInMinutes <= 1 ? 'Just now' : `${diffInMinutes} minutes ago`;
    }
    return diffInHours === 1 ? '1 hour ago' : `${diffInHours} hours ago`;
  } else if (diffInDays === 1) {
    return 'Yesterday';
  } else if (diffInDays < 7) {
    return `${diffInDays} days ago`;
  } else if (diffInDays < 30) {
    const weeks = Math.floor(diffInDays / 7);
    return weeks === 1 ? '1 week ago' : `${weeks} weeks ago`;
  } else if (diffInDays < 365) {
    const months = Math.floor(diffInDays / 30);
    return months === 1 ? '1 month ago' : `${months} months ago`;
  } else {
    const years = Math.floor(diffInDays / 365);
    return years === 1 ? '1 year ago' : `${years} years ago`;
  }
};