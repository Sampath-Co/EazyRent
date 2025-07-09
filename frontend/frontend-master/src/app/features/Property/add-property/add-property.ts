// add-property.component.ts
import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { PropertyService } from '../../../shared/services/property.service'; // <-- Use PropertyService
import { ToastrService } from 'ngx-toastr';

// Collect the details and pass it to the api
// This interface now reflects the data sent to the backend,
// aligning with PropertyDetailsDTO
export interface PropertyPayload {
  address: string;
  rentAmount: number;
  availabilityStatus: 'Available' | 'Rented';
  propertyImage?: File | null;
  propertyImageBase64?: string;
  propertyDescription: string;
}

@Component({
  selector: 'app-add-property',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-property.html',
  styleUrl: './add-property.css',
})
export class AddProperty implements OnInit {
  @Output() propertyAdded = new EventEmitter<PropertyPayload>(); // Emitting the aligned data
  @Output() cancelled = new EventEmitter<void>();

  propertyForm: FormGroup;
  selectedImageUrl: string | null = null;
  isSubmitting: boolean = false;

  constructor(
    private ps: PropertyService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    // <-- Use PropertyService
    this.propertyForm = this.fb.group({
      address: ['', [Validators.required]],
      rentAmount: ['', [Validators.required, Validators.min(1)]],
      availabilityStatus: ['', [Validators.required]],
      propertyImage: ['', [Validators.required]],
      propertyDescription: [
        '',
        [Validators.required, Validators.maxLength(500)],
      ], // Change 'description' to 'propertyDescription'
    });
  }

  ngOnInit(): void {}

  isFieldInvalid(fieldName: string): boolean {
    const field = this.propertyForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onImageSelect(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file size (5MB limit)
      if (file.size > 5 * 1024 * 1024) {
        this.toastr.error('File size should not exceed 5MB', 'Image Error');
        return;
      }

      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.toastr.error('Please select a valid image file', 'Image Error');
        return;
      }

      this.selectedImageUrl = URL.createObjectURL(file); // For preview
      this.propertyForm.patchValue({
        propertyImage: file, // ✅ Store the File object, not Base64
      });
      this.toastr.success('Image selected successfully', 'Success');
    }
  }

  removeImage(): void {
    this.selectedImageUrl = null;
    this.propertyForm.patchValue({
      propertyImage: '',
    });
    this.toastr.info('Image removed', 'Info');
  }

  onSubmit(): void {
    if (this.propertyForm.valid) {
      this.isSubmitting = true;
      const formData: PropertyPayload = {
        address: this.propertyForm.value.address,
        rentAmount: this.propertyForm.value.rentAmount,
        availabilityStatus: this.propertyForm.value.availabilityStatus,
        propertyImage: this.propertyForm.value.propertyImage, // This should be a File object
        propertyImageBase64: this.propertyForm.value.propertyImageBase64,
        propertyDescription: this.propertyForm.value.propertyDescription,
      };

      this.ps.addProperty(formData).subscribe({
        next: () => {
          this.propertyAdded.emit(formData); // Emit the same data that was sent
          this.isSubmitting = false;
          this.toastr.success('Property added successfully!', 'Success');
          this.resetForm();
        },
        error: (error) => {
          this.isSubmitting = false;
          this.toastr.error(
            error?.error?.message || 'Failed to add property',
            'Error'
          );
        },
      });
    } else {
      this.toastr.error('Please fix the errors in the form.', 'Form Invalid');
      // Mark all fields as touched to show validation errors
      Object.keys(this.propertyForm.controls).forEach((key) => {
        this.propertyForm.get(key)?.markAsTouched();
        // Optionally, show which field is invalid
        if (this.propertyForm.get(key)?.invalid) {
          this.toastr.warning(`Field "${key}" is invalid.`, 'Validation');
        }
      });
    }
  }

  onCancel(): void {
    this.cancelled.emit();
    this.resetForm();
    this.toastr.info('Property addition cancelled', 'Cancelled');
  }

  private resetForm(): void {
    this.propertyForm.reset();
    this.selectedImageUrl = null;
  }
}
