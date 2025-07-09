// property.service.ts
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PropertyPayload } from '../../features/Property/add-property/add-property'; // Assuming PropertyPayload is suitable for update
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
@Injectable({
  providedIn: 'root',
})
export class PropertyService {
  private baseUrl = environment.baseUrl;

  constructor(private http: HttpClient, private authService: AuthService) {}

  addProperty(propertyData: PropertyPayload): Observable<void> {
    const formData = new FormData();
    formData.append('address', propertyData.address);
    formData.append('rentAmount', propertyData.rentAmount.toString());
    formData.append('availabilityStatus', propertyData.availabilityStatus);
    formData.append('propertyDescription', propertyData.propertyDescription);
    if (propertyData.propertyImage) {
      formData.append('propertyImage', propertyData.propertyImage); // File object
    }
    // Optionally support propertyImageBase64 for add (not typical, but for consistency)
    if (!propertyData.propertyImage && propertyData.propertyImageBase64) {
      formData.append('propertyImageBase64', propertyData.propertyImageBase64);
    }
    return this.http.post<void>(`${this.baseUrl}/Owner/AddProperty`, formData);
  }

  getProperties(): Observable<Property[]> {
    const role = this.authService.getUserRoles();

    let endpoint = '';
    if (role === 'Owner') {
      endpoint = `${this.baseUrl}/Owner/Properties`;
    } else if (role === 'Tenant') {
      endpoint = `${this.baseUrl}/Tenant/Properties`;
    } else {
      throw new Error('Unknown user role');
    }

    return this.http.get<Property[]>(endpoint);
  }

  // NEW: Fetch a single property by ID
  getPropertyById(propertyId: number): Observable<Property> {
    const role = this.authService.getUserRoles();

    let endpoint = '';
    if (role === 'Owner') {
      endpoint = `${this.baseUrl}/Owner/GetPropertyById/${propertyId}`;
    } else if (role === 'Tenant') {
      endpoint = `${this.baseUrl}/Tenant/GetPropertyById/${propertyId}`;
    } else {
      throw new Error('Unknown user role');
    }

    return this.http.get<Property>(endpoint);
  }

  // NEW: Update property method
  updateProperty(
    propertyId: number,
    propertyData: PropertyPayload
  ): Observable<void> {
    const formData = new FormData();
    formData.append('address', propertyData.address);
    formData.append('rentAmount', propertyData.rentAmount.toString());
    formData.append('availabilityStatus', propertyData.availabilityStatus);
    formData.append('propertyDescription', propertyData.propertyDescription);
    // Only append image if a new one is selected
    if (propertyData.propertyImage) {
      formData.append('propertyImage', propertyData.propertyImage);
    }
    // If no new image, but base64 exists, send it so backend can keep the old image
    if (!propertyData.propertyImage && propertyData.propertyImageBase64) {
      formData.append('propertyImageBase64', propertyData.propertyImageBase64);
    }
    // Backend endpoint for update: /Owner/UpdateProperty/{propertyId}
    return this.http.put<void>(
      `${this.baseUrl}/Owner/UpdateProperty/${propertyId}`,
      formData
    );
  }

  deleteProperty(propertyId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/Owner/DeleteProperty/${propertyId}`
    );
  }

  getPropertiesWithFilters(
    filterOn: string | null,
    filterQuery: string | null,
    filterRent: number | null
  ): Observable<Property[]> {
    const params: any = {};
    if (filterOn) params.filterOn = filterOn;
    if (filterQuery) params.filterQuery = filterQuery;
    if (filterRent) params.filterRent = filterRent;

    return this.http.get<Property[]>(`${this.baseUrl}/Tenant/Properties`, {
      params,
    });
  }
}

// Keep your Property interface here, with propertyId
export interface Property {
  propertyId: number;
  address: string;
  availabilityStatus: string;
  propertyDescription: string;
  propertyImageBase64: string;
  propertyImageMimeType?: string;
  rentAmount: number;
}
