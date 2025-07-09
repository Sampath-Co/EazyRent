import { Component, OnInit } from '@angular/core';
import { PaymentService, Payment } from '../../shared/services/payment';
import { CommonModule } from '@angular/common';
import {
  LeaseService,
  GetLeaseDetailsDTO,
} from '../../shared/services/lease.service';
import { AuthService } from '../../shared/services/auth.service'; 
import { TenantNavbar } from '../navbar/tenant-navbar';
import { OwnerNavbar } from '../navbar/owner-navbar';

@Component({
  selector: 'app-payment-dashboard',
  imports: [CommonModule,TenantNavbar,OwnerNavbar],
  templateUrl: './payment-dashboard.html',
  styleUrl: './payment-dashboard.css',
})
export class PaymentDashboard implements OnInit {
  payments: Payment[] = [];
  isLoading = false;
  error: string | null = null;
  private leaseMap: { [leaseId: number]: string } = {};
  userRole: string = '';

  constructor(
    private paymentService: PaymentService,
    private leaseService: LeaseService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRoles() ?? ''; // Expose userRole
    if (this.userRole === 'Owner' || this.userRole === 'Tenant') {
      this.fetchPaymentsAndLeasesByRole(this.userRole);
    } else {
      this.error = 'Invalid user role.';
    }
  }

  fetchPaymentsAndLeases(): void {
    this.isLoading = true;
    this.leaseService.getOwnerLeases().subscribe({
      next: (leases: GetLeaseDetailsDTO[]) => {
        console.log('Fetched leases:', leases);
        // Build a map of leaseId to tenantName
        this.leaseMap = {};
        leases.forEach((lease) => {
          this.leaseMap[lease.leaseId] = lease.tenantName;
        });
        console.log('Lease map:', this.leaseMap);

        // Now fetch all payments
        this.paymentService.getAllPayments().subscribe({
          next: (payments) => {
            console.log('Fetched payments:', payments);
            // Map tenantName from leaseMap using leaseId
            this.payments = payments.map((payment) => {
              const tenantName = payment.leaseId // Fix: Use the correct property name
                ? this.leaseMap[payment.leaseId] || '-'
                : '-';
              if (tenantName === '-') {
                console.warn(
                  `No tenant name found for leaseId: ${payment.leaseId}`
                );
              }
              return {
                ...payment,
                tenantName,
              };
            });
            console.log('Mapped payments:', this.payments);
            this.isLoading = false;
          },
          error: (err) => {
            console.error('Error fetching payments:', err);
            this.error = 'Failed to load payments.';
            this.isLoading = false;
          },
        });
      },
      error: (err) => {
        console.error('Error fetching leases:', err);
        this.error = 'Failed to load leases.';
        this.isLoading = false;
      },
    });
  }

  fetchTenantPaymentsAndLeases(): void {
    this.isLoading = true;
    this.leaseService.getTenantLeases().subscribe({
      next: (leases: GetLeaseDetailsDTO[]) => {
        console.log('Fetched tenant leases:', leases);
        // Build a map of leaseId to propertyId or other info if needed
        this.leaseMap = {};
        leases.forEach((lease) => {
          this.leaseMap[lease.leaseId] = lease.propertyId.toString(); // or lease.leaseId = lease.propertyId, adjust as needed
        });
        console.log('Lease map:', this.leaseMap);

        // Now fetch all payments for the tenant
        this.paymentService.getAllPayments().subscribe({
          next: (payments) => {
            console.log('Fetched payments:', payments);
            // Map propertyId or other info from leaseMap using leaseId
            this.payments = payments.map((payment) => {
              const propertyId = payment.leaseId
                ? this.leaseMap[payment.leaseId] || '-'
                : '-';
              return {
                ...payment,
                propertyId,
              };
            });
            console.log('Mapped tenant payments:', this.payments);
            this.isLoading = false;
          },
          error: (err) => {
            console.error('Error fetching payments:', err);
            this.error = 'Failed to load payments.';
            this.isLoading = false;
          },
        });
      },
      error: (err) => {
        console.error('Error fetching leases:', err);
        this.error = 'Failed to load leases.';
        this.isLoading = false;
      },
    });
  }

  fetchPaymentsAndLeasesByRole(userRole: 'Owner' | 'Tenant'): void {
    this.isLoading = true;
    const leaseObs =
      userRole === 'Owner'
        ? this.leaseService.getOwnerLeases()
        : this.leaseService.getTenantLeases();

    leaseObs.subscribe({
      next: (leases: GetLeaseDetailsDTO[]) => {
        console.log('Fetched leases:', leases);
        this.leaseMap = {};
        if (userRole === 'Owner') {
          leases.forEach((lease) => {
            this.leaseMap[lease.leaseId] = lease.tenantName;
          });
        } else {
          leases.forEach((lease) => {
            this.leaseMap[lease.leaseId] = lease.tenantName;
          });
        }
        console.log('Lease map:', this.leaseMap);

        this.paymentService.getAllPayments().subscribe({
          next: (payments) => {
            console.log('Fetched payments:', payments);
            this.payments = payments.map((payment) => {
              if (userRole === 'Owner') {
                const tenantName = payment.leaseId
                  ? this.leaseMap[payment.leaseId] || '-'
                  : '-';
                if (tenantName === '-') {
                  console.warn(
                    `No tenant name found for leaseId: ${payment.leaseId}`
                  );
                }
                return { ...payment, tenantName };
              } else {
                const tenantName = payment.leaseId
                  ? this.leaseMap[payment.leaseId] || '-'
                  : '-';
                return { ...payment, tenantName };
              }
            });
            console.log('Mapped payments:', this.payments);
            this.isLoading = false;
          },
          error: (err) => {
            console.error('Error fetching payments:', err);
            this.error = 'Failed to load payments.';
            this.isLoading = false;
          },
        });
      },
      error: (err) => {
        console.error('Error fetching leases:', err);
        this.error = 'Failed to load leases.';
        this.isLoading = false;
      },
    });
  }
}
