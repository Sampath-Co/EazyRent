import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  FormsModule,
} from '@angular/forms';
import {
  MaintenanceService,
  MaintenanceRequestDto,
} from '../../shared/services/maintenance.service';
import { ToastrService } from 'ngx-toastr';
import { OwnerNavbar } from "../navbar/owner-navbar";

@Component({
  selector: 'app-maintenancedashboardowner',
  templateUrl: './maintenancedashboardowner.html',
  imports: [CommonModule, FormsModule, OwnerNavbar],
  styleUrl: './maintenancedashboardowner.css',
})
export class Maintenancedashboardowner implements OnInit {
  maintenanceForm: FormGroup;
  maintenanceRequests: MaintenanceRequestDto[] = [];
  pendingRequestsCount: number = 0; // Added property to store count of pending maintenance requests

  constructor(
    private fb: FormBuilder,
    private service: MaintenanceService,
    private toastr: ToastrService
  ) {
    this.maintenanceForm = this.fb.group({
      requestId: ['', Validators.required],
      propertyId: ['', Validators.required],
      tenantFullName: ['', Validators.required],
      issueDescription: ['', [Validators.required, Validators.maxLength(500)]],
      status: [{ value: 'Pending', disabled: true }, Validators.required],
    });
  }

  ngOnInit(): void {
    this.fetchRequests();
  }

  fetchRequests(): void {
    this.service.getAllRequests().subscribe({
      next: (res) => {
        this.maintenanceRequests = res;
        this.toastr.success(
          'Maintenance requests fetched successfully.',
          'Success'
        );
      },
      error: (err) => {
        this.toastr.error(
          'Failed to fetch maintenance requests: ' + (err?.message || err),
          'Error'
        );
      },
    });
  }

  updateStatus(req: MaintenanceRequestDto, status: string): void {
    const payload = { requestId: req.requestId, status };
    this.service.updateRequestStatus(req.requestId, status).subscribe({
      next: () => {
        req.status = status;
        this.toastr.success(
          `Status updated to ${status} successfully.`,
          'Success'
        );
      },
      error: (err) => {
        this.toastr.error(
          'Failed to update status: ' + (err?.message || err),
          'Error'
        );
      },
    });
  }
}
