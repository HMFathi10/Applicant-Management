# Quick Start Guide

Get the Applicant Management System up and running in minutes with this quick start guide.

## ⚡ Super Quick Start (5 minutes)

### 1. Prerequisites Check
- [ ] .NET 9.0 SDK installed
- [ ] Node.js 18+ installed
- [ ] SQL Server (LocalDB) running

### 2. Backend Setup (2 minutes)
```bash
cd applicant-management-backend
dotnet restore
cd ApplicantManagement.API
dotnet ef database update --project ../ApplicantManagement.Infrastructure/ApplicantManagement.Infrastructure.csproj
dotnet run
```

### 3. Frontend Setup (2 minutes)
```bash
# In a new terminal
cd applicant-management-frontend
npm install
npm run dev
```

### 4. Open Your Browser (1 minute)
- Frontend: http://localhost:5173
- API Swagger: https://localhost:5001/swagger

## 🎯 First Steps

### Create Your First Applicant
1. Open http://localhost:5173
2. Click "Create New Applicant"
3. Fill in the form:
   - Name: "John"
   - Family Name: "Doe"
   - Email: "john.doe@example.com"
   - Phone: "+201234567890"
   - Address: "123 Main Street, Cairo, Egypt"
   - Age: 30
   - Country: "Egypt"
   - Applied Date: Today
4. Click "Submit"

### Test the API
```bash
# Get all applicants
curl http://localhost:5259/api/applicants

# Create applicant with curl
curl -X POST http://localhost:5259/api/applicants \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane","familyName":"Smith","emailAddress":"jane@example.com","phone":"+201234567890","address":"456 Oak Ave, Alexandria, Egypt","age":28,"countryOfOrigin":"Egypt","hired":false,"appliedDate":"2024-01-15T10:00:00"}'
```

### Run Tests
```bash
# Backend tests
cd applicant-management-backend
dotnet test

# API load test (PowerShell)
cd ..\..\.\test-create-applicants-corrected.ps1
```

## 🚨 Common Quick Fixes

### Port Already in Use
```bash
# Kill processes on ports 5000, 5001, 5173
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Database Issues
```bash
# Reset database
cd applicant-management-backend/ApplicantManagement.API
dotnet ef database drop --force
dotnet ef database update
```

### Frontend Won't Start
```bash
# Clear cache and reinstall
cd applicant-management-frontend
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### API Connection Issues
- Check if backend is running: `curl http://localhost:5259/api/applicants`
- Verify CORS settings in `Program.cs`
- Check firewall/antivirus blocking ports

## 📊 Verify Everything Works

### Backend Health Check
```bash
curl http://localhost:5259/api/applicants | head -c 100
```

### Frontend Health Check
Open browser console (F12) and check for errors

### Database Check
```bash
# Count applicants
curl http://localhost:5259/api/applicants | jq length
```

## 🎉 Success!

If you can:
- [ ] See the frontend at http://localhost:5173
- [ ] Create an applicant
- [ ] View the applicant list
- [ ] Get API response from http://localhost:5259/api/applicants

You're ready to go! Check the full README.md for advanced configuration and deployment options.