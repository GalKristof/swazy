import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantService } from '../../services/tenant.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.html'
})
export class FooterComponent {
  private tenantService = inject(TenantService);
  business$ = this.tenantService.business$;
}
