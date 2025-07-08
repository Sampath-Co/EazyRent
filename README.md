# EazyRent Property Management System

<!-- ===================== -->
<!--   ARCHITECTURE IMAGE  -->
<!-- Insert overall architecture diagram here -->
<!-- ===================== -->

## Project Overview

EazyRent is a full-stack property management platform for property owners and tenants. It enables:

- Owners to manage properties, leases, maintenance, and payments
- Tenants to browse properties, request leases (with digital signature), submit maintenance, and pay rent

Built with:

- **Backend:** ASP.NET Core 8, Entity Framework Core, JWT Auth, SQL Server
- **Frontend:** Angular 20, Bootstrap 5, RxJS, ngx-toastr

---

# Backend (ASP.NET Core)

## Tech Stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core (SQL Server)
- JWT Authentication
- AutoMapper
- Swagger (API docs)

## Setup & Configuration

1. Ensure SQL Server is running and update `appsettings.json` with your connection string.
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Apply migrations (if any):
   ```bash
   dotnet ef database update
   ```
4. Run the API:
   ```bash
   dotnet run
   ```
   The API will be available at `https://localhost:5001` (or as configured).

## Main Features

- User registration/login (Owner, Tenant)
- JWT-based authentication & role-based authorization
- Property CRUD (with image upload)
- Lease management (request, approve/reject, digital signature)
- Maintenance requests (tenant submit, owner manage)
- Payment tracking
- Swagger UI for API exploration

## API Controllers

- **UserController**: Auth, registration, login
  <!-- Insert UserController image/diagram here -->
- **OwnerController**: Owner dashboard, stats
  <!-- Insert OwnerController image/diagram here -->
- **TenantController**: Tenant dashboard, stats
  <!-- Insert TenantController image/diagram here -->
- **PropertyController**: CRUD for properties
  <!-- Insert PropertyController image/diagram here -->
- **LeaseController**: Lease requests, approvals
  <!-- Insert LeaseController image/diagram here -->
- **MaintenanceRequestController**: Maintenance workflow
  <!-- Insert MaintenanceRequestController image/diagram here -->
- **PaymentController**: Rent payments
  <!-- Insert PaymentController image/diagram here -->

## Data Models & DTOs

- User, Property, Lease, MaintenanceRequest, Payment
- DTOs for all major API contracts (see `/Models/DTO`)
  <!-- Insert data model diagram here -->

## Services & Repositories

- JWT token service
- Property/Lease/Payment/Maintenance repositories
  <!-- Insert service/repository diagram here -->

---

# Frontend (Angular)

## Tech Stack

- Angular 20
- Bootstrap 5 & Bootstrap Icons
- RxJS
- ngx-toastr

## Setup & Configuration

1. Install dependencies:
   ```bash
   npm install
   ```
2. Start the dev server:
   ```bash
   ng serve
   ```
   App runs at [http://localhost:4200](http://localhost:4200)

## Main Features

- Owner & Tenant role-based dashboards
- Property management (add, update, delete, filter, image upload)
- Lease requests (with digital signature)
- Maintenance requests
- Rent payments (UPI/Card)
- Secure routing (guards, interceptors)
- Toastr notifications

## Main Components

- **Navbar**: Role-based navigation
  <!-- Insert Navbar image here -->
- **Home**: Landing page
  <!-- Insert Home image here -->
- **User**: Login, registration, dashboards
  - Login
    <!-- Insert Login image here -->
  - Registration
    <!-- Insert Registration image here -->
  - OwnerDashboard
    <!-- Insert OwnerDashboard image here -->
  - TenantDashboard
    <!-- Insert TenantDashboard image here -->
- **PropertyPage**: Property details
  <!-- Insert PropertyPage image here -->
- **PaymentDashboard**: Payment status
  <!-- Insert PaymentDashboard image here -->
- **MaintenanceDashboardOwner**: Owner maintenance view
  <!-- Insert MaintenanceDashboardOwner image here -->
- **MaintenanceDashboardTenant**: Tenant maintenance view
  <!-- Insert MaintenanceDashboardTenant image here -->
- **GetLeaseOwner**: Owner lease approvals
  <!-- Insert GetLeaseOwner image here -->
- **Forbidden**: Access denied page
  <!-- Insert Forbidden image here -->

## Component-to-Controller Mapping

This section details how each major frontend component interacts with the backend, including API endpoints, backend controller logic, and image placeholders for each component.

---

### 1. User Login & Registration

**Frontend Components:**

- Login (`/src/app/components/user/login/`)
- Registration (`/src/app/components/user/registration/`)

**Backend Controller:**

- UserController.cs

**API Endpoints:**

- `POST /User/Login` — User login (returns JWT)
- `POST /User/Register` — User registration

**Backend Logic:**

- **Login:** Validates credentials, issues JWT, returns user roles.
- **Register:** Creates new user (Owner or Tenant), hashes password, stores user.

**Key Controller Code:**

```csharp
// UserController.cs
[HttpPost("register")]
public async Task<IActionResult> RegisterOwner(RegistrationDTO dto) { ... }

[HttpPost("login")]
public IActionResult Login(LoginDTO dto) { ... }
```

- **RegisterOwner:** Maps registration DTO to User, saves to DB, returns a message based on role.
- **Login:** Validates credentials, issues JWT if successful.

**Key DTOs:**

```csharp
// LoginDTO.cs
public class LoginDTO {
    [Required]
    [DataType(DataType.EmailAddress)]
    public String Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public String Password { get; set; }
}

// RegistrationDTO.cs
public class RegistrationDTO {
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string Role { get; set; }
}
```

<!-- Insert Login/Registration flow image here -->

---

### 2. Owner Dashboard

**Frontend Component:**

- OwnerDashboard (`/src/app/components/user/ownerdashboard/`)

**Backend Controllers:**

- OwnerController.cs — Dashboard stats, owner-specific data
- PropertyController.cs — Property CRUD
- LeaseController.cs — Lease approvals/rejections
- MaintenanceRequestController.cs — Maintenance requests
- PaymentController.cs — Payment history

**API Endpoints & Logic:**

- `GET /Owner/DashboardStats` — Owner stats (properties, tenants, revenue)
- `GET /Property/Owner/{ownerId}` — List owner's properties
- `POST /Property` — Add property
- `PUT /Property/{id}` — Update property
- `DELETE /Property/{id}` — Delete property
- `GET /Lease/Owner/Applications` — Pending lease applications
- `POST /Lease/Approve` — Approve lease (with digital signature)
- `POST /Lease/Reject` — Reject lease
- `GET /Maintenance/Owner/{ownerId}` — Owner's maintenance requests
- `PUT /Maintenance/{id}/Status` — Update maintenance status
- `GET /Payment/Owner/{ownerId}` — Payment history

**Key Controller Code:**

```csharp
// OwnerController.cs
[Authorize(Roles = "Owner")]
[HttpPost("/Owner/ApproveRejectLease")]
public async Task<IActionResult> ApproveRejectLease([FromBody] ApproveRejectLeaseDto dto) { ... }

[Authorize(Roles = "Owner")]
[HttpDelete("/Owner/DeleteLease/{leaseId}")]
public async Task<IActionResult> DeleteLease(int leaseId) { ... }
```

- **ApproveRejectLease:** Owner approves/rejects a lease, checks ownership, updates status.
- **DeleteLease:** Owner deletes a lease if all related payments are marked as "Paid".

<!-- Insert OwnerDashboard image here -->

---

### 3. Tenant Dashboard

**Frontend Component:**

- TenantDashboard (`/src/app/components/user/tenantdashboard/`)

**Backend Controllers:**

- TenantController.cs — Tenant dashboard data
- PropertyController.cs — List available properties
- LeaseController.cs — Tenant's leases, lease requests
- MaintenanceRequestController.cs — Tenant's maintenance requests
- PaymentController.cs — Tenant's payment history

**API Endpoints & Logic:**

- `GET /Tenant/Properties` — List available properties
- `GET /api/Lease/Tenant/Leases` — Tenant's leases
- `POST /api/Lease/Tenant/RequestLease` — Request a lease (with digital signature)
- `GET /Tenant/GetAllMaintenance` — Tenant's maintenance requests
- `POST /Maintenance` — Submit maintenance request
- `GET /Payment/Tenant/{tenantId}` — Payment history

**Key Controller Code:**

```csharp
// TenantController.cs
[HttpGet("/Tenant/Properties")]
public async Task<IActionResult> GetAllProperties([FromQuery] string? filterOn, [FromQuery] string? filterQuery, [FromQuery] decimal? filterRent) { ... }
```

- **GetAllProperties:** Returns all available properties, supports filtering by criteria.

<!-- Insert TenantDashboard image here -->

---

### 4. Property Management

**Frontend Components:**

- AddProperty (`/src/app/features/Property/add-property/`)
- PropertyList (`/src/app/features/Property/property-list/`)
- UpdateProperty (`/src/app/features/Property/update-property/`)
- PropertyPage (`/src/app/components/property-page/`)

**Backend Controller:**

- PropertyController.cs

**API Endpoints & Logic:**

- `POST /Property` — Add property (with image upload)
- `GET /Property` — List all properties
- `GET /Property/{id}` — Get property details
- `PUT /Property/{id}` — Update property
- `DELETE /Property/{id}` — Delete property

**Key Controller Code:**

```csharp
// PropertyController.cs
[HttpGet("/Owner/Properties")]
[Authorize(Roles = "Owner")]
public async Task<IActionResult> GetMyPropertiesAsOwner() { ... }

[Authorize(Roles = "Owner")]
[HttpPost("/Owner/AddProperty")]
public async Task<IActionResult> AddProperty([FromForm] PropertyDetailsDTO dto) { ... }

[Authorize(Roles = "Tenant")]
[HttpGet("/Tenant/GetPropertyById/{propertyId}")]
public async Task<IActionResult> GetPropertyById(int propertyId) { ... }

[HttpPut("/Owner/UpdateProperty/{propertyId}")]
[Authorize(Roles = "Owner")]
public async Task<IActionResult> UpdateProperty(int propertyId, [FromForm] PropertyDetailsDTO updatedPropertyDetails) { ... }

[HttpDelete("/Owner/DeleteProperty/{propertyId}")]
[Authorize(Roles = "Owner")]
public async Task<IActionResult> DeleteProperty(int propertyId) { ... }
```

- **GetMyPropertiesAsOwner:** Owner retrieves their properties.
- **AddProperty:** Owner adds a property with image upload.
- **GetPropertyById:** Tenant/Owner retrieves property details.
- **UpdateProperty:** Owner updates property details.
- **DeleteProperty:** Owner deletes a property if allowed.

<!-- Insert Property Management image here -->

---

### 5. Lease Management

**Frontend Components:**

- GetLeaseOwner (`/src/app/components/getleaseowner/`)
- LeaseFormTenant (`/src/app/leaseformtenant/`)
- AgreementPage (`/src/app/agreement-page/`)

**Backend Controller:**

- LeaseController.cs

**API Endpoints & Logic:**

- `GET /Lease/Owner/Applications` — Owner: view lease applications
- `POST /Lease/Approve` — Owner: approve lease (with digital signature)
- `POST /Lease/Reject` — Owner: reject lease
- `POST /api/Lease/Tenant/RequestLease` — Tenant: request lease

**Key Controller Code:**

```csharp
// LeaseController.cs
[Authorize(Roles = "Tenant")]
[HttpPost("Tenant/RequestLease")]
public async Task<IActionResult> RequestLease([FromForm] CreateLeaseDTO dto) { ... }

[Authorize(Roles = "Tenant")]
[HttpGet("Tenant/Leases")]
public async Task<IActionResult> GetMyLeasesAsTenant() { ... }

[Authorize(Roles = "Owner")]
[HttpGet("Owner/Leases")]
public async Task<IActionResult> GetMyLeasesAsOwner() { ... }

[Authorize(Roles = "Owner")]
[HttpDelete("Owner/DeleteLease/{leaseId:int}")]
public async Task<IActionResult> DeleteLease(int leaseId) { ... }
```

- **RequestLease:** Tenant requests a lease, creates an initial payment.
- **GetMyLeasesAsTenant/Owner:** Returns leases for the current user.
- **DeleteLease:** Owner deletes a lease if all payments are completed.

<!-- Insert Lease Management image here -->

---

### 6. Maintenance Requests

**Frontend Components:**

- MaintenanceDashboardOwner (`/src/app/components/maintenancedashboardowner/`)
- MaintenanceDashboardTenant (`/src/app/components/maintenancedashboardtenant/`)

**Backend Controller:**

- MaintenanceRequestController.cs

**API Endpoints & Logic:**

- `GET /Maintenance/Owner/{ownerId}` — Owner: view maintenance requests
- `GET /Tenant/GetAllMaintenance` — Tenant: view own requests
- `POST /Maintenance` — Tenant: submit request
- `PUT /Maintenance/{id}/Status` — Owner: update status

**Key Controller Code:**

```csharp
// MaintenanceRequestController.cs
[Authorize(Roles = "Owner")]
[HttpGet("/Owner/GetAllMaintenance/")]
public async Task<IActionResult> GetAllRequests() { ... }

[Authorize(Roles = "Tenant")]
[HttpGet("/Tenant/GetAllMaintenance/")]
public async Task<IActionResult> GetAllRequestsByTenant() { ... }

[Authorize(Roles = "Tenant")]
[HttpPost("/Tenant/CreateMaintenanceRequest/")]
public async Task<IActionResult> AddRequest([FromBody] CreateMaintenceRequestDto dto) { ... }

[Authorize(Roles = "Owner")]
[HttpPut("Owner/Update/{requestId:int}")]
public async Task<IActionResult> UpdateMaintenanceStatus([FromRoute] int requestId, [FromBody] string status) { ... }

[Authorize(Roles = "Owner")]
[HttpDelete("/Owner/DeleteMaintenance/{requestId:int}")]
public async Task<IActionResult> DeleteMaintenanceRequest([FromRoute] int requestId) { ... }
```

- **GetAllRequests/GetAllRequestsByTenant:** Owner/Tenant retrieves maintenance requests.
- **AddRequest:** Tenant submits a maintenance request.
- **UpdateMaintenanceStatus:** Owner updates the status of a request.
- **DeleteMaintenanceRequest:** Owner deletes a request if its status is "terminated".

<!-- Insert Maintenance Requests image here -->

---

### 7. Payments

**Frontend Components:**

- PaymentDashboard (`/src/app/components/payment-dashboard/`)
- PaymentPage (`/src/app/payment-page/`)

**Backend Controller:**

- PaymentController.cs

**API Endpoints & Logic:**

- `GET /Payment/Owner/{ownerId}` — Owner: payment history
- `GET /Payment/Tenant/{tenantId}` — Tenant: payment history
- `POST /Payment` — Tenant: make payment

**Key Controller Code:**

```csharp
// PaymentController.cs
[HttpGet("/Payments")]
[Authorize(Roles = "Tenant,Owner")]
public async Task<IActionResult> GetPayments() { ... }

[HttpPut("/Tenant/UpdatePaymentStatus/{leaseId}")]
[Authorize(Roles = "Tenant")]
public async Task<IActionResult> UpdatePaymentStatus(int leaseId) { ... }

[HttpGet("/Lease/{leaseId}/Payments")]
public async Task<IActionResult> GetPaymentsByLeaseId(int leaseId) { ... }

[HttpPost("Create")]
[Authorize(Roles = "Tenant")]
public async Task<IActionResult> CreatePayment([FromBody] PaymentDTO paymentDto) { ... }
```

- **GetPayments:** Owner/Tenant retrieves their payments.
- **UpdatePaymentStatus:** Tenant marks a payment as paid.
- **GetPaymentsByLeaseId:** Retrieves payments for a specific lease.
- **CreatePayment:** Tenant creates a new payment.

<!-- Insert Payment image here -->

---

### 8. Navigation & Access Control

**Frontend Components:**

- Navbar (`/src/app/components/navbar/`)
- Forbidden (`/src/app/components/forbidden/`)

**Backend Logic:**

- JWT authentication and role-based authorization handled in backend (middleware, not a specific controller).

---

## Feature Modules

- **AddProperty**: Add new property
  <!-- Insert AddProperty image here -->
- **PropertyList**: List/filter properties
  <!-- Insert PropertyList image here -->
- **UpdateProperty**: Edit property
  <!-- Insert UpdateProperty image here -->

---

# How to Run (Full Stack)

1. Start the backend API (see above)
2. Start the frontend Angular app (see above)
3. Ensure CORS is enabled for `http://localhost:4200` in backend
4. Use the app at [http://localhost:4200](http://localhost:4200)

---

# Contribution

- Fork the repo, create a branch, submit a PR
- Please add tests and update docs as needed

# License

- [Specify your license here]

# Contact

- [Your contact info or team email]
