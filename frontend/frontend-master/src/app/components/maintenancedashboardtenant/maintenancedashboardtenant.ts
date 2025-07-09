import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MaintenanceService, MaintenanceRequestDto } from '../../shared/services/maintenance.service';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-maintenancedashboardtenant',
  templateUrl: './maintenancedashboardtenant.html',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  styleUrl: './maintenancedashboardtenant.css',
})
export class Maintenancedashboardtenant {
  maintenanceRequests: MaintenanceRequestDto[] = [];
  maintenanceForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: MaintenanceService,
    private toastr: ToastrService,
    private route: ActivatedRoute // Inject ActivatedRoute to read query parameters
  ) {
    this.maintenanceForm = this.fb.group({
      propertyId: [{ value: '', disabled: true }, Validators.required], // Read-only field
      issueDescription: ['', [Validators.required, Validators.maxLength(500)]],
      status: [{ value: 'Pending', disabled: true }, Validators.required],
    });
  }

  ngOnInit(): void {
    // Get the propertyId and tenantId from query parameters
    this.route.queryParams.subscribe((params) => {
      const propertyId = params['propertyId'];
      const tenantId = params['tenantId'];
      if (propertyId && tenantId) {
        this.maintenanceForm.patchValue({ propertyId }); // Set the propertyId in the form
        this.maintenanceForm.addControl('tenantId', this.fb.control({ value: tenantId, disabled: true }, Validators.required)); // Add tenantId as a read-only field
      } else {
        this.toastr.error('Property ID or Tenant ID is missing.', 'Error');
      }
    });
  }

  onSubmit(): void {
    if (this.maintenanceForm.valid) {
      const formValue = { ...this.maintenanceForm.getRawValue(), status: 'Pending' };
      this.service.createRequest(formValue).subscribe({
        next: (response: any) => {
          this.toastr.success(response?.message || 'Request submitted successfully');
          this.maintenanceForm.reset({ status: 'Pending' });
        },
        error: (error) => {
          const errorMsg = error?.error || 'Failed to submit request';
          console.error('Maintenance request submission error:', error);
          this.toastr.error(errorMsg);
        },
      });
    }
  }
}
