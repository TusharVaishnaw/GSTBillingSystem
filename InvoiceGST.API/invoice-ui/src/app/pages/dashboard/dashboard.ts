import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { ApiService } from '../../api.service';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {

  totalInvoices: number = 0;

  constructor(private api: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.api.getInvoices().subscribe((res: any) => {
      console.log("TOTAL 👉", res.data.totalRecords);

      this.totalInvoices = res.data.totalRecords;

      this.cdr.detectChanges(); // 🔥 IMPORTANT
    });
  }
}