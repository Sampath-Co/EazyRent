import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export interface CreateMaintenanceRequestDto {
  requestId: number;
  propertyId: number | null;
  tenantId: number | null;
  issueDescription: string | null;
  status: string | null;
}

export interface MaintenanceRequestDto {
  requestId: number;
  propertyId: number;
  tenantFullName: string;
  issueDescription: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class MaintenanceService {
  private baseUrl = environment.baseUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  // Role-based: Get all maintenance requests
  getAllRequests(): Observable<MaintenanceRequestDto[]> {
    const role = this.authService.getUserRoles();
    let endpoint = '';
    if (role === 'Owner') {
      endpoint = `${this.baseUrl}/Owner/GetAllMaintenance`;
    } else if (role === 'Tenant') {
      endpoint = `${this.baseUrl}/Tenant/GetAllMaintenance`;
    } else {
      throw new Error('Unknown user role');
    }
    return this.http.get<MaintenanceRequestDto[]>(endpoint);
  }

  // Create a new maintenance request (for tenant)
  createRequest(request: CreateMaintenanceRequestDto): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/Tenant/CreateMaintenanceRequest`,
      request
    );
  }

  // Update the status of a maintenance request (for owner)
  updateRequestStatus(requestId: number, status: string): Observable<any> {
    return this.http.put(
      `${this.baseUrl}/MaintenanceRequest/Owner/Update/${requestId}`,
      JSON.stringify(status),
      {
        headers: { 'Content-Type': 'application/json' }
      }
    );
  }
}
