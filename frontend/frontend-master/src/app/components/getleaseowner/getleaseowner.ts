import { Component, OnInit } from '@angular/core';
import {
  LeaseService,
  GetLeaseDetailsDTO,
} from '../../shared/services/lease.service';
import { SharedDataService } from '../../shared/services/shared-data.service';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { OwnerNavbar } from "../navbar/owner-navbar";

@Component({
  selector: 'app-getleaseowner',
  imports: [CommonModule, OwnerNavbar],
  templateUrl: './getleaseowner.html',
  styleUrl: './getleaseowner.css',
})
export class Getleaseowner implements OnInit {
  leases: GetLeaseDetailsDTO[] = [];

  constructor(
    private leaseService: LeaseService,
    private sharedDataService: SharedDataService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.fetchLeases();
  }

  fetchLeases(): void {
    this.leaseService.getOwnerLeases().subscribe({
      next: (response) => {
        console.log('Fetched leases:', response); // Log the fetched data
        this.leases = response.map((lease) => ({
          ...lease,
          digitalSignature: lease.digitalSignature
            ? `data:image/png;base64,${lease.digitalSignature}`
            : null,
        }));
        this.sharedDataService.setLeaseData(this.leases); // Pass data to shared service
      },
      error: (error) => {
        console.error('Error fetching leases:', error);
        this.toastr.error(
          'Error fetching leases: ' + (error?.message || error),
          'Error'
        );
      },
    });
  }

  approveLease(leaseId: number): void {
    this.leaseService.approveRejectLease(leaseId, 'Approved').subscribe({
      next: () => {
        this.toastr.success('Lease approved successfully!', 'Success');
        this.fetchLeases(); // Refresh the list
      },
      error: (error) => {
        console.error('Error approving lease:', error);
        this.toastr.error(
          'Error approving lease: ' + (error?.message || error),
          'Error'
        );
      },
    });
  }

  rejectLease(leaseId: number): void {
    this.leaseService.approveRejectLease(leaseId, 'Rejected').subscribe({
      next: () => {
        this.toastr.success('Lease rejected successfully!', 'Success');
        this.fetchLeases(); // Refresh the list
      },
      error: (error) => {
        console.error('Error rejecting lease:', error);
        this.toastr.error(
          'Error rejecting lease: ' + (error?.message || error),
          'Error'
        );
      },
    });
  }

  viewDigitalSignature(digitalSignature: string): void {
    window.open(digitalSignature, '_blank'); // Open the digital signature in a new tab
  }

  updateLeaseStatus(leaseId: number, status: string): void {
    this.leaseService.approveRejectLease(leaseId, status).subscribe({
      next: (response) => {
        try {
          const parsedResponse =
            typeof response === 'string' ? { message: response } : response;
          this.toastr.success(
            parsedResponse.message ||
              `Lease status updated to ${status} successfully!`,
            'Success'
          );
        } catch (error) {
          this.toastr.success(
            `Lease status updated to ${status} successfully!`,
            'Success'
          );
        }
        this.fetchLeases(); // Refresh the list
      },
      error: (error) => {
        console.error(`Error updating lease status to ${status}:`, error);
        this.toastr.error(
          `Failed to update lease status: ${error.message || 'Unknown error'}`,
          'Error'
        );
      },
    });
  }

  getPendingLeasesCount(): number {
    return this.leases.filter((lease) => lease.status === 'Pending').length;
  }

  logPendingLeases(): void {
    const pendingLeases = this.leases.filter(
      (lease) => lease.status === 'Pending'
    );
    console.log('Pending Leases:', pendingLeases);
  }
}
