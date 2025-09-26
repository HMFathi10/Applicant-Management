# Applicant Management System - API cURL Commands

This document provides standardized cURL commands for all CRUD operations in the Applicant Management System API.

## Base Information
- **Base URL**: `http://localhost:5259/api/Applicants`
- **Content-Type**: `application/json`
- **Accept**: `text/plain`

## CRUD Operations

### 1. CREATE Applicant (POST)

**Endpoint**: `POST /api/Applicants`

**cURL Command**:
```bash
curl -X 'POST' \
  'http://localhost:5259/api/Applicants' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "John",
    "familyName": "Doe",
    "emailAddress": "john.doe@example.com",
    "phone": "+201234567890",
    "department": "IT",
    "countryOfOrigin": "Egypt",
    "age": 30,
    "appliedDate": "2025-01-25T01:48:38.483Z",
    "hired": false
  }'
```

**Required Fields**:
- `name` (string): Applicant's first name
- `familyName` (string): Applicant's last name
- `emailAddress` (string): Valid email address
- `phone` (string): Egyptian format (+20XXXXXXXXXX)
- `department` (string): Department name
- `countryOfOrigin` (string): Country name
- `age` (integer): Applicant's age
- `appliedDate` (datetime): Application date in ISO format
- `hired` (boolean): Hiring status

### 2. READ All Applicants (GET)

**Endpoint**: `GET /api/Applicants`

**cURL Command**:
```bash
curl -X 'GET' \
  'http://localhost:5259/api/Applicants' \
  -H 'accept: text/plain'
```

### 3. READ Applicant by ID (GET)

**Endpoint**: `GET /api/Applicants/{id}`

**cURL Command**:
```bash
curl -X 'GET' \
  'http://localhost:5259/api/Applicants/1' \
  -H 'accept: text/plain'
```

**Note**: This endpoint currently returns empty results as it's not fully implemented.

### 4. UPDATE Applicant (PUT)

**Endpoint**: `PUT /api/Applicants/{id}`

**cURL Command**:
```bash
curl -X 'PUT' \
  'http://localhost:5259/api/Applicants/1' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "John",
    "familyName": "Smith",
    "emailAddress": "john.smith@example.com",
    "phone": "+201234567890",
    "department": "HR",
    "countryOfOrigin": "Egypt",
    "age": 31,
    "appliedDate": "2025-01-25T01:48:38.483Z",
    "hired": true
  }'
```

**Note**: This endpoint currently doesn't perform actual updates as it's not fully implemented.

### 5. DELETE Applicant (DELETE)

**Endpoint**: `DELETE /api/Applicants/{id}`

**cURL Command**:
```bash
curl -X 'DELETE' \
  'http://localhost:5259/api/Applicants/1' \
  -H 'accept: text/plain'
```

**Note**: This endpoint currently doesn't perform actual deletion as it's not fully implemented.

### 6. SEARCH Applicants (GET)

**Endpoint**: `GET /api/Applicants/search?query={searchTerm}`

**cURL Command**:
```bash
curl -X 'GET' \
  'http://localhost:5259/api/Applicants/search?query=John' \
  -H 'accept: text/plain'
```

**Note**: This endpoint currently fails with 500 Internal Server Error due to model binding issues.

### 7. FILTER Applicants (GET)

**Endpoint**: `GET /api/Applicants/filter?experienceMin={min}&experienceMax={max}`

**cURL Command**:
```bash
curl -X 'GET' \
  'http://localhost:5259/api/Applicants/filter?experienceMin=20&experienceMax=30' \
  -H 'accept: text/plain'
```

**Note**: This endpoint currently fails with 500 Internal Server Error due to model binding issues.

## Validation Rules

### Phone Number Format
- Must follow Egyptian format: `+20XXXXXXXXXX` (10 digits after +20)
- Example: `+201234567890`

### Email Address Format
- Must be a valid email format
- Example: `john.doe@example.com`

### Required Fields
All fields in the CREATE operation are required and cannot be null or empty.

## Error Handling

### Success Responses
- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operation (returns applicant ID)
- **204 No Content**: Successful DELETE operation

### Error Responses
- **400 Bad Request**: Invalid request data
- **500 Internal Server Error**: Server-side errors

## Testing Examples

### Create Multiple Test Applicants
```bash
# Applicant 1
curl -X 'POST' \
  'http://localhost:5259/api/Applicants' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "Alice",
    "familyName": "Johnson",
    "emailAddress": "alice.johnson@example.com",
    "phone": "+201234567891",
    "department": "Marketing",
    "countryOfOrigin": "Egypt",
    "age": 28,
    "appliedDate": "2025-01-20T10:30:00.000Z",
    "hired": false
  }'

# Applicant 2
curl -X 'POST' \
  'http://localhost:5259/api/Applicants' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "Bob",
    "familyName": "Williams",
    "emailAddress": "bob.williams@example.com",
    "phone": "+201234567892",
    "department": "Sales",
    "countryOfOrigin": "Egypt",
    "age": 35,
    "appliedDate": "2025-01-22T14:15:00.000Z",
    "hired": true
  }'
```

### Test All Operations in Sequence
```bash
# 1. Create an applicant
# 2. Get all applicants to verify creation
# 3. Search for specific applicants
# 4. Filter by experience (age) range
# 5. Update an applicant
# 6. Delete an applicant
```

## Notes

1. **Current Limitations**:
   - UPDATE and DELETE operations are not fully implemented
   - SEARCH and FILTER operations have model binding issues
   - GET by ID returns empty results

2. **Data Format**: All JSON payloads must use PascalCase property names matching the `CreateApplicantCommand` structure.

3. **Testing**: Use the provided cURL commands to test the API endpoints and verify functionality.