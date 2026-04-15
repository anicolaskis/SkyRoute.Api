import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';

import { BookingStateService } from '../../core/services/booking-state.service';
import { DurationPipe }        from '../../shared/pipes/duration.pipe';

@Component({
  selector: 'app-confirmation-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './confirmation-page.component.html',
  styleUrl: './confirmation-page.component.css',
})
export class ConfirmationPageComponent implements OnInit {

  private readonly bookingState = inject(BookingStateService);
  private readonly router       = inject(Router);

  readonly booking = this.bookingState.confirmedBooking;

  ngOnInit(): void {
    // Guard: if someone navigates directly to /confirmation, send them to search.
    if (!this.booking()) {
      this.router.navigate(['/']);
    }
  }

  onSearchAgain(): void {
    this.bookingState.clear();
    this.router.navigate(['/']);
  }
}
