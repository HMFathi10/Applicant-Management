# Applicant Management System

A full-stack web application for managing job applicants, built with .NET 9 backend API and React frontend with TypeScript.

## 🚀 Features

- **Applicant Management**: Create, read, update, and delete applicant records
- **Real-time Validation**: Comprehensive input validation on both frontend and backend
- **Responsive UI**: Modern, mobile-friendly interface
- **RESTful API**: Clean API architecture with proper error handling
- **Database Integration**: Entity Framework Core with SQL Server
- **Search & Filtering**: Advanced search capabilities for applicants
- **Export Functionality**: Export applicant data in various formats

## 📋 Prerequisites

### Backend Requirements
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** - LocalDB, SQL Server Express, or full SQL Server
- **Visual Studio 2022** (recommended) or **Visual Studio Code** with C# extension
- **Git** for version control

### Frontend Requirements
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **npm** (comes with Node.js) or **yarn**

### Development Tools (Optional but Recommended)
- **Postman** or **Insomnia** for API testing
- **SQL Server Management Studio (SSMS)** for database management
- **PowerShell** for running test scripts

## 🛠️ Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/applicant-management.git
cd applicant-management
```

### 2. Backend Setup

#### Navigate to Backend Directory
```bash
cd applicant-management-backend
```

#### Restore NuGet Packages
```bash
dotnet restore
```

#### Database Setup

1. **Ensure SQL Server is running** (LocalDB or SQL Server instance)

2. **Update Connection String** (if needed):
   Open `ApplicantManagement.API/appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=ApplicantManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Apply Database Migrations**:
   ```bash
   cd ApplicantManagement.API
   dotnet ef database update --project ../ApplicantManagement.Infrastructure/ApplicantManagement.Infrastructure.csproj
   ```

#### Run the Backend API
```bash
dotnet run --project ApplicantManagement.API
```

The API will start at `https://localhost:5001` or `http://localhost:5000`

### 3. Frontend Setup

#### Navigate to Frontend Directory
```bash
cd ../applicant-management-frontend
```

#### Install Dependencies
```bash
npm install
```

#### Configure API Endpoint
Update the API base URL in your environment configuration. The default should work if the backend is running on the standard ports.

#### Run Development Server
```bash
npm run dev
```

The frontend will start at `http://localhost:5173`

## 🔧 Configuration

### Backend Configuration

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ApplicantManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Environment Variables
You can override configuration using environment variables:
- `ConnectionStrings__DefaultConnection` - Database connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development, Staging, Production)
- `ASPNETCORE_URLS` - URLs the server should listen on

### Frontend Configuration

#### Environment Files
Create `.env` files for different environments:
- `.env.development` - Development environment
- `.env.production` - Production environment

Example `.env.development`:
```
VITE_API_BASE_URL=http://localhost:5259/api
VITE_APP_TITLE=Applicant Management System
```

## 🧪 Testing

### Backend Testing
```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter Category=Integration
```

### Frontend Testing
```bash
# Run tests
npm test

# Run tests with coverage
npm run test:coverage
```

### API Testing with PowerShell Scripts

We provide several PowerShell scripts for testing the API:

#### Simple Sequential Testing
```powershell
.\test-create-applicants-simple.ps1
```

#### Bulk Testing with Performance Metrics
```powershell
.\test-create-applicants-bulk.ps1
```

#### Corrected Testing (with proper validation)
```powershell
.\test-create-applicants-corrected.ps1
```

#### Update Applicant Testing
```powershell
.\test-update-applicant.ps1
```

#### Search Endpoint Testing
```powershell
.\test-search-endpoint.ps1
```

## 📊 Database Schema

### Applicants Table
```sql
CREATE TABLE Applicants (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    FamilyName NVARCHAR(100) NOT NULL,
    EmailAddress NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Age INT NOT NULL,
    CountryOfOrigin NVARCHAR(100) NOT NULL,
    Hired BIT NOT NULL DEFAULT 0,
    AppliedDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

## 🚀 Deployment

### Backend Deployment

1. **Publish the API**:
   ```bash
   dotnet publish ApplicantManagement.API -c Release -o ./publish
   ```

2. **Deploy to IIS** or **Azure App Service**

3. **Update connection string** for production database

### Frontend Deployment

1. **Build for production**:
   ```bash
   npm run build
   ```

2. **Deploy the `dist` folder** to your web server or CDN

3. **Configure environment variables** for production

## 📚 API Documentation

### Base URL
- Development: `http://localhost:5259/api`
- Production: `https://your-domain.com/api`

### Endpoints

#### Applicants
- `GET /applicants` - Get all applicants
- `GET /applicants/{id}` - Get applicant by ID
- `POST /applicants` - Create new applicant
- `PUT /applicants/{id}` - Update applicant
- `DELETE /applicants/{id}` - Delete applicant
- `GET /applicants/search?query={searchTerm}` - Search applicants

### Example API Requests

#### Create Applicant
```bash
curl -X POST http://localhost:5259/api/applicants \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John",
    "familyName": "Doe",
    "emailAddress": "john.doe@example.com",
    "phone": "+201234567890",
    "address": "123 Main St, Cairo, Egypt",
    "age": 30,
    "countryOfOrigin": "Egypt",
    "hired": false,
    "appliedDate": "2024-01-15T10:00:00"
  }'
```

#### Get All Applicants
```bash
curl http://localhost:5259/api/applicants
```

## 🐛 Troubleshooting

### Common Issues

#### Database Connection Issues
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Verify database permissions

#### CORS Issues
- Check CORS configuration in `Program.cs`
- Ensure frontend URL is allowed in development

#### Port Conflicts
- Backend default ports: 5000 (HTTP), 5001 (HTTPS)
- Frontend default port: 5173
- Change ports in configuration if needed

#### Build Errors
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete `bin` and `obj` folders and rebuild
- Update Node.js and npm to latest versions

### Validation Errors

#### Applicant Validation Rules
- **Name**: 5-100 characters, required
- **FamilyName**: 5-100 characters, required
- **EmailAddress**: Valid email format, required
- **Phone**: 10 digits after +20 prefix, required
- **Address**: 10-255 characters, required
- **Age**: 20-60 years, required
- **CountryOfOrigin**: Required, must be valid country
- **AppliedDate**: Valid date, required

## 📞 Support

For issues and questions:
1. Check the troubleshooting section
2. Review API logs in the `logs` folder
3. Check browser console for frontend errors
4. Create an issue in the repository

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with .NET 9 and React 19
- Uses Entity Framework Core for data access
- Styled with modern CSS and responsive design
- Testing scripts provided for comprehensive API validation