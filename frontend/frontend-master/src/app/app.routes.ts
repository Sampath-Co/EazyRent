import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { Registration } from './components/user/registration/registration';
import { Login } from './components/user/login/login';
import { OwnerDashboardComponent } from './components/user/ownerdashboard/ownerdashboard';
import { Tenantdashboard } from './components/user/tenantdashboard/tenantdashboard';
import { authGuard } from './shared/auth-guard';
import { Forbidden } from './components/forbidden/forbidden';
import { Maintenancedashboardtenant } from './components/maintenancedashboardtenant/maintenancedashboardtenant';
import { Maintenancedashboardowner } from './components/maintenancedashboardowner/maintenancedashboardowner';
import { PropertyList } from './features/Property/property-list/property-list';
import { AddProperty } from './features/Property/add-property/add-property';

import { Leaseformtenant } from './leaseformtenant/leaseformtenant';
import { Getleaseowner } from './components/getleaseowner/getleaseowner';
import { PropertyPage } from './components/property-page/property-page';
import { PaymentPage } from './payment-page/payment-page';
import { AgreementPage } from './agreement-page/agreement-page';
import { LeasetenantPage } from './leasetenant-page/leasetenant-page';
import { PaymentDashboard } from './components/payment-dashboard/payment-dashboard';
import { MaintenanceTenant } from './components/maintenance-tenant/maintenance-tenant';

export const routes: Routes = [
  {
    path: '',
    component: Home,
  },
  { path: 'registration', component: Registration },
  { path: 'login', component: Login },
  {
    path: 'owner-dashboard',
    component: OwnerDashboardComponent,
    canActivate: [authGuard],
    data: { requiredRole: 'Owner' },
    canActivateChild: [authGuard],
    children: [],
  },
  {
    path: 'tenant-dashboard',
    component: Tenantdashboard,
    canActivate: [authGuard],
    data: { requiredRole: 'Tenant' },
    canActivateChild: [authGuard],
    children: [],
  },
  {
    path: 'forbidden',
    component: Forbidden,
  },
  // { path: 'owner-dashboard', component: OwnerDashboardComponent },
  {
    path: 'tenant-dashboard',
    component: Tenantdashboard,
    canActivate: [authGuard],
    data: { requiredRole: 'Tenant' },
  },
  { path: 'Property-list', component: PropertyList },
  { path: 'add-property', component: AddProperty },
  { path: 'tenant-maintenance', component: Maintenancedashboardtenant },
  { path: 'owner-maintenance', component: Maintenancedashboardowner },
  {
    path: 'owner/update-property/:id', // Notice the ':id' for the route parameter
    loadComponent: () =>
      import('./features/Property/update-property/update-property').then(
        (m) => m.UpdatePropertyComponent
      ),
    canActivate: [authGuard],
    data: { roles: ['Owner'] },
  }, // Only Owners can update properties

  { path: 'lease-request', component: Leaseformtenant },
  { path: 'lease-owner', component: Getleaseowner },
  { path: 'property-page/:id', component: PropertyPage },
  { path: 'agreement-page', component: AgreementPage },
  { path: 'payment', component: PaymentPage },
  { path: 'getleasetenant', component: LeasetenantPage },
  { path: 'payments', component: PaymentDashboard },
  { path: 'maintenance-tenant', component: MaintenanceTenant },
  // { path: '**', redirectTo: '', pathMatch: 'full' }
];
