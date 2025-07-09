// owner-dashboard.component.ts
import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { Router } from '@angular/router';
import { SharedDataService } from '../../../shared/services/shared-data.service';
import { LeaseService } from '../../../shared/services/lease.service';
import {
  MaintenanceService,
  MaintenanceRequestDto,
} from '../../../shared/services/maintenance.service';
import { OwnerNavbar } from '../../navbar/owner-navbar';
import { TOKEN_KEY } from '../../../shared/constants';

// Updated interfaces to match database schema
export interface Property {
  propertyID: number; // Changed from propertyId
  ownerID: number; // Changed from ownerId
  address: string;
  rentAmount: number;
  availabilityStatus: string;
  propertyImage?: string; // Added from schema
  propertyDescription?: string; // Added from schema
}

export interface Users {
  userID: number;
  fullName: string;
  email: string;
  passwordHash: string;
  phoneNumber: string;
  role: string; // 'Owner', 'Tenant', 'Admin'
}

export interface Lease {
  leaseID: number; // Changed from leaseId
  propertyID: number; // Changed from propertyId
  tenantID: number; // Changed from tenantId
  startDate: Date;
  endDate: Date;
  rentAmount: number;
  digitalSignature?: string;
  status: string;
  property?: Property;
  tenant?: Users; // Changed from Tenant to Users
}

export interface Payment {
  paymentID: number; // Changed from paymentId
  leaseID: number; // Changed from leaseId
  amount: number;
  paymentDate: Date;
  status: string;
  lease?: Lease;
}

export interface MaintenanceRequest {
  requestID: number; // Changed from requestId
  propertyID: number; // Changed from propertyId
  tenantID: number; // Changed from tenantId
  issueDescription: string;
  status: string;
  createdDate?: Date; // Not in schema, might need to be added
  priority?: string; // Not in schema, might need to be added
  property?: Property;
  tenant?: Users; // Changed from Tenant to Users
}

// Application interface - not in schema, might need to create table
export interface Application {
  applicationId: number;
  propertyID: number; // Changed to match Property
  tenantID: number; // Changed to match Users
  applicationDate: Date;
  status: string;
  message?: string;
  property?: Property;
  tenant?: Users; // Changed from Tenant to Users
}

export interface DashboardStats {
  totalProperties: number;
  activeTenants: number;
  pendingApplications: number;
  maintenanceRequests: number;
  monthlyRevenue: number;
}

export interface RecentActivity {
  id: number;
  type: string;
  message: string;
  timestamp: Date;
  icon: string;
  iconColor: string;
}

@Component({
  standalone: true,
  imports: [CommonModule, OwnerNavbar],
  selector: 'app-owner-dashboard',
  templateUrl: './ownerdashboard.html',
  styleUrls: ['./ownerdashboard.css'],
})
export class OwnerDashboardComponent implements OnInit {
  // Component properties
  isLoading = true;
  ownerName = '';
  ownerID: number = 0; // Changed from ownerId

  // Dashboard data
  dashboardStats: DashboardStats = {
    totalProperties: 0,
    activeTenants: 0,
    pendingApplications: 0,
    maintenanceRequests: 0,
    monthlyRevenue: 0,
  };

  properties: Property[] = [];
  recentProperties: Property[] = [];
  pendingApplications: Application[] = [];
  maintenanceRequests: MaintenanceRequest[] = [];
  recentActivities: RecentActivity[] = [];
  leases: Lease[] = []; // Added leases property
  pendingLeasesCount: number = 0; // Added property to store count of pending leases
  maintenanceRequestsCount: number = 0; // Added property to store count of maintenance requests
  pendingRequestsCount: number = 0; // Added property to store count of pending requests

  // API base URL - adjust according to your backend
  // private readonly apiBaseUrl = 'https://localhost:7074/';
  private baseUrl = environment.baseUrl;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object,
    private router: Router,
    @Inject(SharedDataService) private sharedDataService: SharedDataService,
    private leaseService: LeaseService,
    private service: MaintenanceService // Inject MaintenanceService
  ) {
    // Get owner details from localStorage or auth service
    this.initializeOwnerData();
  }

  ngOnInit(): void {
    this.fetchRequests();

    // Subscribe to maintenance requests and calculate pending requests count
    this.sharedDataService
      .getMaintenanceRequests()
      .subscribe((data: MaintenanceRequest[]) => {
        console.log('Received maintenance requests:', data);
        this.maintenanceRequests = data;
        this.pendingRequestsCount = this.maintenanceRequests.filter(
          (req) => req.status === 'Pending'
        ).length; // Calculate count of pending requests
      });

    if (isPlatformBrowser(this.platformId)) {
      this.loadDashboardData();
    }

    // Fetch lease data from LeaseService and update SharedDataService
    this.leaseService.getOwnerLeases().subscribe((data) => {
      this.sharedDataService.setLeaseData(data); // Update shared data service
    });

    // Subscribe to lease data and calculate pending leases count
    this.sharedDataService.getLeaseData().subscribe((data: Lease[]) => {
      console.log('Received lease data in owner dashboard:', data);
      this.leases = data;
      this.pendingLeasesCount = this.leases.filter(
        (lease) => lease.status === 'Pending'
      ).length; // Calculate count of pending leases
    });

    // Fetch maintenance request data and update SharedDataService
    this.getMaintenanceRequests().subscribe((data) => {
      this.sharedDataService.setMaintenanceRequests(data); // Update shared data service
    });

    // Subscribe to maintenance request data and calculate pending maintenance requests count
    this.sharedDataService
      .getMaintenanceRequests()
      .subscribe((data: MaintenanceRequest[]) => {
        console.log(
          'Received maintenance request data in owner dashboard:',
          data
        );
        this.maintenanceRequests = data;

        // Include both Pending and Active statuses in the count
        this.maintenanceRequestsCount = this.maintenanceRequests.filter(
          (request) =>
            request.status === 'Pending' || request.status === 'Active'
        ).length;

        console.log(
          'Maintenance Requests Count (Pending + Active):',
          this.maintenanceRequestsCount
        );
      });
  }

  private initializeOwnerData(): void {
    if (isPlatformBrowser(this.platformId)) {
      const token = sessionStorage.getItem(TOKEN_KEY); // <--- Use TOKEN_KEY for getting the token

      if (token) {
        try {
          // Split the token and base64 decode the payload part
          // The payload is the second part (index 1) after splitting by '.'
          const jwtPayload = JSON.parse(atob(token.split('.')[1]));

          // Extract the name claim using its full URI
          this.ownerName = jwtPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'Owner';

          // Extract the name identifier for ownerID using its full URI
          this.ownerID = parseInt(jwtPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']) || 0;

        } catch (error) {
          console.error('Error parsing token from sessionStorage:', error);
          this.ownerName = 'John Doe'; // Fallback name
          this.ownerID = 1; // Fallback ID
        }
      } else {
        // No token found in sessionStorage
        this.ownerName = 'John Doe';
        this.ownerID = 1;
      }
    }
  }


  private getHttpHeaders(): HttpHeaders {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      return new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: token ? `Bearer ${token}` : '',
      });
    } else {
      return new HttpHeaders();
    }
  }

  loadDashboardData(): void {
    this.isLoading = true;

    forkJoin({
      // stats: this.getDashboardStats(),
      properties: this.getOwnerProperties(),
      applications: this.getPendingApplications(),
      maintenance: this.getMaintenanceRequests(),
      activities: this.getRecentActivities(),
    }).subscribe({
      next: (data) => {
        // this.dashboardStats = data.stats;
        this.properties = data.properties;
        this.recentProperties = data.properties.slice(0, 2);
        this.pendingApplications = data.applications;
        this.maintenanceRequests = data.maintenance;
        this.recentActivities = data.activities;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.isLoading = false;
      },
    });
  }

  // API Methods - Updated endpoints to match schema
  // getDashboardStats(): Observable<DashboardStats> {
  //   return this.http.get<DashboardStats>(
  //     `${this.apiBaseUrl}/owners/${this.ownerID}/dashboard-stats`,
  //     { headers: this.getHttpHeaders() }
  //   ).pipe(
  //     catchError(error => {
  //       console.error('Error fetching dashboard stats:', error);
  //       return [];
  //     })
  //   );
  // }

  getOwnerProperties(): Observable<Property[]> {
    return this.http
      .get<Property[]>(`${this.baseUrl}/Owner/Properties`, {
        headers: this.getHttpHeaders(),
      })
      .pipe(
        catchError((error) => {

          console.error('Error fetching properties:', error);
          return [];
        })
      );
  }

  getPendingApplications(): Observable<Application[]> {
    return this.http
      .get<Application[]>(
        `${this.baseUrl}/applications/owner/${this.ownerID}/pending`,
        { headers: this.getHttpHeaders() }
      )
      .pipe(
        catchError((error) => {
          console.error('Error fetching pending applications:', error);
          return [];
        })
      );
  }

  getMaintenanceRequests(): Observable<MaintenanceRequest[]> {
    return this.http
      .get<MaintenanceRequest[]>(`${this.baseUrl}/Owner/GetAllMaintenance`, {
        headers: this.getHttpHeaders(),
      })
      .pipe(
        catchError((error) => {
          console.error('Error fetching maintenance requests:', error);
          return [];
        })
      );
  }

  getRecentActivities(): Observable<RecentActivity[]> {
    return this.http
      .get<RecentActivity[]>(
        `${this.baseUrl}/owners/${this.ownerID}/recent-activities`,
        { headers: this.getHttpHeaders() }
      )
      .pipe(
        catchError((error) => {
          console.error('Error fetching recent activities:', error);
          return [];
        })
      );
  }

  // Action Methods
  onAddProperty(): void {
    console.log('Add new property');
    // Navigate to add property page
    this.router.navigate(['/add-property']);
  }

  onEditProperty(propertyID: number): void {
    // Updated parameter name
    console.log('Edit property:', propertyID);
  }

  onViewAllProperties(): void {
    console.log('View all properties');
    this.router.navigate(['/Property-list']);
  }

  onReviewApplications(): void {
    console.log('Review pending applications');
    this.router.navigate(['/lease-owner']);
  }

  onViewMaintenanceRequests(): void {
    console.log('View maintenance requests');
    this.router.navigate(['/owner-maintenance']);
  }

  onViewPaymentHistory(): void {
    console.log('View payment history');
  }

  onViewReports(): void {
    console.log('View reports');
  }

  onApproveApplication(applicationId: number): void {
    this.updateApplicationStatus(applicationId, 'Approved');
  }

  onRejectApplication(applicationId: number): void {
    this.updateApplicationStatus(applicationId, 'Rejected');
  }

  private updateApplicationStatus(applicationId: number, status: string): void {
    const updateData = { status: status };

    this.http
      .put(`${this.baseUrl}/applications/${applicationId}/status`, updateData, {
        headers: this.getHttpHeaders(),
      })
      .subscribe({
        next: (response) => {
          console.log('Application status updated:', response);
          this.loadDashboardData();
        },
        error: (error) => {
          console.error('Error updating application status:', error);
        },
      });
  }

  onUpdateMaintenanceStatus(requestID: number, status: string): void {
    // Updated parameter name
    const updateData = { status: status };

    this.http
      .put(`${this.baseUrl}/maintenance/${requestID}/status`, updateData, {
        headers: this.getHttpHeaders(),
      })
      .subscribe({
        next: (response) => {
          console.log('Maintenance status updated:', response);
          this.loadDashboardData();
        },
        error: (error) => {
          console.error('Error updating maintenance status:', error);
        },
      });
  }

  // Utility Methods
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0,
    }).format(amount);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getTimeAgo(date: Date | string): string {
    const now = new Date();
    const diffInMinutes = Math.floor(
      (now.getTime() - new Date(date).getTime()) / (1000 * 60)
    );

    if (diffInMinutes < 60) {
      return `${diffInMinutes} minutes ago`;
    } else if (diffInMinutes < 1440) {
      const hours = Math.floor(diffInMinutes / 60);
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else {
      const days = Math.floor(diffInMinutes / 1440);
      return `${days} day${days > 1 ? 's' : ''} ago`;
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'available':
      case 'approved':
      case 'completed':
        return 'badge bg-success';
      case 'occupied':
      case 'pending':
        return 'badge bg-warning';
      case 'rejected':
      case 'maintenance':
        return 'badge bg-danger';
      default:
        return 'badge bg-secondary';
    }
  }

  getPriorityBadgeClass(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'high':
        return 'badge bg-danger';
      case 'medium':
        return 'badge bg-warning';
      case 'low':
        return 'badge bg-success';
      default:
        return 'badge bg-secondary';
    }
  }

  onRefreshDashboard(): void {
    this.loadDashboardData();
  }

  logout(): void {
    // Remove the token and user data from localStorage or sessionStorage
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');

    // Navigate to the login page
    this.router.navigate(['']);
  }

  fetchRequests(): void {
    this.service.getAllRequests().subscribe((res: MaintenanceRequestDto[]) => {
      this.sharedDataService.setMaintenanceRequests(res);
    });
  }
}
