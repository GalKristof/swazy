import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes'; // if you have app.routes.ts
import { provideClientHydration } from '@angular/platform-browser';
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes), // or provideRouter([]) if no app.routes.ts
    provideHttpClient(withFetch()),
    provideClientHydration()
    // Add other global providers here if needed
  ]
};
