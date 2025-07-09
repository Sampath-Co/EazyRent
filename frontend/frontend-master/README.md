# Real Estate Property Management Frontend

This project is a comprehensive property management web application built with **Angular 20**. It enables property owners and tenants to manage properties, leases, maintenance requests, and payments through a modern, role-based dashboard interface.

---

## Table of Contents

- [Real Estate Property Management Frontend](#real-estate-property-management-frontend)
  - [Table of Contents](#table-of-contents)
  - [Project Overview](#project-overview)
  - [Architecture \& Folder Structure](#architecture--folder-structure)
  - [Key Features](#key-features)
  - [User Roles \& Workflows](#user-roles--workflows)
    - [Owner](#owner)
    - [Tenant](#tenant)
  - [Core Angular Concepts Used](#core-angular-concepts-used)
  - [Services \& State Management](#services--state-management)
  - [Authentication \& Authorization](#authentication--authorization)
  - [Routing \& Navigation](#routing--navigation)
  - [Forms \& Validation](#forms--validation)
  - [File Uploads](#file-uploads)
  - [Notifications \& Error Handling](#notifications--error-handling)
  - [Styling \& UI](#styling--ui)
  - [Environment \& Configuration](#environment--configuration)
  - [Testing](#testing)
  - [How to Run](#how-to-run)
  - [Dependencies](#dependencies)

---

## Project Overview

This application allows:

- **Owners** to add, update, and manage properties, review lease applications, approve/reject leases, view maintenance requests, and track payments.
- **Tenants** to browse properties, request leases (with digital signature upload), view their leases, submit maintenance requests, and make rent payments.

The app is built with a modular, service-oriented Angular architecture, using RxJS for state management and HttpClient for API communication.

---

## Architecture & Folder Structure

```
frontend/
  src/app/
    components/         # UI components (dashboard, navbar, etc.)
    features/           # Feature modules (property management)
    shared/             # Shared services, guards, interceptors, constants
    models/             # Data models (if any)
    environments/       # Environment configs
    ...
```

- **components/**: Contains all major UI components for owners and tenants.
- **features/Property/**: Add, list, and update property features.
- **shared/services/**: Core business logic (auth, property, lease, maintenance, payment, shared data).
- **shared/auth-guard.ts**: Route protection based on authentication and roles.
- **shared/auth-interceptor.ts**: Automatically attaches JWT tokens to HTTP requests.
- **environments/**: API URLs and environment-specific settings.

---

## Key Features

- **User Registration & Login** (with role selection: Owner/Tenant)
- **Role-based Dashboards**
  - Owner: Property management, lease approvals, maintenance, payments, stats
  - Tenant: Property browsing, lease requests, maintenance, payments
- **Property Management**: Add, update, delete, and filter properties (with image upload)
- **Lease Management**: Tenants request leases (with digital signature), owners approve/reject
- **Maintenance Requests**: Tenants submit, owners review and update status
- **Payment System**: Tenants pay rent, payment status tracked per lease
- **Notifications**: Toastr for user feedback (success, error, info)
- **Secure Routing**: Guards and interceptors for protected routes and API calls

---

## User Roles & Workflows

### Owner

- Register/login as Owner
- Add/manage properties (address, rent, status, image, description)
- View dashboard stats (properties, tenants, applications, maintenance, revenue)
- Review lease applications, approve/reject with digital signature
- View/manage maintenance requests
- Track payment history

### Tenant

- Register/login as Tenant
- Browse property listings
- Request lease (upload digital signature, select dates)
- Accept agreement and submit lease
- View own leases and payment status
- Submit/view maintenance requests
- Make rent payments (UPI/Card)

---

## Core Angular Concepts Used

- **Components**: Modular UI (dashboard, forms, lists, navbar, etc.)
- **Services**: Business logic and API calls (auth, property, lease, maintenance, payment)
- **Guards**: `authGuard` for route protection and role-based access
- **Interceptors**: `authInterceptor` for JWT token handling and error feedback
- **Reactive Forms**: For registration, login, property, lease, and maintenance forms
- **Routing**: Configured in `app.routes.ts` for all major pages
- **Observables & RxJS**: For async data, state sharing, and API responses
- **File Uploads**: For property images and digital signatures
- **Toastr**: For user notifications

---

## Services & State Management

- **AuthService**: Handles login, registration, token storage, role extraction
- **PropertyService**: CRUD for properties, filtering, image upload
- **LeaseService**: Lease requests, approvals, digital signature handling
- **MaintenanceService**: Maintenance request creation and status updates
- **Payment**: Rent payment status updates
- **SharedDataService**: Shares digital signature, lease, and maintenance data across components (using RxJS `BehaviorSubject`)

---

## Authentication & Authorization

- **JWT-based**: Tokens stored in sessionStorage
- **Role-based Routing**: Guards restrict access to owner/tenant dashboards and features
- **Interceptor**: Attaches token to all outgoing HTTP requests, handles 401/403 errors globally

---

## Routing & Navigation

- Configured in `app.routes.ts`
- Major routes: `/login`, `/registration`, `/owner-dashboard`, `/tenant-dashboard`, `/add-property`, `/lease-request`, `/agreement-page`, `/payment`, etc.
- Route guards for protected and role-specific routes

---

## Forms & Validation

- **Reactive Forms**: Used for all major forms (registration, login, property, lease, maintenance)
- **Validation**: Built-in and custom validators (e.g., password match, phone pattern, file type/size)
- **Error Feedback**: Inline error messages and Toastr notifications

---

## File Uploads

- **Property Images**: Owners upload images when adding/updating properties
- **Digital Signatures**: Tenants upload signature files when requesting leases
- **FormData**: Used for sending files to backend APIs

---

## Notifications & Error Handling

- **Toastr**: Success, error, and info messages for all user actions
- **Global HTTP Error Handling**: Via interceptor for auth errors
- **Inline Form Errors**: For validation and submission issues

---

## Styling & UI

- **Bootstrap 5** and **Bootstrap Icons** for responsive, modern UI
- **Custom CSS** for dashboard, forms, and property cards
- **ngx-toastr** for notifications

---

## Environment & Configuration

- **Environment Files**: `environment.ts` and `environment.development.ts` for API URLs
- **angular.json**: Configures assets, styles, and build options

---

## Testing

- **Unit Testing**: Supported via Angular CLI and Karma (see `ng test`)
- **Spec Files**: Provided for some components (expand as needed)

---

## How to Run

1. Install dependencies:
   ```bash
   npm install
   ```
2. Start the development server:
   ```bash
   ng serve
   ```
3. Open [http://localhost:4200](http://localhost:4200) in your browser.

---

## Dependencies

- Angular 20+
- Bootstrap 5
- Bootstrap Icons
- ngx-toastr
- RxJS
- Zone.js
- (Dev) Karma, Jasmine, Angular CLI
