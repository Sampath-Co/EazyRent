import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  FormGroup,
  Validators,
  ValidatorFn,
  AbstractControl,
} from '@angular/forms';
import { AuthService } from '../../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router, RouterLink } from '@angular/router';
import { Navbar } from "../../navbar/navbar";

@Component({
  selector: 'app-registration',
  imports: [ReactiveFormsModule, CommonModule, RouterLink, Navbar],
  templateUrl: './registration.html',
  styleUrls: ['./registration.css'],
})
export class Registration implements OnInit {
  form: FormGroup;
  isSubmitted: boolean = false;
  ngOnInit(): void {
    if (this.service.isLoggedIn()) {
        this.router.navigateByUrl('/login');
    }
  }
  constructor(
    public formBuilder: FormBuilder,
    private service: AuthService,
    private toastr: ToastrService,
    private router: Router
  ) {
    // Initialize the form inside the constructor
    this.form = this.formBuilder.group(
      {
        fullName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(6),
            Validators.pattern(
              '^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,32}$'
            ),
          ],
        ],
        confirmPassword: [''],
        phoneNumber: [
          '',
          [
            Validators.required,
            Validators.pattern('^\\d{10}$'), // Updated pattern to strictly allow 10 digits
          ],
        ],
        role: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator }
    );
  }

  passwordMatchValidator: ValidatorFn = (
    control: AbstractControl
  ): { [key: string]: any } | null => {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if (
      password &&
      confirmPassword &&
      password.value !== confirmPassword.value
    ) {
      confirmPassword?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      confirmPassword?.setErrors(null);
    }
    return null;
  };

  // Method to handle form submission
  onSubmit(): void {
    this.isSubmitted = true;
    if (this.form.valid) {
      this.service.createUser(this.form.value).subscribe({
        next: (response) => {
          this.toastr.success('User registered successfully!', 'Success');
          this.form.reset();
          this.isSubmitted = false;
        },
        error: (error) => {
          this.toastr.error(
            'Failed to register user. Please try again.',
            'Error'
          );
        },
      });
    } else {
      this.toastr.error(
        'Form is invalid. Please check the fields and try again.',
        'Error'
      );
    }
  }

  hasDisplayError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return (
      Boolean(control?.invalid) &&
      (Boolean(control?.touched) || this.isSubmitted)
    );
  }
}