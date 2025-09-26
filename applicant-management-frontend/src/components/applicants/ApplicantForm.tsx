import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Input, Modal } from '../common';
import { Save, User, Mail, Phone, Calendar, MapPin } from 'lucide-react';
// import { applicantValidationSchema, validateForm as validateFormData } from '../../utils/validation';
import { showSuccess, showError } from '../../utils/notifications';
import { countryService } from '../../services/countryService';

// Utility functions for input formatting
const sanitizeInput = (value: string): string => {
  return value.trim();
};

const formatEmail = (value: string): string => {
  return value.trim().toLowerCase();
};

const formatPhoneNumber = (value: string): string => {
  // Format for Egyptian phone numbers
  // Remove any non-digit characters except the + sign
  let cleaned = value.replace(/[^\d+]/g, '');

  // Ensure it starts with +20 (Egypt country code)
  if (!cleaned.startsWith('+20')) {
    if (cleaned.startsWith('+')) {
      cleaned = '+20' + cleaned.substring(1);
    } else if (cleaned.startsWith('20')) {
      cleaned = '+' + cleaned;
    } else {
      cleaned = '+20' + cleaned;
    }
  }

  // Limit to exactly 9 digits after the +20 prefix
  const prefix = '+20';
  const digits = cleaned.substring(prefix.length);
  const limitedDigits = digits.substring(0, 10);

  return prefix + limitedDigits;
};
import { useApplicantContext } from '../../context/ApplicantContext';

interface Country {
  name: string;
  alpha2Code: string;
  alpha3Code: string;
  region: string;
}

interface FormData {
  // New properties
  Name: string;
  FamilyName: string;
  Address: string;
  CountryOfOrigin: string;
  EmailAddress: string;
  Age: number;
  Hired: boolean;

  // Legacy properties (non-professional)
  phone: string;
  appliedDate: string;
}

interface FormErrors {
  [key: string]: string;
}

export const ApplicantForm: React.FC = () => {
  const { state, actions } = useApplicantContext();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const mode = id ? 'edit' : 'create';
  const initialData = id ? state.applicants.find(app => app.id.toString() === id) : undefined;

  const [countries, setCountries] = useState<Country[]>([]);
  const [countriesLoading, setCountriesLoading] = useState(false);

  const [formData, setFormData] = useState<FormData>({
    // New properties
    Name: '',
    FamilyName: '',
    Address: '',
    CountryOfOrigin: '',
    EmailAddress: '',
    Age: 20, // Default age set to 20
    Hired: false,
    phone: '+20 ', // Default to Egypt country code
    appliedDate: new Date().toISOString().split('T')[0],
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    console.log({ state, id, message: state.applicants.find(app => app.id.toString() === id) });
  }, []);

  useEffect(() => {
    // Load countries using the country service
    const loadCountries = async () => {
      setCountriesLoading(true);
      try {
        const countries = await countryService.getCountries();
        setCountries(countries);
      } catch (error) {
        console.error('Error loading countries:', error);
        setCountries([]);
      } finally {
        setCountriesLoading(false);
      }
    };

    loadCountries();
  }, []);

  useEffect(() => {
    if (initialData && mode === 'edit') {
      setFormData({
        // New properties - map from existing data or set defaults
        Name: initialData.name?.split(' ')[0] || '',
        FamilyName: initialData.familyName || '',
        Address: initialData.address || '',
        CountryOfOrigin: initialData.countryOfOrigin, // Will need to be manually entered
        EmailAddress: initialData.emailAddress || '',
        Age: initialData.age, // Default age set to 20
        Hired: initialData.hired,
        phone: initialData.phone || '+20 ', // Default to Egypt if not set
        appliedDate: initialData.appliedDate,
      });
    } else if (mode === 'create') {
      setFormData({
        // New properties
        Name: '',
        FamilyName: '',
        Address: '',
        CountryOfOrigin: '',
        EmailAddress: '',
        Age: 20, // Default age set to 20
        Hired: false,
        phone: '+20 ', // Default to Egypt country code
        appliedDate: new Date().toISOString().split('T')[0],
      });
      setErrors({});
    }
  }, [initialData, mode]);

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    // Validate new properties
    if (!formData.Name || formData.Name.length < 5) {
      newErrors.Name = 'Name must be at least 5 characters';
    }

    if (!formData.FamilyName || formData.FamilyName.length < 5) {
      newErrors.FamilyName = 'Family Name must be at least 5 characters';
    }

    if (!formData.Address || formData.Address.length < 10) {
      newErrors.Address = 'Address must be at least 10 characters';
    }

    if (!formData.CountryOfOrigin) {
      newErrors.CountryOfOrigin = 'Country of Origin is required';
    }

    if (!formData.EmailAddress) {
      newErrors.EmailAddress = 'Email Address is required';
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(formData.EmailAddress)) {
        newErrors.EmailAddress = 'Please enter a valid email address';
      }
    }

    if (!formData.Age || formData.Age < 20) {
      newErrors.Age = 'Age must be at least 20';
    }

    // Validate phone number - must be exactly 9 digits after +20
    const phoneDigits = formData.phone.replace(/[^\d]/g, '').substring(2); // Remove +20 prefix
    if (phoneDigits.length !== 10) {
      newErrors.phone = 'Phone number must be exactly 10 digits after +20 prefix';
    }

    // Validate non-professional legacy properties only
    // const nonProfessionalSchema = {
    //   name: applicantValidationSchema.name,
    //   email: applicantValidationSchema.email,
    //   phone: applicantValidationSchema.phone,
    // };
    // const legacyValidation = validateFormData(formData, nonProfessionalSchema);
    // if (!legacyValidation.isValid) {
    //   legacyValidation.errors.forEach((error: ValidationError) => {
    //     newErrors[error.field] = error.message;
    //   });
    // }

    console.log({
      formData,
      newErrors,
    });

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return false;
    }

    setErrors({});
    return true;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    console.log({
      formData,
    });
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setSubmitting(true);
    try {

      // Create submission data that preserves compatibility
      const submissionData = {
        name: formData.Name,
        familyName: formData.FamilyName,
        address: formData.Address,
        countryOfOrigin: formData.CountryOfOrigin,
        emailAddress: formData.EmailAddress,
        age: formData.Age,
        hired: formData.Hired,
        phone: formData.phone,
        appliedDate: formData.appliedDate,
      };

      if (mode === 'create') {
        // Create new applicant
        await actions.addApplicant(submissionData);
        showSuccess('Applicant created successfully!', {
          title: 'Success',
          duration: 3000
        });
      } else if (initialData) {
        // Update existing applicant
        await actions.updateApplicant({ ...initialData, ...submissionData });
        showSuccess('Applicant updated successfully!', {
          title: 'Success',
          duration: 3000
        });
      }

      // Navigate back to dashboard after successful operation
      setTimeout(() => {
        navigate('/');
      }, 1000);

    } catch (error) {
      console.error('Error submitting form:', error);

      // Enhanced error handling with specific error messages
      let errorMessage = 'Failed to save applicant. Please try again.';

      if (error instanceof Error) {
        // Handle specific error types
        if (error.message.includes('Network')) {
          errorMessage = 'Network error: Please check your connection and try again.';
        } else if (error.message.includes('404')) {
          errorMessage = 'Service not found. Please contact support.';
        } else if (error.message.includes('400')) {
          errorMessage = 'Invalid data provided. Please check your input and try again.';
        } else if (error.message.includes('409')) {
          errorMessage = 'A conflict occurred. This applicant may already exist.';
        } else {
          errorMessage = error.message;
        }
      }

      setErrors({ submit: errorMessage });
      showError(errorMessage, {
        title: 'Error',
        duration: 5000
      });

    } finally {
      setSubmitting(false);
    }
  };

  const handleInputChange = (field: keyof FormData, value: string | number | boolean) => {
    let processedValue = value;

    // Sanitize and format specific fields
    if (field === 'Name' || field === 'FamilyName' || field === 'Address') {
      processedValue = sanitizeInput(String(value));
    } else if (field === 'EmailAddress') {
      processedValue = formatEmail(String(value));
    } else if (field === 'phone') {
      processedValue = formatPhoneNumber(String(value));
    }

    setFormData(prev => ({ ...prev, [field]: processedValue }));

    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const getFieldAccessibilityProps = (fieldName: string, error?: string) => {
    return {
      'aria-invalid': !!error,
      'aria-describedby': error ? `${fieldName}-error` : undefined,
      'aria-required': true
    };
  };

  return (
    <Modal
      isOpen={true}
      onClose={() => navigate('/')}
      title={mode === 'create' ? 'Add New Applicant' : 'Edit Applicant'}
      size="xl"
      aria-label={mode === 'create' ? 'Add new applicant form' : 'Edit applicant form'}
    >
      <form onSubmit={handleSubmit} noValidate>
        <div style={{ display: 'grid', gap: '1.5rem' }}>
          {/* Applicant Information */}
          <div>
            <h3 style={{ marginBottom: '1rem', color: 'var(--color-text-secondary)' }}>Applicant Information</h3>
            <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))' }}>

              <Input
                icon={<User />}
                label="Name *"
                value={formData.Name}
                onChange={(e) => handleInputChange('Name', e.target.value)}
                error={errors.Name}
                placeholder="John"
                {...getFieldAccessibilityProps('Name', errors.Name)}
              />
              <Input
                icon={<User />}
                label="Family Name *"
                value={formData.FamilyName}
                onChange={(e) => handleInputChange('FamilyName', e.target.value)}
                error={errors.FamilyName}
                placeholder="Doe"
                {...getFieldAccessibilityProps('FamilyName', errors.FamilyName)}
              />
              <Input
                icon={<Mail />}
                label="Email Address *"
                type="email"
                value={formData.EmailAddress}
                onChange={(e) => handleInputChange('EmailAddress', e.target.value)}
                error={errors.EmailAddress}
                placeholder="john.doe@example.com"
                {...getFieldAccessibilityProps('EmailAddress', errors.EmailAddress)}
              />
              <Input
                icon={<Calendar />}
                label="Age *"
                type="number"
                min="20"
                value={formData.Age}
                onChange={(e) => handleInputChange('Age', Number(e.target.value))}
                error={errors.Age}
                placeholder="25"
                {...getFieldAccessibilityProps('Age', errors.Age)}
              />
              <Input
                icon={<Phone />}
                label="Phone Number *"
                type="tel"
                value={formData.phone}
                onChange={(e) => handleInputChange('phone', e.target.value)}
                error={errors.phone}
                placeholder="+20 123456789"
                {...getFieldAccessibilityProps('phone', errors.phone)}
              />

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                  Country of Origin *
                </label>
                <select
                  value={formData.CountryOfOrigin}
                  onChange={(e) => handleInputChange('CountryOfOrigin', e.target.value)}
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '0.375rem',
                    border: errors.CountryOfOrigin ? '1px solid var(--color-error)' : '1px solid var(--color-border)',
                    backgroundColor: 'var(--color-background)',
                    color: 'var(--color-text)',
                    fontSize: '1rem'
                  }}
                  aria-invalid={!!errors.CountryOfOrigin}
                  aria-describedby={errors.CountryOfOrigin ? 'country-error' : undefined}
                  disabled={countriesLoading}
                >
                  <option value="">Select a country</option>
                  {countries.map((country) => (
                    <option key={country.alpha3Code} value={country.name}>
                      {country.name}
                    </option>
                  ))}
                </select>
                {errors.CountryOfOrigin && (
                  <div id="country-error" style={{ color: 'var(--color-error)', fontSize: '0.875rem', marginTop: '0.25rem' }}>
                    {errors.CountryOfOrigin}
                  </div>
                )}
                {countriesLoading && (
                  <div style={{ fontSize: '0.875rem', marginTop: '0.25rem', color: 'var(--color-text-secondary)' }}>
                    Loading countries...
                  </div>
                )}
              </div>

              <Input
                icon={<MapPin />}
                label="Address *"
                value={formData.Address}
                onChange={(e) => handleInputChange('Address', e.target.value)}
                error={errors.Address}
                placeholder="123 Main St, City"
                {...getFieldAccessibilityProps('Address', errors.Address)}
              />

              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                  Hired Status *
                </label>
                <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input
                      type="radio"
                      name="Hired"
                      value="true"
                      checked={formData.Hired === true}
                      onChange={() => handleInputChange('Hired', true)}
                    />
                    Yes
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <input
                      type="radio"
                      name="Hired"
                      value="false"
                      checked={formData.Hired === false}
                      onChange={() => handleInputChange('Hired', false)}
                    />
                    No
                  </label>
                </div>
                {errors.Hired && (
                  <div style={{ color: 'var(--color-error)', fontSize: '0.875rem', marginTop: '0.25rem' }}>
                    {errors.Hired}
                  </div>
                )}
              </div>
            </div>
          </div>



          {/* Legacy Personal Information (Hidden but maintained for compatibility) */}
          <div style={{ display: 'none' }}>
            <Input
              value={formData.Name}
              onChange={(e) => handleInputChange('Name', e.target.value)}
            />
            <Input
              value={formData.EmailAddress}
              onChange={(e) => handleInputChange('EmailAddress', e.target.value)}
            />
            <Input
              value={formData.phone}
              onChange={(e) => handleInputChange('phone', e.target.value)}
            />
            <Input
              value={formData.Address}
              onChange={(e) => handleInputChange('Address', e.target.value)}
            />
          </div>

          {/* Skills, Evaluation, Status, and Notes removed as requested */}

          {errors.submit && (
            <div style={{ color: 'var(--color-error)', textAlign: 'center', marginTop: '1rem' }}>
              {errors.submit}
            </div>
          )}
        </div>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end', marginTop: '2rem' }}>
          <Button
            variant="outline"
            type="button"
            onClick={() => navigate('/')}
            disabled={submitting}
            aria-label="Cancel and return to dashboard"
          >
            Cancel
          </Button>
          <Button
            type="submit"
            icon={<Save />}
            disabled={submitting}
            aria-label={mode === 'create' ? 'Create applicant' : 'Update applicant'}
          >
            {submitting ? 'Saving...' : mode === 'create' ? 'Create Applicant' : 'Update Applicant'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};