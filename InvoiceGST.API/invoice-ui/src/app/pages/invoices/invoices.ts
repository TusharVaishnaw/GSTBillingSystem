import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../api.service';
import { CommonModule } from '@angular/common';

// Material
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  templateUrl: './invoices.html',
  styleUrls: ['./invoices.css']
})
export class InvoicesComponent implements OnInit {

  displayedColumns: string[] = ['invoiceNumber', 'customer', 'amount', 'status'];

  dataSource: any[] = [];


  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  loadInvoices() {
  this.api.getInvoices().subscribe((res: any) => {
    console.log("INVOICES 👉", res);
    this.dataSource = res.data.data;
  });
}

  ngOnInit() {
  this.api.getInvoices().subscribe((res: any) => {
    this.dataSource = res.data.data;
    this.cdr.detectChanges(); 
    this.loadInvoices();

  });
}


}