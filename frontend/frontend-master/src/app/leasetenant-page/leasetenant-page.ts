import { Component, OnInit } from '@angular/core';
import { LeaseService } from '../shared/services/lease.service';
import { GetLeaseDetailsDTO } from '../shared/services/lease.service';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { TenantNavbar } from "../components/navbar/tenant-navbar";

@Component({
  selector: 'app-leasetenant-page',
  imports: [CommonModule, TenantNavbar],
  templateUrl: './leasetenant-page.html',
  styleUrls: ['./leasetenant-page.css']
})
export class LeasetenantPage implements OnInit {
  leases: (GetLeaseDetailsDTO & { paymentStatus: string })[] = [];

  constructor(
    private leaseService: LeaseService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.fetchLeases();
  }

  fetchLeases(): void {
    this.leaseService.getTenantLeases().subscribe(
      (leases) => {
        this.toastr.success('Leases fetched successfully.', 'Success');
        this.leases = leases.map(lease => ({ ...lease, paymentStatus: '' }));
        this.leases.forEach(lease => {
          if (!lease.leaseId) {
            this.toastr.error('Invalid leaseId encountered.', 'Error');
            return;
          }
          this.leaseService.getPaymentStatus(lease.leaseId).subscribe(
            (status) => {
              lease.paymentStatus = status;
            },
            (error) => {
              this.toastr.error(`Error fetching payment status for lease ${lease.leaseId}: ${error?.message || error}`, 'Error');
            }
          );
        });
      },
      (error) => {
        this.toastr.error('Error fetching leases: ' + (error?.message || error), 'Error');
      }
    );
  }

  navigateToPayment(leaseId: number, rentAmount: number): void {
    const paymentUrl = `/payment?leaseId=${leaseId}&rentAmount=${rentAmount}`;
    this.toastr.info('Redirecting to payment page...', 'Info');
    window.location.href = paymentUrl;
  }

  navigateToMaintenance(propertyId: number, tenantId: number): void {
    // Navigate to the maintenance dashboard with the propertyId and tenantId as query parameters
    const maintenanceUrl = `/tenant-maintenance?propertyId=${propertyId}&tenantId=${tenantId}`;
    this.toastr.info('Redirecting to maintenance request page...', 'Info');
    window.location.href = maintenanceUrl; // Securely pass the propertyId and tenantId
  }

}
