import { Component, OnInit } from '@angular/core';
import { PropertyService, Property } from '../../shared/services/property.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-property-page',
  templateUrl: './property-page.html',
  imports: [CommonModule],
  styleUrls: ['./property-page.css']
})
export class PropertyPage implements OnInit {
  property: Property | null = null;

  constructor(
    private propertyService: PropertyService,
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const propertyId = Number(this.route.snapshot.paramMap.get('id'));
    if (isNaN(propertyId)) {
      const invalidId = this.route.snapshot.paramMap.get('id');
      console.error('Invalid propertyId:', invalidId);
      this.toastr.error('Invalid property ID: ' + invalidId, 'Error');
      return;
    }

    this.propertyService.getPropertyById(propertyId).subscribe({
      next: (data) => {
        this.property = data;
        this.toastr.success('Property details loaded successfully.', 'Success');
      },
      error: (err) => {
        console.error('Error fetching property:', err);
        this.toastr.error('Error fetching property: ' + (err?.message || err), 'Error');
      }
    });
  }

  redirectToLeaseForm(): void {
    if (this.property) {
      console.log('Redirecting with:', {
        propertyId: this.property.propertyId,
        rentAmount: this.property.rentAmount
      });
      this.router.navigate(['/lease-request'], {
        queryParams: {
          rentAmount: this.property.rentAmount,
          propertyId: this.property.propertyId
        }
      });
      this.toastr.info('Redirecting to lease request form...', 'Info');
    } else {
      this.toastr.warning('No property selected to lease.', 'Warning');
    }
  }
}
