import { Component, OnInit } from '@angular/core';
import {
  ReactiveFormsModule,
  FormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { LeaseService } from '../shared/services/lease.service';
import { SharedDataService } from '../shared/services/shared-data.service';

@Component({
  selector: 'app-leaseformtenant',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule], // Only modules here!
  templateUrl: './leaseformtenant.html',
  styleUrl: './leaseformtenant.css',
})
export class Leaseformtenant implements OnInit {
  leaseForm: FormGroup;
  selectedFile: File | null = null;
  today: string = new Date().toISOString().split('T')[0];

  constructor(
    private fb: FormBuilder,
    private leaseService: LeaseService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private sharedDataService: SharedDataService // Injected SharedDataService
  ) {
    this.leaseForm = this.fb.group({
      propertyId: [{ value: '', disabled: true }, Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      proposedRentAmount: [{ value: '', disabled: true }, Validators.required],
      digitalSignature: [null], // for file input
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const propertyId = Number(params['propertyId']);
      const proposedRentAmount = Number(params['rentAmount']);

      if (isNaN(propertyId) || isNaN(proposedRentAmount)) {
        this.toastr.error('Invalid propertyId or rentAmount received. Please check the input.', 'Error');
        return;
      }

      this.toastr.info('Query parameters received successfully.', 'Info');
      this.leaseForm.patchValue({
        propertyId: propertyId,
        proposedRentAmount: proposedRentAmount,
      });
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    this.leaseForm.patchValue({ digitalSignature: this.selectedFile });
    if (this.selectedFile) {
      this.sharedDataService.setDigitalSignature(this.selectedFile); // Store file in SharedDataService
    } else {
      this.toastr.error('No file selected. Please select a file before proceeding.', 'Error');
    }
    this.toastr.success('File selected successfully.', 'Success');
  }

  submitLease(): void {
    const rawFormValues = this.leaseForm.getRawValue();
    const propertyId = Number(rawFormValues.propertyId);
    const proposedRentAmount = Number(rawFormValues.proposedRentAmount);

    if (isNaN(propertyId) || isNaN(proposedRentAmount)) {
      this.toastr.error('Invalid PropertyId or ProposedRentAmount. Please check your input.', 'Error');
      return;
    }

    const data = {
      propertyId: propertyId,
      startDate: rawFormValues.startDate,
      endDate: rawFormValues.endDate,
      proposedRentAmount: proposedRentAmount,
      digitalSignature: this.selectedFile as File,
    };

    this.toastr.info('Submitting lease request...', 'Info');

    this.router.navigate(['/agreement-page'], {
      queryParams: {
        propertyId: propertyId,
        startDate: rawFormValues.startDate,
        endDate: rawFormValues.endDate,
        proposedRentAmount: proposedRentAmount,
      },
    });
  }
}
