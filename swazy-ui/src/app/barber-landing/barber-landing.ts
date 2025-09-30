import {Component, computed, inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import {TenantService} from '../services/tenant.service';

interface Barber {
  name: string;
  title: string;
  image: string;
}

interface Service {
  name: string;
  price: string;
  duration: string;
}

@Component({
  selector: 'app-barber-landing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './barber-landing.html',
  styleUrls: ['./barber-landing.scss']
})
export class BarberLandingComponent {
  private tenantService = inject(TenantService);
  business$ = this.tenantService.business$;

  barbers: Barber[] = [
    {
      name: 'Kovács János',
      title: 'Mester fodrász',
      image: 'https://images.unsplash.com/photo-1621605815971-fbc98d665033?w=400&h=400&fit=crop'
    },
    {
      name: 'Nagy Péter',
      title: 'Senior stylist',
      image: 'https://images.unsplash.com/photo-1622286342621-4bd786c2447c?w=400&h=400&fit=crop'
    },
    {
      name: 'Tóth Márk',
      title: 'Fodrász',
      image: 'https://images.unsplash.com/photo-1605497788044-5a32c7078486?w=400&h=400&fit=crop'
    }
  ];

  services: Service[] = [
    { name: 'Férfi hajvágás', price: '4 500 Ft', duration: '30 perc' },
    { name: 'Szakáll igazítás', price: '2 500 Ft', duration: '20 perc' },
    { name: 'Borotválás', price: '3 500 Ft', duration: '25 perc' },
    { name: 'Hajvágás + szakáll', price: '6 000 Ft', duration: '45 perc' }
  ];
}
