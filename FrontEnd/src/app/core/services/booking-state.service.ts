import { Injectable, signal } from '@angular/core';
import { FlightOffer, BookingResponse } from '../models/flight.models';

/**
 * BookingStateService — shares data between the search page and the booking page.
 *
 * When a user clicks "Book" on a flight, we store that selection here
 * and navigate to /booking. The booking component reads it back.
 * We also store the confirmed booking so the confirmation screen can display it.
 *
 * Why not use query params or route state?
 * — The flight offer object is too large and complex for a URL.
 * — Angular's router state (extras.state) is lost on refresh.
 * — A simple service with signals is easy to understand and test.
 *
 * Note: this is in-memory state. A refresh clears it — which is fine
 * for this challenge. In production you'd serialize to sessionStorage.
 */
@Injectable({ providedIn: 'root' })
export class BookingStateService {

  /** The flight the user selected to book. Null if none selected yet. */
  readonly selectedOffer = signal<FlightOffer | null>(null);

  /** How many passengers were searched (needed by the booking form). */
  readonly passengerCount = signal<number>(1);

  /** The confirmed booking returned by the API. Null until booking is complete. */
  readonly confirmedBooking = signal<BookingResponse | null>(null);

  setSelectedOffer(offer: FlightOffer, passengers: number): void {
    this.selectedOffer.set(offer);
    this.passengerCount.set(passengers);
  }

  setConfirmedBooking(booking: BookingResponse): void {
    this.confirmedBooking.set(booking);
  }

  clear(): void {
    this.selectedOffer.set(null);
    this.confirmedBooking.set(null);
    this.passengerCount.set(1);
  }
}
