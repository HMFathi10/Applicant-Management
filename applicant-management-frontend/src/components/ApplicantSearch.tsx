import React, { useState, useEffect } from 'react';
import useDebounce from '../hooks/useDebounce';

interface ApplicantSearchProps {
  onSearch: (searchTerm: string) => void;
  placeholder?: string;
}

const ApplicantSearch: React.FC<ApplicantSearchProps> = ({ 
  onSearch, 
  placeholder = 'Search applicants...' 
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  useEffect(() => {
    onSearch(debouncedSearchTerm);
  }, [debouncedSearchTerm, onSearch]);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };

  return (
    <div className="search-container">
      <input
        type="text"
        className="search-input"
        placeholder={placeholder}
        value={searchTerm}
        onChange={handleChange}
        aria-label="Search applicants"
      />
    </div>
  );
};

export default ApplicantSearch;