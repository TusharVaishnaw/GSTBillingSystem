import { Routes } from '@angular/router';
import { Layout } from './layout/layout';
import { DashboardComponent } from './pages/dashboard/dashboard';
import { InvoicesComponent } from './pages/invoices/invoices';
import { Customers } from './pages/customers/customers';
import { Payments } from './pages/payments/payments';
import { CreateInvoiceComponent } from './pages/create-invoice/create-invoice'; 


export const routes: Routes = [
  {
    path: '',
    component: Layout,
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'invoices', component: InvoicesComponent },
      { path: 'customers', component: Customers },
      { path: 'payments', component: Payments },
      { path: 'create-invoice', component: CreateInvoiceComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];