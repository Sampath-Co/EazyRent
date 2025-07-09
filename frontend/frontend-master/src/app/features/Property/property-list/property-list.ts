// property-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { PropertyService, Property } from '../../../shared/services/property.service';
import { TOKEN_KEY } from '../../../shared/constants';
import { FormsModule } from '@angular/forms';
import { OwnerNavbar } from '../../../components/navbar/owner-navbar';
import { TenantNavbar } from '../../../components/navbar/tenant-navbar';

@Component({
  selector: 'app-property-list',
  standalone: true,
  imports: [CommonModule, RouterLink,FormsModule,OwnerNavbar,TenantNavbar],
  templateUrl: './property-list.html',
  styleUrl: './property-list.css',
})
export class PropertyList implements OnInit {
  properties: Property[] = [];
  userRole: string | null = null;

  // Filter variables
  filterOn: string | null = null;
  filterQuery: string | null = null;
  filterRent: number | null = null;

  constructor(private propertyService: PropertyService, private router: Router) {}

  ngOnInit(): void {
    this.getUserRole();
    this.fetchProperties();
  }

  getUserRole(): void {
    const token = sessionStorage.getItem(TOKEN_KEY);
    if (token) {
      try {
        const jwtPayload = JSON.parse(atob(token.split('.')[1]));
        this.userRole = jwtPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
      } catch (e) {
        console.error('Error parsing JWT token:', e);
        this.userRole = null;
      }
    } else {
      this.userRole = null;
    }
    console.log('User Role:', this.userRole);
  }

  fetchProperties(): void {
    this.propertyService.getProperties().subscribe({
      next: (response: Property[]) => {
        this.properties = response;
        console.log('Fetched properties:', this.properties);
      },
      error: (error: any) => {
        console.error('Error fetching properties:', error);
      },
    });
  }

  applyFilters(): void {
    this.propertyService
      .getPropertiesWithFilters(this.filterOn, this.filterQuery, this.filterRent)
      .subscribe({
        next: (response: Property[]) => {
          this.properties = response;
          console.log('Filtered properties:', this.properties);
        },
        error: (error: any) => {
          console.error('Error applying filters:', error);
        },
      });
  }

  resetFilters(): void {
    this.filterOn = null;
    this.filterQuery = null;
    this.filterRent = null;
    this.fetchProperties(); // Reload all properties
  }

  formatPrice(price: number): string {
    return '₹' + price.toLocaleString('en-IN');
  }

  onUpdateProperty(propertyId: number): void {
    this.router.navigate(['owner/update-property', propertyId]);
  }

  onDeleteProperty(propertyId: number): void {
    if (confirm('Are you sure you want to delete this property? This action cannot be undone.')) {
      this.propertyService.deleteProperty(propertyId).subscribe({
        next: () => {
          console.log(`Property with ID ${propertyId} deleted successfully!`);
          this.properties = this.properties.filter((p) => p.propertyId !== propertyId);
          alert('Property deleted successfully.');
        },
        error: (error) => {
          console.error('Error deleting property:', error);
          alert('Failed to delete property. Please try again.');
        },
      });
    }
  }
}