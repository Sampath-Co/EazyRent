import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // Required for *ngIf, *ngFor
import { FormsModule } from '@angular/forms'; // Required for [(ngModel)]
import { HttpClient } from '@angular/common/http'; // Required for making HTTP requests
import { Router, RouterModule } from '@angular/router'; // Required for navigation
import { catchError, map } from 'rxjs/operators';
import { of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { TenantNavbar } from '../../navbar/tenant-navbar';

// Define interfaces for data structures based on your ERD and API responses
interface Property {
  propertyID: string;
  ownerID: string;
  address: string;
  rentAmount: number;
  availabilityStatus: string;
  propertyImage?: string; // Optional, as it might not always be present or could be a URL
  propertyDescription: string;
}

interface Lease {
  leaseID: string;
  propertyID: string;
  tenantID: string;
  startDate: string; // Assuming ISO string from backend
  endDate: string; // Assuming ISO string from backend
  rentAmount: number;
  digitalSignature?: string; // Optional
  status: string;
  property?: Property; // Added for convenience to display property details in lease item
}

interface MaintenanceRequest {
  requestID: string;
  propertyID: string;
  tenantID: string;
  issueDescription: string;
  status: string;
  createdDate: string; // Assuming ISO string from backend
  property?: Property; // Added for convenience to display property details in request card
}

@Component({
  selector: 'app-tenant',
  standalone: true,
  imports: [CommonModule, FormsModule, TenantNavbar, RouterModule],
  templateUrl: './tenantdashboard.html',
  styleUrls: ['./tenantdashboard.css'],
})
export class Tenantdashboard implements OnInit {
  // Base URL for your API
  private baseUrl = environment.baseUrl; // Adjust this if your backend URL is different

  // --- Header Data ---
  tenantName: string = 'Tenant User'; // This should ideally come from authenticated user data

  // --- Available Properties Section Data ---
  // This will now hold a limited number of properties for initial display
  availableProperties: Property[] = [];

  // --- My Leases Section Data ---
  myLeases: Lease[] = []; // Array to hold tenant's active leases

  // --- Maintenance Requests Section Data ---
  maintenanceRequests: MaintenanceRequest[] = []; // Array to hold tenant's maintenance requests

  // Loading states for better UX
  loadingProperties: boolean = false; // Re-added loading state for properties
  loadingLeases: boolean = false;
  loadingMaintenance: boolean = false;

  constructor(private http: HttpClient, private router: Router) {} // Inject HttpClient and Router

  ngOnInit(): void {
    // Load data from backend when the component initializes
    this.loadTenantData();
  }

  /**
   * Loads initial data for the tenant portal by making API calls to the backend.
   */
  loadTenantData(): void {
    this.getInitialProperties(); // Fetch a few properties for the dashboard
    this.getMyLeases();
    this.getMaintenanceRequests();
    // You might also fetch tenantName from a user profile API here
  }

  /**
   * Fetches a limited number of available properties for initial display on the dashboard.
   * API: GET /Tenant/Properties
   */
  getInitialProperties(): void {
    this.loadingProperties = true;
    this.http
      .get<Property[]>(`${this.baseUrl}/Tenant/Properties`)
      .pipe(
        catchError((error) => {
          console.error('Error fetching initial properties:', error);
          this.loadingProperties = false;
          // Return an empty array or handle error gracefully
          return of([]);
        }),
        // Take only the first few properties for dashboard display
        map((properties) => properties.slice(0, 3)) // Display up to 3 properties
      )
      .subscribe((properties) => {
        this.availableProperties = properties;
        this.loadingProperties = false;
      });
  }

  /**
   * Fetches the current tenant's leases from the backend.
   * API: GET /api/Lease/Tenant/Leases
   */
  getMyLeases(): void {
    this.loadingLeases = true;
    // Assuming tenantID is available from authentication or a global state
    // For now, we'll assume the API implicitly handles the current authenticated tenant
    this.http
      .get<Lease[]>(`${this.baseUrl}/api/Lease/Tenant/Leases`)
      .pipe(
        catchError((error) => {
          console.error('Error fetching my leases:', error);
          this.loadingLeases = false;
          return of([]);
        })
      )
      .subscribe((leases) => {
        this.myLeases = leases.map((lease) => ({
          ...lease,
          // Convert date strings to Date objects if needed for formatting
          startDate: new Date(lease.startDate).toISOString(),
          endDate: new Date(lease.endDate).toISOString(),
          // Placeholder for property details if the API doesn't return full property
          property: { address: `Property ID: ${lease.propertyID}` } as Property,
        }));
        this.loadingLeases = false;
      });
  }

  /**
   * Fetches the current tenant's maintenance requests from the backend.
   * API: GET /Tenant/GetAllMaintenance
   */
  getMaintenanceRequests(): void {
    this.loadingMaintenance = true;
    // Assuming tenantID is available from authentication or a global state
    this.http
      .get<MaintenanceRequest[]>(`${this.baseUrl}/Tenant/GetAllMaintenance`)
      .pipe(
        catchError((error) => {
          console.error('Error fetching maintenance requests:', error);
          this.loadingMaintenance = false;
          return of([]);
        })
      )
      .subscribe((requests) => {
        this.maintenanceRequests = requests.map((request) => ({
          ...request,
          createdDate: new Date(request.createdDate).toISOString(),
          // Placeholder for property details if the API doesn't return full property
          property: {
            address: `Property ID: ${request.propertyID}`,
          } as Property,
        }));
        this.loadingMaintenance = false;
      });
  }
  
  onViewAllProperties(): void {
    console.log('Navigating to property list page...');
    this.router.navigate(['/Property-list']); 
  }

  onApplyForProperty(propertyID: string): void {
    console.log(`Attempting to apply for property: ${propertyID}`);
    // API: POST /api/Lease/Tenant/RequestLease
    // You'll need to send the tenantID and propertyID.
    // Assuming tenantID is available (e.g., from a service or authentication)
    const tenantID = 'TENANT_USER_ID_PLACEHOLDER'; // Replace with actual tenant ID

    const requestBody = {
      propertyID: propertyID,
      tenantID: tenantID,
      // Add other necessary fields for lease request, e.g., startDate, endDate, rentAmount (if tenant proposes)
      // For simplicity, let's assume the backend handles defaults or gets them from property details.
      // You might need a more complex form for this.
    };

    this.http
      .post(`${this.baseUrl}/api/Lease/Tenant/RequestLease`, requestBody)
      .pipe(
        catchError((error) => {
          console.error('Error applying for property:', error);
          // Show user a message about the error
          return of(null); // Return observable of null to continue stream
        })
      )
      .subscribe((response) => {
        if (response) {
          console.log('Application submitted successfully:', response);
          // Optionally, refresh leases or show a success message
          this.getMyLeases(); // Refresh the lease list
        } else {
          console.log('Application failed or was cancelled.');
        }
      });
  }

  onCreateMaintenanceRequest(): void {
    this.router.navigate(['/maintenance-tenant']);
  }

  onViewPaymentHistory(): void {
    this.router.navigate(['/payments']);
  }

  onViewMyApplications(): void {
    this.router.navigate(['/getleasetenant']);
  }

  /**
   * Handles user logout.
   * This would typically clear authentication tokens and redirect to the login page.
   */
  onLogout(): void {
    console.log('User logged out.');
    // Implement actual logout logic here (e.g., clear localStorage, call auth service)
    this.router.navigate(['/login']); // Redirect to your login page
  }

  // --- Utility Functions for Template ---

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
    }).format(amount);
  }

  // Ensure the date passed to this function is a Date object or a string parseable by Date constructor
  formatDate(dateInput: string | Date): string {
    const date = new Date(dateInput);
    return date.toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getLeaseStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'lease-status active';
      case 'pending':
        return 'lease-status pending';
      case 'expired':
        return 'lease-status expired';
      default:
        return 'lease-status';
    }
  }

  getMaintenanceStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'open':
        return 'status-open';
      case 'in progress':
        return 'status-in-progress';
      case 'completed':
        return 'status-completed';
      default:
        return '';
    }
  }
}
