import { Pipe, PipeTransform } from '@angular/core';

/**
 * Converts a .NET TimeSpan string ("HH:mm:ss" or "d.HH:mm:ss") to a
 * human-readable format like "3h 25m".
 *
 * Usage in template:  {{ offer.flightOffer.duration | duration }}
 */
@Pipe({ name: 'duration', standalone: true })
export class DurationPipe implements PipeTransform {

  transform(value: string | null | undefined): string {
    if (!value) return '—';

    // .NET serialises TimeSpan as "HH:mm:ss" or "d.HH:mm:ss" for > 24 h
    const parts = value.split(':');
    if (parts.length < 2) return value;

    const hours   = parseInt(parts[0], 10);
    const minutes = parseInt(parts[1], 10);

    if (hours === 0) return `${minutes}m`;
    if (minutes === 0) return `${hours}h`;
    return `${hours}h ${minutes}m`;
  }
}
