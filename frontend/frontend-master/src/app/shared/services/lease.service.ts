// lease.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';
 
// DTOs for type safety
export interface CreateLeaseDTO {
  PropertyId: number;
  StartDate: string; // ISO string (yyyy-MM-dd)
  EndDate: string;   // ISO string (yyyy-MM-dd)
  ProposedRentAmount: number;
  DigitalSignature: File;
}
 
export interface GetLeaseDetailsDTO {
  leaseId: number;
  propertyId: number;
  tenantId: number;
  tenantName: string;
  startDate: string;
  endDate: string;
  rentAmount: number;
  status: string;
  digitalSignature: string | null;
}
 
@Injectable({
  providedIn: 'root'
})
export class LeaseService {
  private requestLeaseUrl = `${environment.apiBaseUrl}/Lease/Tenant/RequestLease`;
  private tenantLeasesUrl = `${environment.apiBaseUrl}/Lease/Tenant/Leases`;
  private ownerLeasesUrl = `${environment.apiBaseUrl}/Owner/Leases`;
 
  constructor(private http: HttpClient) {}
 
  // Submit a lease request (with file upload)
  submitLease(data: {
    propertyId: number;
    startDate: string;
    endDate: string;
    proposedRentAmount: number;
    digitalSignature: File;
  }): Observable<any> {
    const formData = new FormData();
    const propertyId = Number(data.propertyId);
    const proposedRentAmount = Number(data.proposedRentAmount);

    if (isNaN(propertyId) || isNaN(proposedRentAmount)) {
        throw new Error('Invalid PropertyId or ProposedRentAmount. Ensure they are valid numbers.');
    }

    formData.append('PropertyId', propertyId.toString());
    formData.append('StartDate', data.startDate);
    formData.append('EndDate', data.endDate);
    formData.append('ProposedRentAmount', proposedRentAmount.toString());
    formData.append('DigitalSignature', data.digitalSignature);
 
    // Log for debugging
    for (const pair of formData.entries()) {
      console.log(pair[0], pair[1]);
    }

    console.log('Appending to FormData:', {
      PropertyId: data.propertyId,
      ProposedRentAmount: data.proposedRentAmount
    });
 
    return this.http.post(this.requestLeaseUrl, formData);
  }
 
  // Get leases for the logged-in tenant
  getTenantLeases(): Observable<GetLeaseDetailsDTO[]> {
    console.log('Fetching tenant leases from:', this.tenantLeasesUrl); // Log the URL for debugging
    return this.http.get<GetLeaseDetailsDTO[]>(this.tenantLeasesUrl).pipe(
      map(leases => {
        console.log('Fetched tenant leases:', leases); // Log the fetched leases for debugging
        return leases;
      })
    );
  }
 
  // Add method to approve or reject lease
  approveRejectLease(leaseId: number, status: string): Observable<any> {
    const payload = { LeaseId: leaseId, Status: status };
    return this.http.post(`${environment.baseUrl}/Owner/ApproveRejectLease`, payload);
  }

  // Add method to get leases for properties owned by the owner
  getOwnerLeases(): Observable<GetLeaseDetailsDTO[]> {
    return this.http.get<GetLeaseDetailsDTO[]>(`${environment.apiBaseUrl}/Lease/Owner/Leases`);
  }

  // Add a method to fetch tenant leases and handle payment status
/*   getTenantLeasesWithPaymentStatus(): Observable<(GetLeaseDetailsDTO & { paymentStatus: string })[]> {
    return this.http.get<GetLeaseDetailsDTO[]>(this.tenantLeasesUrl).pipe(
      map(leases => leases.map(lease => ({
        ...lease,
        paymentStatus: lease.status === 'Active' ? 'Active' : 'Pending'
      })))
    );
  }
} */
getPaymentStatus(leaseId: number): Observable<string> {
  const paymentStatusUrl = `${environment.baseUrl}/Lease/${leaseId}/Payments`;
  console.log('Payment Status URL:', paymentStatusUrl); // Log the URL for debugging
  return this.http.get<Array<{ status: string }>>(paymentStatusUrl).pipe(
    map(response => {
      console.log('API Response:', response); // Log the API response for debugging
      return response.length > 0 ? response[0].status : 'Unknown'; // Extract the status from the first object or return 'Unknown'
    })
  );
}
}