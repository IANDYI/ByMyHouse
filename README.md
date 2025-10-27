# 🏠 BuyMyHouse
### Cloud-Based Estate Agency Platform

A microservices-based estate agency system built with .NET 8 and Azure Functions for managing house listings, processing mortgage applications, and automating offer generation.

---

## 🎯 Overview

This project consists of **three independent microservices** working together to provide a complete estate agency platform:

| Service | Technology | Purpose |
|---------|-----------|---------|
| **Listings API** | SQL Server + Blob Storage | House listing management with image storage |
| **Mortgage API** | Azure Table Storage (CQRS) | Mortgage application handling |
| **Azure Functions** | Timer-triggered jobs | Batch processing & notifications |

### Business Rules

The system implements the following mortgage evaluation criteria:

- ✓ Minimum annual income requirement: **€25,000**
- ✓ Maximum loan amount calculation: **4.5× annual income**
- ✓ Interest rate range: **3.0% - 5.5%** (based on loan-to-income ratio)
- ✓ Standard mortgage term: **25 years**
- ✓ Offer validity period: **14 days**
- ✓ Automated document generation and email notifications
- ✓ Image storage in Azure Blob Storage for property listings

---

## 📋 Prerequisites

Before running the application, ensure you have the following installed:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download)** - Framework for the APIs
2. **[Azurite](https://www.npmjs.com/package/azurite)** - Local Azure Storage emulator
   ```bash
   npm install -g azurite
   ```
3. **[Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)** - For running Functions locally
   ```bash
   npm install -g azure-functions-core-tools@4
   ```

> **Note:** SQL Server LocalDB is automatically included with the .NET SDK installation.

---

## 🚀 Quick Start

### Starting the Application

```bash
cd BuyMyHouse
chmod +x start-all.sh
./start-all.sh
```

> **Note:** The script starts all services in the background. To view logs:
> ```bash
> tail -f /tmp/listings.log      # Listings API
> tail -f /tmp/mortgage.log      # Mortgage API
> tail -f /tmp/functions.log     # Azure Functions
> ```

### Stopping the Application

To stop all services:

```bash
chmod +x stop-all.sh
./stop-all.sh
```

Alternatively, manually kill processes:
```bash
pkill -9 -f azurite
pkill -9 -f BuyMyHouse
pkill -9 -f "dotnet run"
pkill -9 -f "func start"
```

> **Note:** The compiled executables run as `BuyMyHouse.Listings` and `BuyMyHouse.Mortgage` processes, so you may need to kill those specifically if running as compiled binaries.

### Manual Startup (Alternative)

If the startup scripts don't work, you can start each service manually in separate terminal windows:

#### **Terminal 1 - Azurite**
```bash
azurite --silent --location ~/azurite
```

#### **Terminal 2 - Listings API**
```bash
cd BuyMyHouse.Listings
dotnet run
```

#### **Terminal 3 - Mortgage API**
```bash
cd BuyMyHouse.Mortgage
dotnet run
```

#### **Terminal 4 - Azure Functions**
```bash
cd BuyMyHouse.Functions
func start
```

### HTTPS Certificate Setup

If you encounter certificate warnings when accessing the APIs in your browser:

```bash
dotnet dev-certs https --trust
```

Then restart all services.

> **Note:** The APIs are configured to automatically use SQLite on Mac/Linux, and SQL Server LocalDB on Windows.

---

## 🌐 Service Endpoints

Once everything is running, access the services at:

| Service | URL | Description |
|---------|-----|-------------|
| Listings API | `https://localhost:5001/swagger` | House listings and images |
| Mortgage API | `https://localhost:5002/swagger` | Mortgage applications |
| Azure Functions | `http://localhost:7071` | Function monitoring |

---

## 🧪 Testing Guide

### Step 1: Test House Listings

**Option A - Browser:**
Navigate to `https://localhost:5001/swagger` in your browser

**Option B - Command Line:**
```bash
curl https://localhost:5001/api/houses
```

You should receive a JSON response containing **2 pre-seeded house listings**:

**House 1 - Amsterdam**
- Address: Prinsengracht 112, Amsterdam
- Price: €520,000
- Size: 85 m², 2 bedrooms, 1 bathroom
- Description: Charming traditional canal house with wooden beams and garden access

**House 2 - Alkmaar**
- Address: Langestraat 45B, Alkmaar
- Price: €420,000
- Size: 110 m², 3 bedrooms, 2 bathrooms
- Description: Contemporary townhouse with open-plan living and private terrace

### Step 2: Upload House Images (Optional)

**Via Swagger UI:**
1. Go to `https://localhost:5001/swagger`
2. Find the endpoint: `POST /api/houses/{id}/images`
3. Enter house ID: `1` or `2`
4. Upload an image file (supported formats: jpg, png, gif)
5. The system will return an image URL and store it in Blob Storage

**Via Command Line:**
```bash
curl -X POST https://localhost:5001/api/houses/1/images \
  -F "image=@/path/to/your/image.jpg"
```

### Step 3: Submit a Mortgage Application

**Using Swagger UI:**
1. Open `https://localhost:5002/swagger`
2. Locate: `POST /api/mortgageapplications`
3. Use this sample payload:
   ```json
   {
  "applicantEmail": "alex.brown@example.com",
  "applicantName": "Alex Brown",
  "annualIncome": 65000,
  "requestedAmount": 280000,
     "houseId": 1
   }
   ```
4. Copy the `applicationId` from the response for verification

**Expected Response:**
```json
{
  "id": 1,
  "applicantEmail": "alex.brown@example.com",
  "applicantName": "Alex Brown",
  "annualIncome": 65000,
  "requestedAmount": 280000,
  "houseId": 1,
  "applicationDate": "2025-11-27T22:53:00Z",
  "status": "Pending"
}
```

**Using curl:**
```bash
curl -X POST https://localhost:5002/api/mortgageapplications \
  -H "Content-Type: application/json" \
  -d '{
    "applicantEmail": "emma.taylor@example.com",
    "applicantName": "Emma Taylor",
    "annualIncome": 58000,
    "requestedAmount": 260000,
    "houseId": 1
  }'
```

### Step 4: Trigger Azure Functions Manually

The functions normally run on an automated schedule (daily at **23:00** and **09:00**), but you can trigger them manually for testing:

#### Process Pending Applications

```bash
curl http://localhost:7071/api/test-process
```

#### Send Offer Emails

```bash
curl http://localhost:7071/api/test-send
```

> 💡 **Tip:** Watch the logs to see detailed information:
> - Application approval/rejection status
> - Email sending confirmations  
> - Generated document URLs
> 
> View logs with: `tail -f /tmp/functions.log`

### Step 5: Verify Stored Data

#### View Table Storage (Applications)

1. Install [Azure Storage Explorer](https://azure.microsoft.com/features/storage-explorer/)
2. Connect to **"Local Emulator"**
3. Expand the **`MortgageApplications`** table to view submitted applications and their status

#### View Blob Storage (Documents & Images)

In Azure Storage Explorer:
1. Expand **"Blob Containers"**
2. Open **`mortgage-offers`** container → Generated mortgage offer documents
3. Open **`house-images`** container → Uploaded property images

---

## ✅ Expected Behavior

After completing the tests, you should observe:

- [x] Listings API successfully returns 2 houses
- [x] Mortgage API creates applications with **"Pending"** status
- [x] Process Function updates applications to **"Approved"** or **"Rejected"** (visible in logs)
- [x] Send Function logs email notifications for approved applications
- [x] Storage Explorer displays applications in Table Storage and generated documents in Blob Storage

### Test Scenarios

**Test Case 1 - Approved Application:**
```json
{
  "applicantEmail": "test.approved@example.com",
  "applicantName": "Test User",
  "annualIncome": 65000,
  "requestedAmount": 250000,
  "houseId": 1
}
```
Expected: Approved (income €25k+, loan within 4.5× income limit of €292,500)

**Test Case 2 - Rejected (Income Too Low):**
```json
{
  "applicantEmail": "test.rejected@example.com",
  "applicantName": "Test User",
  "annualIncome": 20000,
  "requestedAmount": 80000,
  "houseId": 1
}
```
Expected: Rejected (income below minimum threshold of €25k)

**Test Case 3 - Rejected (Loan Too High):**
```json
{
  "applicantEmail": "test.rejected2@example.com",
  "applicantName": "Test User",
  "annualIncome": 50000,
  "requestedAmount": 250000,
  "houseId": 2
}
```
Expected: Rejected (loan €250k exceeds 4.5× income limit of €225k)

### Expected Mortgage Calculation Examples

With annual income of €65,000:
- Maximum loan: €292,500 (4.5 × €65,000)
- Interest rate tiers: 3.0% (low loan), up to 5.5% (high loan ratio)
- Monthly payment for €250k at 4.0% over 25 years: ~€1,320

---

## 🔧 Troubleshooting

### Issue: Listings API Won't Start (LocalDB Error on Mac/Linux)

If you see "LocalDB is not supported on this platform" error, the project has been configured to automatically use SQLite on Mac/Linux. Make sure you have restored packages:

```bash
dotnet restore
```

Then restart the Listings API.

### Issue: Ports Already in Use

```bash
# Check what's using the ports
lsof -i :5001 :5002 :7071

# Kill the process (replace <PID> with actual ID)
kill -9 <PID>
```

### Issue: Azurite Not Working

```bash
# Stop all instances
pkill -f azurite

# Restart
azurite --silent --location ~/azurite
```

### Issue: Build Errors

Clean and rebuild the solution:

```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Additional Information

- The system uses **CQRS** (Command Query Responsibility Segregation) pattern for mortgage applications
- Image uploads are automatically validated for file type and size
- Mortgage offers are stored as text documents in Blob Storage
- Email notifications are currently logged (no actual emails sent in development mode)

For more detailed API documentation, visit the Swagger UI endpoints after starting the services.