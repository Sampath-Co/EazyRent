import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-owner-navbar',
  standalone: true,
  imports: [RouterModule],
  template: `
    <nav
      class="navbar navbar-expand-lg navbar-light bg-white shadow-sm sticky-top"
    >
      <div class="container-fluid px-4">
        <a
          class="navbar-brand d-flex align-items-center"
          routerLink="/owner/dashboard"
          style="cursor: pointer;"
        >
          <div class="logo-container">
            <span class="brand-text ms-2">EazyRent</span>
          </div>
        </a>
        <div class="navbar-nav-container d-none d-lg-flex mx-auto">
          <div class="nav-pills d-flex">
            <a
              class="nav-item nav-link"
              routerLink="/owner-dashboard"
              routerLinkActive="active"
              >Dashboard</a
            >
            <a
              class="nav-item nav-link"
              routerLink="/Property-list"
              routerLinkActive="active"
              >Properties</a
            >
            <a
              class="nav-item nav-link"
              routerLink="/lease-owner"
              routerLinkActive="active"
              >Leases</a
            >
            <a
              class="nav-item nav-link"
              routerLink="/owner-maintenance"
              routerLinkActive="active"
              >Maintenance</a
            >
            <a
              class="nav-item nav-link"
              routerLink="/payments"
              routerLinkActive="active"
              >Payments</a
            >
          </div>
        </div>
        <div class="navbar-actions d-flex align-items-center">
          <button
            class="btn btn-outline-danger ms-2"
            type="button"
            (click)="onLogout()"
          >
            <i class="fas fa-sign-out-alt me-1"></i> Logout
          </button>
        </div>
      </div>
    </nav>
  `,
  styleUrls: ['./navbar.css'],
})
export class OwnerNavbar {
  constructor(private router: Router) {}
  onLogout() {
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
