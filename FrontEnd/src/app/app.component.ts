import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * AppComponent — the root shell.
 * Its only job is to render the router outlet.
 * All layout and logic lives inside the feature page components.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class AppComponent {}
