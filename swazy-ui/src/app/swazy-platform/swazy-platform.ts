import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface PricingPlan {
  id: string;
  name: string;
  monthlyPrice: number;
  annualMonthlyPrice: number;
  annualYearlyPrice: number;
  annualSavings: number;
  features: string[];
  perfectFor: string;
  highlighted?: boolean;
}

@Component({
  selector: 'app-swazy-platform',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './swazy-platform.html',
  styleUrls: ['./swazy-platform.scss']
})
export class SwazyPlatformComponent {
  billingCycle = signal<'monthly' | 'annual'>('monthly');

  get isAnnual(): boolean {
    return this.billingCycle() === 'annual';
  }

  plans: PricingPlan[] = [
    {
      id: 'starter',
      name: 'Starter',
      monthlyPrice: 19900,
      annualMonthlyPrice: 17910,
      annualYearlyPrice: 214920,
      annualSavings: 23880,
      features: [
        '♾️ Korlátlan időpont',
        '♾️ Korlátlan dolgozó',
        '♾️ Korlátlan email',
        'Egyedi weboldal (32 sablon)',
        'Saját domain',
        'Opcionális nyereséges SMS',
        'Foglalási naptár',
        'Ügyfél adatbázis',
        'Statisztikák (foglalások, bevétel)',
      ],
      perfectFor: '1-3 dolgozó, kisebb szalonok'
    },
    {
      id: 'professional',
      name: 'Professional',
      monthlyPrice: 44900,
      annualMonthlyPrice: 40410,
      annualYearlyPrice: 484920,
      annualSavings: 53880,
      features: [
        '♾️ Korlátlan időpont',
        '♾️ Korlátlan dolgozó',
        '♾️ Korlátlan email',
        'Egyedi weboldal (32 sablon)',
        'Saját domain',
        'Opcionális nyereséges SMS',
        'Foglalási naptár',
        'Ügyfél adatbázis',
        'Statisztikák (foglalások, bevétel)',
        'Export funkciók (Excel, CSV)',
        'QR Kód a weboldaladhoz, analitikával',
        'Kiemelt támogatás'
      ],
      perfectFor: '4-8 dolgozó, növekvő szalonok',
      highlighted: true
    },
    {
      id: 'business',
      name: 'Business',
      monthlyPrice: 99900,
      annualMonthlyPrice: 89910,
      annualYearlyPrice: 1078920,
      annualSavings: 119880,
      features: [
        '♾️ Korlátlan időpont',
        '♾️ Korlátlan dolgozó',
        '♾️ Korlátlan email',
        'Egyedi weboldal (32 sablon)',
        'Saját domain',
        'Opcionális nyereséges SMS',
        'Foglalási naptár',
        'Ügyfél adatbázis',
        'Statisztikák (foglalások, bevétel)',
        'Export funkciók (Excel, CSV)',
        'QR Kód a weboldaladhoz, analitikával',
        'Kiemelt támogatás',
        'Több telephely kezelése',
        'Egyedi testreszabások, fejlesztések'
      ],
      perfectFor: '8-15 dolgozó, nagy szalonok / több telephely'
    }
  ];

  toggleBillingCycle() {
    this.billingCycle.set(this.isAnnual ? 'monthly' : 'annual');
  }

  openSignupModal(planId?: string) {
    if (planId) {
      localStorage.setItem('selectedPlan', planId);
      localStorage.setItem('selectedBillingCycle', this.billingCycle());
    }
    const modal = document.getElementById('signup_modal') as HTMLDialogElement;
    modal?.showModal();
  }

  scrollToSection(sectionId: string) {
    const element = document.getElementById(sectionId);
    element?.scrollIntoView({ behavior: 'smooth' });
  }

  formatPrice(price: number): string {
    return price.toLocaleString('hu-HU');
  }
}
