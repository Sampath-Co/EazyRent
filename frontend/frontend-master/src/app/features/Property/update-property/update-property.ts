// features/Property/update-property/update-property.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
} from '@angular/forms'; // Import AbstractControl
import { ActivatedRoute, Router } from '@angular/router';
import {
  PropertyService,
  Property,
} from '../../../shared/services/property.service';
import { PropertyPayload } from '../add-property/add-property';
import { Observable } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-update-property',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-property.html',
  styleUrl: './update-property.css',
})
export class UpdatePropertyComponent implements OnInit {
  propertyForm!: FormGroup;
  selectedFile: File | null = null;
  selectedImageUrl: string | null = null;
  isSubmitting = false;
  propertyId: number | null = null;
  isEditMode = false;
  existingImageBase64: string | null = null;

  constructor(
    private fb: FormBuilder,
    private propertyService: PropertyService,
    private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.propertyId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEditMode = !!this.propertyId;

    this.initializeForm();

    if (this.isEditMode && this.propertyId) {
      this.loadPropertyData(this.propertyId);
    }
  }

  initializeForm(): void {
    this.propertyForm = this.fb.group({
      address: ['', Validators.required],
      rentAmount: ['', [Validators.required, Validators.min(1)]],
      availabilityStatus: ['', Validators.required],
      propertyImage: [null, this.isEditMode ? null : Validators.required], // Image required for add, optional for update
      propertyDescription: [
        '',
        [Validators.required, Validators.maxLength(500)],
      ],
    });
  }

  loadPropertyData(id: number): void {
    this.propertyService.getPropertyById(id).subscribe({
      next: (property: Property) => {
        this.propertyForm.patchValue({
          address: property.address,
          rentAmount: property.rentAmount,
          availabilityStatus: property.availabilityStatus,
          propertyDescription: property.propertyDescription,
        });
        // Handle existing image
        if (property.propertyImageBase64) {
          let base64 = property.propertyImageBase64.trim();
          // If the string contains 'data:', use from there
          const dataIndex = base64.indexOf('data:');
          if (dataIndex !== -1) {
            base64 = base64.substring(dataIndex);
            this.selectedImageUrl = base64;
          } else {
            // fallback: construct the data URL as before
            const mimeType = property.propertyImageMimeType || 'image/jpeg';
            this.selectedImageUrl = `data:${mimeType};base64,${base64}`;
          }
          this.existingImageBase64 = property.propertyImageBase64;
          // Log the image URL
          console.log('Loaded property image URL:', this.selectedImageUrl);
          // When an image already exists for update, the file input is not required to be re-selected.
          this.propertyForm.get('propertyImage')?.setValidators(null);
          this.propertyForm.get('propertyImage')?.updateValueAndValidity();
        } else {
          // If there was no image, and it's an update, the image is still optional.
          // But if you wanted to enforce adding an image during update if one wasn't there before,
          // you'd set Validators.required here. For now, it remains optional.
          this.propertyForm.get('propertyImage')?.setValidators(null); // Keep it optional for update
          this.propertyForm.get('propertyImage')?.updateValueAndValidity();
          this.existingImageBase64 = null;
        }
        this.toastr.success('Property data loaded for editing.', 'Success');
      },
      error: (error) => {
        console.error('Error loading property data:', error);
        this.toastr.error('Failed to load property data for editing.', 'Error');
        this.router.navigate(['/owner/properties']);
      },
    });
  }

  onImageSelect(event: Event): void {
    const element = event.currentTarget as HTMLInputElement;
    let fileList: FileList | null = element.files;
    if (fileList && fileList.length > 0) {
      this.selectedFile = fileList[0];
      const reader = new FileReader();
      reader.onload = () => {
        this.selectedImageUrl = reader.result as string;
        this.existingImageBase64 = null;
        // Log the image URL after selection
        console.log('Selected image URL:', this.selectedImageUrl);
      };
      reader.readAsDataURL(this.selectedFile);
      this.propertyForm.patchValue({ propertyImage: this.selectedFile });
      this.propertyForm.get('propertyImage')?.updateValueAndValidity();
      this.toastr.success('Image selected successfully.', 'Success');
    } else {
      this.selectedFile = null;
      this.selectedImageUrl = null;
      this.propertyForm.patchValue({ propertyImage: null });
      this.toastr.info('No image selected.', 'Info');
    }
  }

  removeImage(): void {
    this.selectedFile = null;
    this.selectedImageUrl = null;
    this.propertyForm.patchValue({ propertyImage: null });
    this.existingImageBase64 = null;
    // Log image removal
    console.log(
      'Image removed. selectedImageUrl is now:',
      this.selectedImageUrl
    );
    // If it's an add property form, and the image is removed, it becomes required again.
    // If it's an update form, and the existing image is removed, it remains optional (unless you want to force a new upload).
    if (!this.isEditMode) {
      // Only set back to required if in Add mode
      this.propertyForm
        .get('propertyImage')
        ?.setValidators(Validators.required);
      this.propertyForm.get('propertyImage')?.updateValueAndValidity();
    }
    this.toastr.info('Image removed.', 'Info');
  }

  // Directly check field validity
  isFieldInvalid(fieldName: string): boolean {
    const field = this.propertyForm.get(fieldName);
    // Return true if the field exists, has errors, and has been touched or is dirty
    return !!field && field.invalid && (field.touched || field.dirty);
  }

  onSubmit(): void {
    this.propertyForm.markAllAsTouched();
    if (this.propertyForm.invalid) {
      this.toastr.error(
        'Please fix the errors in the form before submitting.',
        'Form Invalid'
      );
      console.log('Form is invalid:', this.propertyForm.errors);
      return;
    }

    this.isSubmitting = true;

    const formValue = this.propertyForm.value;
    const propertyPayload: PropertyPayload = {
      address: formValue.address,
      rentAmount: formValue.rentAmount,
      availabilityStatus: formValue.availabilityStatus,
      propertyDescription: formValue.propertyDescription,
      propertyImage: this.selectedFile || undefined,
      propertyImageBase64:
        !this.selectedFile && this.existingImageBase64
          ? this.existingImageBase64
          : undefined,
    };

    // Log the image URL at submit
    console.log('Submitting with image URL:', this.selectedImageUrl);

    let apiCall: Observable<void>;

    if (this.isEditMode && this.propertyId) {
      apiCall = this.propertyService.updateProperty(
        this.propertyId,
        propertyPayload
      );
    } else {
      // This path should ideally not be taken in the UpdatePropertyComponent
      apiCall = this.propertyService.addProperty(propertyPayload);
    }

    apiCall.subscribe({
      next: () => {
        const successMessage = this.isEditMode
          ? 'Property updated successfully!'
          : 'Property added successfully!';
        this.toastr.success(successMessage, 'Success');
        this.isSubmitting = false;
        this.router.navigate(['/owner/properties']);
      },
      error: (error) => {
        console.error('Operation failed:', error);
        this.toastr.error(
          `Failed to ${
            this.isEditMode ? 'update' : 'add'
          } property. Please try again.`,
          'Error'
        );
        this.isSubmitting = false;
      },
    });
  }

  onCancel(): void {
    this.toastr.info('Update cancelled.', 'Cancelled');
    this.router.navigate(['/owner/properties']);
  }
}
