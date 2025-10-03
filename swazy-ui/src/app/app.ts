import { Component, inject, OnInit, signal, PLATFORM_ID, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { registerLicense } from '@syncfusion/ej2-base';
import { TenantService } from './services/tenant.service';
import { ToastService } from './services/toast.service';
import { environment } from '../environments/environment';
import { of, timer, map, catchError, take, combineLatest, interval, Subscription } from 'rxjs';

interface SuccessResult { success: true; business: any; }
interface ErrorResult { success: false; error: any; }

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App implements OnInit, OnDestroy {
  private tenantService = inject(TenantService);
  toastService = inject(ToastService);
  private platformId = inject(PLATFORM_ID);

  isLoading = signal(true);
  hasError = signal(false);
  title = 'swazy-ui';
  loadingText = signal('Betöltés');
  private loadingSubscription: Subscription | undefined;

  private MIN_LOAD_TIME_MS = 1000;

  constructor() {
    registerLicense(environment.syncfusionKey);
  }

  ngOnInit() {
    // Start the tenant data loading sequence
    this.loadTenantData();
    // Start the loading animation only if we are in the browser
    this.startLoadingAnimation();
  }

  ngOnDestroy(): void {
    if (this.loadingSubscription) {
      this.loadingSubscription.unsubscribe();
    }
  }

  /**
   * Clears error and restarts the load process
   */
  retry() {
    this.loadTenantData();
    this.startLoadingAnimation();
  }

  private startLoadingAnimation() {
    if (!isPlatformBrowser(this.platformId)) return;

    if (this.loadingSubscription) {
      this.loadingSubscription.unsubscribe();
    }

    const phrases = ['Betöltés', 'Betöltés.', 'Betöltés..', 'Betöltés...'];
    let index = 0;

    this.loadingSubscription = interval(250).subscribe(() => {
      this.loadingText.set(phrases[index % phrases.length]);
      index++;

      if (!this.isLoading()) {
        if (this.loadingSubscription) {
          this.loadingSubscription.unsubscribe();
        }
      }
    });
  }

  private loadTenantData() {
    if (this.tenantService.getCurrentBusiness()) {
      console.log('✅ [App] Business data found via TransferState, skipping load sequence.');
      this.isLoading.set(false);
      this.hasError.set(false);
      return;
    }

    this.isLoading.set(true);
    this.hasError.set(false);

    const minTime$ = isPlatformBrowser(this.platformId)
      ? timer(this.MIN_LOAD_TIME_MS).pipe(map(() => true), take(1))
      : of(true); // Server ignores delay

    const dataLoad$ = this.tenantService.loadBusinessData().pipe(
      map(business => ({ success: true, business } as SuccessResult)),
      catchError(error => {
        console.error('❌ [App] Error loading business:', error);
        return of({ success: false, error } as ErrorResult);
      }),
      take(1)
    );

    combineLatest([minTime$, dataLoad$])
      .subscribe(([minTimeCompleted, result]) => {
        if (result.success) {
          const businessData = (result as SuccessResult).business;

          console.log('✅ [App] Business data loaded (min time fulfilled):', businessData);
          this.isLoading.set(false);
          this.hasError.set(false);
        } else {
          console.error('❌ [App] Data failed (min time fulfilled). Error:', (result as ErrorResult).error);
          this.isLoading.set(false);
          this.hasError.set(true);
          if (this.loadingSubscription) {
            this.loadingSubscription.unsubscribe();
          }
        }
      });
  }
}
