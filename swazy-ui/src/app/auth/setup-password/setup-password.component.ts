import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { TenantService } from '../../services/tenant.service';

@Component({
  selector: 'app-setup-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './setup-password.component.html',
  styleUrl: './setup-password.component.scss'
})
export class SetupPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastService = inject(ToastService);
  private tenantService = inject(TenantService);

  setupForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  invitationToken = '';
  tokenValid = true;
  tenantName = '';

  constructor() {
    this.setupForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8), this.passwordStrengthValidator]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.invitationToken = this.route.snapshot.paramMap.get('token') || '';

    if (!this.invitationToken) {
      this.tokenValid = false;
      this.errorMessage = 'Érvénytelen meghívó link';
      this.router.navigate(['/']);
      return;
    }

    this.tenantService.business$.subscribe((business: any) => {
      if (business) {
        this.tenantName = business.name;
      }
    });
  }

  passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);

    const valid = hasUpperCase && hasLowerCase && hasNumber;

    return valid ? null : { weakPassword: true };
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) return null;

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.setupForm.invalid || !this.tokenValid) {
      this.setupForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { password } = this.setupForm.value;

    this.authService.setupPassword(this.invitationToken, password).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.success('Jelszó sikeresen beállítva! Üdvözöljük!');
        this.router.navigate(['/manage']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.error || 'A jelszó beállítása sikertelen. A meghívó link lehet, hogy lejárt.';
        this.toastService.error(this.errorMessage);
      }
    });
  }

  get password() {
    return this.setupForm.get('password');
  }

  get confirmPassword() {
    return this.setupForm.get('confirmPassword');
  }
}
