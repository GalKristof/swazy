import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TenantService } from './services/tenant.service';
import { ToastService } from './services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App implements OnInit {
  private tenantService = inject(TenantService);
  toastService = inject(ToastService);

  isLoading = signal(true);
  title = 'swazy-ui';

  ngOnInit() {
    this.tenantService.loadBusinessData().subscribe({
      next: (business) => {
        console.log('✅ [App] Business data loaded:', business);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('❌ [App] Error loading business:', error);
        this.isLoading.set(false);
      }
    });
  }
}
