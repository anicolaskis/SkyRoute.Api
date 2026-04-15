import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FlightSearchRequest, FlightOffer, BookingRequest, BookingResponse } from '../models/flight.models';

/**
 * FlightService — the only place in the app that talks to the backend.
 *
 * All HTTP calls go through this service. Components never use HttpClient directly.
 * This makes it easy to mock the service in tests and to change the API URL in one place.
 */
@Injectable({ providedIn: 'root' })
export class FlightService {

  // The backend base URL. In a real project this would come from environment.ts.
  private readonly apiUrl = 'http://localhost:5000/api';

  // Angular's new inject() function — no need for a constructor just to inject HttpClient.
  private readonly http = inject(HttpClient);

  /**
   * Searches for available flights across all providers.
   * Returns the full list; sorting is done on the frontend (per spec).
   */
  searchFlights(request: FlightSearchRequest): Observable<FlightOffer[]> {
    return this.http.post<FlightOffer[]>(`${this.apiUrl}/flights/search`, request);
  }

  /**
   * Creates a confirmed booking for the selected flight + passengers.
   * Returns the booking confirmation including the reference code.
   */
  createBooking(request: BookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.apiUrl}/bookings`, request);
  }
}
