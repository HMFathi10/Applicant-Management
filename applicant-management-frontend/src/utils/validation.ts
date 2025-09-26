export interface ValidationRule {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  pattern?: RegExp;
  email?: boolean;
  phone?: boolean;
  min?: number;
  max?: number;
  custom?: (value: any) => string | undefined;
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface ValidationResult {
  isValid: boolean;
  errors: ValidationError[];
}

export const validateField = (value: any, rules: ValidationRule, fieldName: string): string | undefined => {
  if (rules.required && !value) {
    return `${fieldName} is required`;
  }

  if (value) {
    if (typeof value === 'string') {
      if (rules.minLength && value.length < rules.minLength) {
        return `${fieldName} must be at least ${rules.minLength} characters long`;
      }

      if (rules.maxLength && value.length > rules.maxLength) {
        return `${fieldName} must be no more than ${rules.maxLength} characters long`;
      }

      if (rules.pattern && !rules.pattern.test(value)) {
        return `${fieldName} format is invalid`;
      }

      if (rules.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        return `${fieldName} must be a valid email address`;
      }

      if (rules.phone && !/^[\+]?[1-9][\d]{0,15}$/.test(value.replace(/[\s\-\(\)\.]/g, ''))) {
        return `${fieldName} must be a valid phone number`;
      }
    }

    if (typeof value === 'number') {
      if (rules.min !== undefined && value < rules.min) {
        return `${fieldName} must be at least ${rules.min}`;
      }

      if (rules.max !== undefined && value > rules.max) {
        return `${fieldName} must be no more than ${rules.max}`;
      }
    }

    if (rules.custom) {
      const customError = rules.custom(value);
      if (customError) {
        return customError;
      }
    }
  }

  return undefined;
};

export const validateForm = (data: Record<string, any>, schema: Record<string, ValidationRule>): ValidationResult => {
  const errors: ValidationError[] = [];

  Object.keys(schema).forEach(field => {
    const error = validateField(data[field], schema[field], field);
    if (error) {
      errors.push({ field, message: error });
    }
  });

  return {
    isValid: errors.length === 0,
    errors
  };
};

export const applicantValidationSchema: Record<string, ValidationRule> = {
  name: {
    required: true,
    minLength: 2,
    maxLength: 100
  },
  email: {
    required: true,
    email: true,
    maxLength: 100
  },
  phone: {
    required: true,
    phone: true,
    maxLength: 20
  },
  position: {
    required: true,
    minLength: 2,
    maxLength: 100
  },
  department: {
    required: true,
    minLength: 2,
    maxLength: 50
  },
  location: {
    required: true,
    minLength: 2,
    maxLength: 100
  },
  experience: {
    required: true,
    min: 0,
    max: 50,
    custom: (value) => {
      if (value && !Number.isInteger(value)) {
        return 'Experience must be a whole number';
      }
      return undefined;
    }
  },
  skills: {
    required: true,
    custom: (value) => {
      if (!value || !Array.isArray(value) || value.length === 0) {
        return 'At least one skill is required';
      }
      if (value.some(skill => typeof skill !== 'string' || skill.trim().length === 0)) {
        return 'All skills must be non-empty strings';
      }
      return undefined;
    }
  },
  education: {
    required: true,
    minLength: 5,
    maxLength: 200
  },
  workAuthorization: {
    required: true,
    minLength: 2,
    maxLength: 50
  },
  status: {
    required: true,
    custom: (value) => {
      const validStatuses = ['Pending', 'Interview', 'Offered', 'Rejected'];
      if (!validStatuses.includes(value)) {
        return 'Invalid status';
      }
      return undefined;
    }
  },
  rating: {
    required: true,
    min: 0,
    max: 5,
    custom: (value) => {
      if (value && (typeof value !== 'number' || value % 0.5 !== 0)) {
        return 'Rating must be in increments of 0.5';
      }
      return undefined;
    }
  },
  notes: {
    maxLength: 1000
  }
};

export const sanitizeInput = (input: string): string => {
  return input
    .trim()
    .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
    .replace(/<[^>]*>/g, '')
    .replace(/javascript:/gi, '')
    .replace(/on\w+\s*=/gi, '');
};

export const formatPhoneNumber = (value: string): string => {
  const cleaned = value.replace(/\D/g, '');
  const match = cleaned.match(/^(\d{3})(\d{3})(\d{4})$/);
  if (match) {
    return `(${match[1]}) ${match[2]}-${match[3]}`;
  }
  return value;
};

export const formatEmail = (value: string): string => {
  return value.trim().toLowerCase();
};

export const debounce = <T extends (...args: any[]) => void>(
  func: T,
  delay: number
): T => {
  let timeoutId: ReturnType<typeof setTimeout>;
  return ((...args: Parameters<T>) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func(...args), delay);
  }) as T;
};