import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5069/api';

  getInvoices() {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get(`${this.baseUrl}/InvoicesApi`, { headers });
  }

      createInvoice(data: any) {
      const token = localStorage.getItem('token');

      const headers = new HttpHeaders({
        Authorization: `Bearer ${token}`
      });

      return this.http.post(`${this.baseUrl}/InvoicesApi`, data, { headers });
    }

    getCustomers() {
      const token = localStorage.getItem('token');

      const headers = new HttpHeaders({
        Authorization: `Bearer ${token}`
      });

      return this.http.get(`${this.baseUrl}/CustomersApi`, { headers });
    }

}