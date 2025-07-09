import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  FormGroup,
  Validators,
} from '@angular/forms';
import { AuthService } from '../../../shared/services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router, RouterLink } from '@angular/router';
import { TOKEN_KEY } from '../../../shared/constants';
import { Navbar } from "../../navbar/navbar";

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, CommonModule, RouterLink, Navbar],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login implements OnInit {
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
    this.form = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    this.isSubmitted = true;
    if (this.form.valid) {
      this.service.login(this.form.value).subscribe({
        next: (res: any) => {
          sessionStorage.setItem(TOKEN_KEY, res.token);
          const roles = this.service.getUserRoles();
          const username = this.service.getUsername();
          if (roles) {
            if (roles === 'Owner') {
              this.router.navigateByUrl('/owner-dashboard', { state: { username } });
            } else if (roles === 'Tenant') {
              this.router.navigateByUrl('/tenant-dashboard');
            } else {
              this.router.navigateByUrl('/login');
            }
          } else {
            this.toastr.error('No roles found for the user.', 'Error');
            this.router.navigateByUrl('/login');
          }
          this.toastr.success('Login successful!', 'Success');
        },
        error: (err) => {
          if (err.status === 400) {
            this.toastr.error('Invalid email or password.', 'Error');
          } else {
            this.toastr.error('An unexpected error occurred.', 'Error');
          }
        },
      });
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
