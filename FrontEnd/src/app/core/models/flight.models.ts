/**
 * Core TypeScript models for SkyRoute.
 *
 * These interfaces mirror the backend DTOs exactly — same field names, same casing
 * (ASP.NET serialises PascalCase to camelCase by default, so we use camelCase here).
 *
 * If a backend DTO changes, update the corresponding interface here and TypeScript
 * will surface every broken reference at compile time.
 */

// ── Enums ─────────────────────────────────────────────────────────────────────

/** Must match SkyRoute.Domain.Models.CabinClass */
export type CabinClass = 'Economy' | 'Business' | 'First';

/** Must match SkyRoute.Domain.Models.DocumentType */
export type DocumentType = 'Passport' | 'NationalId';

/** Must match SkyRoute.Domain.Models.Currency */
export type Currency = 'USD';

// ── Search ────────────────────────────────────────────────────────────────────

/** Sent to POST /api/flights/search */
export interface FlightSearchRequest {
  origin: string;
  destination: string;
  /** ISO-8601 date string, e.g. "2025-06-15T00:00:00" */
  departureDate: string;
  passengers: number;
  cabinClass: CabinClass;
}

/** Returned by POST /api/flights/search — mirrors PricedFlightOffer */
export interface FlightOffer {
  flightOffer: {
    provider: string;
    flightNumber: string;
    origin: string;
    destination: string;
    departureTime: string; // ISO-8601 from the API
    arrivalTime: string;
    cabinClass: CabinClass;
    basePrice: number;
    duration: string;      // TimeSpan serialised as "HH:mm:ss"
  };
  totalPrice: number;
  pricePerPassenger: number;
  currency: Currency;
}

// ── Airport ───────────────────────────────────────────────────────────────────

/** Used in the search form dropdowns */
export interface Airport {
  code: string;   // IATA code, e.g. "JFK"
  name: string;   // Human-readable name
  country: string; // ISO-3166 alpha-2, e.g. "US"
}

// ── Booking ───────────────────────────────────────────────────────────────────

export interface BookingPassenger {
  firstName: string;
  lastName: string;
  email: string;
  dateOfBirth: string;     // ISO-8601
  documentType: DocumentType;
  documentNumber: string;
}

/** Sent to POST /api/bookings */
export interface BookingRequest {
  provider: string;
  flightNumber: string;
  departureTime: string;
  arrivalTime: string;
  origin: string;
  destination: string;
  cabinClass: CabinClass;
  totalPrice: number;
  currency: Currency;
  passengers: BookingPassenger[];
}

/** Returned by POST /api/bookings */
export interface BookingResponse {
  id: string;
  referenceCode: string;
  provider: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  cabinClass: CabinClass;
  totalPrice: number;
  currency: Currency;
  status: string;
}
