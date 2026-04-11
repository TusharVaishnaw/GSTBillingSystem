import { Component } from '@angular/core';
import { ApiService } from '../../api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';


@Component({
  selector: 'app-create-invoice',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSelectModule],
  templateUrl: './create-invoice.html',
  styleUrls: ['./create-invoice.css']
})
export class CreateInvoiceComponent {

  customerId: number = 0;

  items = [
    { productName: '', quantity: 1, rate: 0 }
  ];

  constructor(private api: ApiService, private router: Router) {}

  customers: any[] = [];

  ngOnInit() {
    this.api.getCustomers().subscribe((res: any) => {
      this.customers = res.data.data;
    });
  }

  addItem() {
    this.items.push({ productName: '', quantity: 1, rate: 0 });
  }

  removeItem(index: number) {
    this.items.splice(index, 1);
  }

  createInvoice() {
    const data = {
      customerId: this.customerId,
      items: this.items
    };

    console.log("SENDING 👉", data);

    this.api.createInvoice(data).subscribe(() => {
      alert("Invoice Created 🔥");

    this.router.navigate(['/invoices']);
    });
  }

  getTotal() {
  return this.items.reduce((sum, item) => {
    return sum + (item.quantity * item.rate);
  }, 0);
}
}