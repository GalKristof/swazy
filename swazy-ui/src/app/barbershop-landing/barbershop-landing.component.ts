import { Component } from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

// Interface Definitions
interface Service {
  id: string;
  name: string;
  description: string;
  price: string;
  icon: SafeHtml;
}

interface Barber {
  id: string;
  name: string;
  specialization: string;
  experience: number;
  avatar: string;
  personalityTrait: string;
}

interface Testimonial {
  id: string;
  customerName: string;
  rating: number;
  comment: string;
  avatar: string;
}

@Component({
  selector: 'app-barbershop-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, NgOptimizedImage],
  templateUrl: './barbershop-landing.component.html',
  styleUrls: ['./barbershop-landing.component.scss']
})
export class BarbershopLandingComponent {

  services: Service[];
  barbers: Barber[];
  testimonials: Testimonial[];

  public currentYear: number = new Date().getFullYear();

  constructor(private router: Router, private sanitizer: DomSanitizer) {
    this.services = [
      { id: 's1', name: 'Precíziós hajvágás', description: 'Modern vagy klasszikus stílus, az elképzeléseidhez és arcodhoz igazítva.', price: '8.000 Ft-tól', icon: this.sanitizer.bypassSecurityTrustHtml('<svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M7.63 7.888A5.018 5.018 0 018.5 5.5a5 5 0 017 3.585m-9.637 6.415a5.018 5.018 0 00-.87 2.388 5 5 0 007-3.585M8.5 5.5L15.5 12M5.5 8.5L12 15.5" /></svg>') },
      { id: 's2', name: 'Klasszikus borotválás forró törölközővel', description: 'Kényeztető, teljeskörű borotválkozási élmény, ahogy a nagy könyvben meg van írva.', price: '7.000 Ft-tól', icon: this.sanitizer.bypassSecurityTrustHtml('<svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" /></svg>') },
      { id: 's3', name: 'Szakállformázás és ápolás', description: 'Legyen az rövid vagy hosszú, formázzuk és ápoljuk szakálladat prémium termékekkel.', price: '5.000 Ft-tól', icon: this.sanitizer.bypassSecurityTrustHtml('<svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M9 10l2 2m0 0l2-2m-2 2V6m0 8a2 2 0 100-4 2 2 0 000 4zm0 0v1m0-1a2 2 0 100-4 2 2 0 000 4zm0 0h1m-1 0V6M6 15a6 6 0 1012 0H6z" /></svg>') },
      { id: 's4', name: 'Hajtetoválás', description: 'Egyedi minták és vonalak a stílusod kiemeléséért.', price: 'Egyedi ár', icon: this.sanitizer.bypassSecurityTrustHtml('<svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M17.657 18.657A8 8 0 016.343 7.343M15 3l-3.05 3.05a2.5 2.5 0 000 3.536L15 12.636M12 21l3.05-3.05a2.5 2.5 0 000-3.536L12 11.364M6.343 18.657L9 16.001M17.657 7.343L15 9.999" /></svg>') },
      { id: 's5', name: 'Deluxe ápolási csomag', description: 'Hajvágás, borotválás vagy szakálligazítás, és extra kényeztetés egyben.', price: '15.000 Ft-tól', icon: this.sanitizer.bypassSecurityTrustHtml('<svg class="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v1h2a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V8a2 2 0 012-2h2V5zm14 0H5v10h14V5zm-4 4a2 2 0 11-4 0 2 2 0 014 0z" /></svg>') }
    ];

    // Using more reliable image sources
    this.barbers = [
      {
        id: 'b1',
        name: 'Kovács János',
        specialization: 'Fade specialista',
        experience: 8,
        avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=400&fit=crop&crop=face',
        personalityTrait: 'Precíz és kreatív'
      },
      {
        id: 'b2',
        name: 'Nagy László',
        specialization: 'Klasszikus vágások mestere',
        experience: 12,
        avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&h=400&fit=crop&crop=face',
        personalityTrait: 'Barátságos és tapasztalt'
      },
      {
        id: 'b3',
        name: 'Szabó István',
        specialization: 'Szakállkirály',
        experience: 6,
        avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400&h=400&fit=crop&crop=face',
        personalityTrait: 'Innovatív és laza'
      }
    ];

    this.testimonials = [
      {
        id: 't1',
        customerName: 'Molnár Péter',
        rating: 5,
        comment: 'A legjobb borbélyüzlet Budapesten! János mindig tökéletes munkát végez.',
        avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&h=100&fit=crop&crop=face'
      },
      {
        id: 't2',
        customerName: 'Kiss Dávid',
        rating: 5,
        comment: 'Professzionális csapat, remek hangulat és ingyen sör. Mi kell még?',
        avatar: 'https://images.unsplash.com/photo-1599566150163-29194dcaad36?w=100&h=100&fit=crop&crop=face'
      },
      {
        id: 't3',
        customerName: 'Varga Zoltán',
        rating: 4,
        comment: 'László klasszikus vágásai verhetetlenek. Kicsit drága, de megéri.',
        avatar: 'https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=100&h=100&fit=crop&crop=face'
      }
    ];
  }

  onBookAppointment(barberId?: string): void {
    if (barberId) {
      this.router.navigate(['/booking'], { queryParams: { barber: barberId } });
    } else {
      this.router.navigate(['/booking']);
    }
  }

  onServiceDetails(serviceId: string): void {
    console.log('Service details requested for:', serviceId);
  }

  onScrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  // Method to handle image loading errors
  onImageError(event: any): void {
    // Fallback to a default avatar if image fails to load
    event.target.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiBmaWxsPSIjRTVFN0VCIi8+CjxwYXRoIGQ9Ik0yMCAyMEM4Ljk1NDMgMjAgMjAgOC45NTQzIDIwIDIwVjIwWk0yMCAyMEM4Ljk1NDMgMjAgMjAgOC45NTQzIDIwIDIwVjIwWiIgZmlsbD0iIzlDQTNBRiIvPgo8L3N2Zz4K';
  }
}
