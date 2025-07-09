import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SharedDataService {
  private digitalSignature: File | null = null;
  private leaseData = new BehaviorSubject<any[]>([]);
  private maintenanceRequests = new BehaviorSubject<any[]>([]);

  setDigitalSignature(file: File): void {
    this.digitalSignature = file;
  }

  getDigitalSignature(): File | null {
    return this.digitalSignature;
  }

  setLeaseData(data: any[]): void {
    this.leaseData.next(data);
  }

  getLeaseData() {
    return this.leaseData.asObservable();
  }

  setMaintenanceRequests(data: any[]): void {
    this.maintenanceRequests.next(data);
  }

  getMaintenanceRequests() {
    return this.maintenanceRequests.asObservable();
  }
}