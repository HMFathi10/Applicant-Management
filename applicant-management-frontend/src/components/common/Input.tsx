import { forwardRef } from 'react';
import type { InputHTMLAttributes, ReactNode } from 'react';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helpText?: string;
  required?: boolean;
  icon?: ReactNode;
  iconPosition?: 'left' | 'right';
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ 
    label, 
    error, 
    helpText, 
    required = false, 
    icon, 
    iconPosition = 'left',
    className = '',
    id,
    ...props 
  }, ref) => {
    const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;
    const hasError = !!error;
    const hasIcon = !!icon;

    const inputClasses = [
      'form-input',
      hasError && 'form-input--error',
      hasIcon && `form-input--icon-${iconPosition}`,
      className
    ].filter(Boolean).join(' ');

    return (
      <div className="form-group">
        {label && (
          <label 
            htmlFor={inputId} 
            className={`form-label ${required ? 'form-label--required' : ''}`}
          >
            {label}
          </label>
        )}
        <div className="input-wrapper">
          {hasIcon && iconPosition === 'left' && (
            <span className="input-icon input-icon--left" aria-hidden="true">
              {icon}
            </span>
          )}
          <input
            ref={ref}
            id={inputId}
            className={inputClasses}
            aria-invalid={hasError}
            aria-describedby={[
              helpText && `${inputId}-help`,
              hasError && `${inputId}-error`
            ].filter(Boolean).join(' ')}
            required={required}
            {...props}
          />
          {hasIcon && iconPosition === 'right' && (
            <span className="input-icon input-icon--right" aria-hidden="true">
              {icon}
            </span>
          )}
        </div>
        {helpText && !hasError && (
          <div id={`${inputId}-help`} className="form-help-text">
            {helpText}
          </div>
        )}
        {hasError && (
          <div id={`${inputId}-error`} className="form-error" role="alert">
            <span aria-hidden="true">⚠️</span>
            {error}
          </div>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export default Input;