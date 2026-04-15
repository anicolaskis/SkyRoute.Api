import { Airport } from '../models/flight.models';

/**
 * Static airport list — matches the AirportRegistry in the backend exactly.
 * This is the single source of truth for the dropdown options.
 * If a new airport is added to the backend's AirportRegistry, add it here too.
 */
export const AIRPORTS: Airport[] = [
  // United States
  { code: 'JFK', name: 'New York JFK',        country: 'US' },
  { code: 'LAX', name: 'Los Angeles LAX',      country: 'US' },
  { code: 'ORD', name: 'Chicago O\'Hare',       country: 'US' },
  { code: 'MIA', name: 'Miami MIA',            country: 'US' },
  // Argentina
  { code: 'EZE', name: 'Buenos Aires EZE',     country: 'AR' },
  { code: 'AEP', name: 'Buenos Aires AEP',     country: 'AR' },
  { code: 'COR', name: 'Córdoba COR',          country: 'AR' },
  // United Kingdom
  { code: 'LHR', name: 'London Heathrow',      country: 'GB' },
  { code: 'LGW', name: 'London Gatwick',       country: 'GB' },
  // Spain
  { code: 'MAD', name: 'Madrid MAD',           country: 'ES' },
  { code: 'BCN', name: 'Barcelona BCN',        country: 'ES' },
  // Brazil
  { code: 'GRU', name: 'São Paulo GRU',        country: 'BR' },
  { code: 'GIG', name: 'Rio de Janeiro GIG',   country: 'BR' },
  // Mexico
  { code: 'MEX', name: 'Mexico City MEX',      country: 'MX' },
  { code: 'CUN', name: 'Cancún CUN',           country: 'MX' },
];

/**
 * Returns true when origin and destination are in different countries.
 * Used to decide whether to show "Passport" or "National ID" in the booking form.
 */
export function isInternationalRoute(originCode: string, destinationCode: string): boolean {
  const origin = AIRPORTS.find(a => a.code === originCode);
  const dest   = AIRPORTS.find(a => a.code === destinationCode);

  // If either airport is unknown, treat conservatively as international.
  if (!origin || !dest) return true;

  return origin.country !== dest.country;
}
