import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

import { MenubarModule } from 'primeng/menubar';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';

import { ThemeService } from './services/theme.service'; // Import ThemeService

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MenubarModule,
    ButtonModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  menuItems: MenuItem[] = [];

  constructor(private themeService: ThemeService) {} // Inject ThemeService

  ngOnInit() {
    // Set an initial theme (e.g., from tenant config or user preference)
    // For now, defaulting to 'lara-light-blue'.
    // If you have loadInitialTheme in ThemeService, you might call it here.
    // this.themeService.loadInitialTheme(); // Example if you implement this
    this.themeService.setTheme('lara-light-blue');


    this.menuItems = [
      {
        label: 'Services Admin',
        routerLink: '/services',
        routerLinkActiveOptions: { exact: true }
      },
      {
        label: 'Businesses Admin',
        routerLink: '/businesses'
      },
      {
        label: 'Business Services',
        routerLink: '/business-services'
      },
      {
        label: 'Booking',
        routerLink: '/booking'
      },
      {
        label: 'Bookings',
        routerLink: '/bookings'
      },
      {
        label: 'Scheduler',
        routerLink: '/scheduler'
      },
      {
        label: 'Employees',
        routerLink: '/employees'
      }
      // Add more items as needed
    ];
  }

  changeTheme(theme: string) {
    this.themeService.setTheme(theme);
  }
}
