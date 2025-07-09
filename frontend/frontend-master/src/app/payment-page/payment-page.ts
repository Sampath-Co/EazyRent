import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LeaseService } from '../shared/services/lease.service';
import { PaymentService } from '../shared/services/payment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-payment-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-page.html',
  styleUrl: './payment-page.css'
})
export class PaymentPage implements OnInit {
  rentAmount: number = 0;
  leaseId: number = 0;
  paymentSuccess: boolean = false;
  paymentMethod: string = 'upi';
  upiId: string = '';

  constructor(private route: ActivatedRoute, private leaseService: LeaseService, private paymentService: PaymentService, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.leaseId = +params['leaseId'];
      this.rentAmount = +params['rentAmount']; 
      console.log('Query params received in payment page:', params);
    });
  }

  isPaymentDetailsValid(): boolean {
    if (this.paymentMethod === 'upi') {
      return this.upiId.trim() !== '';
    } else if (this.paymentMethod === 'card') {
      // Add card validation logic here if needed
      return true;
    }
    return false;
  }

  makePayment(): void {
    if (this.paymentMethod === 'upi' && this.upiId.trim() === '') {
      alert('Please enter a valid UPI ID.');
      return;
    }

    this.paymentService.updatePaymentStatus(this.leaseId).subscribe({
      next: () => {
        this.paymentSuccess = true;
        setTimeout(() => {
          alert('Payment successful!');
          this.router.navigate(['/tenant-dashboard']);
        }, 2000);
      },
      error: (err:any) => alert('Payment failed: ' + err.message)
    });
  }
}
