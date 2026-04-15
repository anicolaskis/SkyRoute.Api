import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';

/**
 * Application-wide providers.
 *
 * provideHttpClient()           — makes HttpClient available for injection everywhere.
 * provideRouter(routes)         — sets up the router with our route definitions.
 * provideZoneChangeDetection()  — enables efficient change detection (Angular default).
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
  ],
};
