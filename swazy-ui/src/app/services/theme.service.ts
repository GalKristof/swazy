import { Injectable, Inject, Renderer2, RendererFactory2 } from '@angular/core';
import { DOCUMENT } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private renderer: Renderer2;
  private themeLinkElement: HTMLLinkElement | null = null;

  // Define available themes. In a real app, this could come from a config or API.
  // The paths assume themes are available relative to the application root (e.g., from angular.json styles).
  // Or, if using asset copying, paths would be like 'assets/themes/lara-light-blue/theme.css'.
  private availableThemes: { [key: string]: string } = {
    'lara-light-blue': 'node_modules/primeng/resources/themes/lara-light-blue/theme.css',
    'lara-dark-blue': 'node_modules/primeng/resources/themes/lara-dark-blue/theme.css',
    // Add more themes here, e.g.:
    // 'material-light-indigo': 'node_modules/primeng/resources/themes/material-light-indigo/theme.css',
    // 'bootstrap-dark-blue': 'node_modules/primeng/resources/themes/bootstrap4-dark-blue/theme.css'
  };

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private rendererFactory: RendererFactory2
  ) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.initializeThemeLinkElement();
  }

  private initializeThemeLinkElement() {
    this.themeLinkElement = this.document.getElementById('app-theme') as HTMLLinkElement;
    if (!this.themeLinkElement) {
      console.error('Theme link element with id "app-theme" not found in index.html.');
    }
  }

  setTheme(themeName: string) {
    if (!this.themeLinkElement) {
      console.error('Theme link element not initialized.');
      return;
    }

    const themePath = this.availableThemes[themeName];
    if (themePath) {
      this.renderer.setAttribute(this.themeLinkElement, 'href', themePath);
      console.log(`Theme changed to: ${themeName} (Path: ${themePath})`);

      // Optional: Store preference
      // localStorage.setItem('selected-theme', themeName);
    } else {
      console.warn(`Theme "${themeName}" not found in availableThemes.`);
    }
  }

  // Optional: Method to load initial theme, e.g., from localStorage
  // loadInitialTheme() {
  //   const savedTheme = localStorage.getItem('selected-theme');
  //   if (savedTheme && this.availableThemes[savedTheme]) {
  //     this.setTheme(savedTheme);
  //   } else {
  //     this.setTheme('lara-light-blue'); // Default theme
  //   }
  // }
}
