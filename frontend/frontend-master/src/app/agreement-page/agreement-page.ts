import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaseService } from '../shared/services/lease.service';
import { SharedDataService } from '../shared/services/shared-data.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-agreement-page',
  imports: [FormsModule],
  templateUrl: './agreement-page.html',
  styleUrl: './agreement-page.css',
})
export class AgreementPage implements OnInit {
  propertyId!: number;
  startDate!: string;
  endDate!: string;
  proposedRentAmount!: number;
  isAgreementAccepted: boolean = false; // Added property to track agreement acceptance
  digitalSignature!: File; // Added property to store the digital signature file

  constructor(
    private route: ActivatedRoute,
    private leaseService: LeaseService,
    private router: Router,
    private sharedDataService: SharedDataService, // Injected SharedDataService
    private toastr: ToastrService // Injected ToastrService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: any) => {
      this.propertyId = +params['propertyId'];
      this.startDate = params['startDate'];
      this.endDate = params['endDate'];
      this.proposedRentAmount = +params['proposedRentAmount'];
      const signature = this.sharedDataService.getDigitalSignature(); // Retrieve file from SharedDataService
      if (!signature) {
        this.toastr.error(
          'Digital signature is required but was not provided.',
          'Error'
        );
        return;
      }
      this.digitalSignature = signature;
    });
  }

  acceptAgreement(): void {
    const data = {
      propertyId: this.propertyId,
      startDate: this.startDate,
      endDate: this.endDate,
      proposedRentAmount: this.proposedRentAmount,
      digitalSignature: this.digitalSignature, // Use the actual signature file
    };

    this.leaseService.submitLease(data).subscribe({
      next: (res) => {
        this.router.navigate(['/getleasetenant']).then(() => {
          // Show toaster notification after navigation
          this.toastr.success(
            'Lease generated successfully. Kindly wait for the owner to accept it and then proceed to payment.',
            'Success'
          );
        });
      },
      error: (err) =>
        this.toastr.error(
          'Error submitting lease: ' + (err?.message || err),
          'Error'
        ),
    });
  }
}
