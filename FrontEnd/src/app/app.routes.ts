import { Routes } from '@angular/router';

/**
 * Application routes.
 *
 * Each route lazy-loads its page component — only the code for the
 * current page is downloaded, keeping the initial bundle small.
 *
 * Route guard note: BookingPage and ConfirmationPage guard themselves
 * internally (redirect to '/' if state is missing). For a larger app
 * these would use proper CanActivate guards.
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/search/search-page.component').then(m => m.SearchPageComponent),
  },
  {
    path: 'booking',
    loadComponent: () =>
      import('./features/booking/booking-page.component').then(m => m.BookingPageComponent),
  },
  {
    path: 'confirmation',
    loadComponent: () =>
      import('./features/confirmation/confirmation-page.component').then(m => m.ConfirmationPageComponent),
  },
  // Catch-all: any unknown URL goes back to search
  { path: '**', redirectTo: '' },
];
