import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { AddProperty } from './add-property';

describe('AddPropertyComponent', () => {
  let component: AddProperty;
  let fixture: ComponentFixture<AddProperty>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddProperty, ReactiveFormsModule, HttpClientModule], // Add HttpClientModule here
    }).compileComponents();

    fixture = TestBed.createComponent(AddProperty);
    component = fixture.componentInstance;
    fixture.detectChanges(); // Ensure the component is initialized
  });

  // Test Case 1: Check if the component is created
  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // Test Case 2: Check if the form is invalid when empty
  it('should have an invalid form when empty', () => {
    expect(component.propertyForm.valid).toBeFalsy();
  });

  // Test Case 3: Check if the form is valid when all required fields are filled
  it('should have a valid form when all required fields are filled', () => {
    component.propertyForm.setValue({
      address: '123 Main Street',
      rentAmount: 5000,
      availabilityStatus: 'Available',
      propertyImage: new File(['dummy content'], 'test-image.jpg', { type: 'image/jpeg' }), // Added a valid image file
      propertyDescription: 'A beautiful property in the city center.',
      location: 'Downtown' // Ensure location is set
    });
    expect(component.propertyForm.valid).toBeTruthy();
  });
});