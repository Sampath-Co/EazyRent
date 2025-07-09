import { Component, OnInit } from '@angular/core';
import { MaintenanceService, MaintenanceRequestDto } from '../../shared/services/maintenance.service';
import { CommonModule } from '@angular/common';
import { TenantNavbar } from "../navbar/tenant-navbar";

@Component({
  selector: 'app-maintenance-tenant',
  templateUrl: './maintenance-tenant.html',
  styleUrl: './maintenance-tenant.css',
  imports: [CommonModule, TenantNavbar],
})
export class MaintenanceTenant implements OnInit {
  maintenanceRequests: MaintenanceRequestDto[] = [];
  isLoading = false;
  error: string | null = null;

  constructor(private maintenanceService: MaintenanceService) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.maintenanceService.getAllRequests().subscribe({
      next: (requests) => {
        this.maintenanceRequests = requests;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load maintenance requests.';
        this.isLoading = false;
      }
    });
  }
}
