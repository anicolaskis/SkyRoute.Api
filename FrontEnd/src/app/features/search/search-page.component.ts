import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';

import { FlightService } from '../../core/services/flight.service';
import { BookingStateService } from '../../core/services/booking-state.service';
import { FlightOffer, CabinClass } from '../../core/models/flight.models';
import { AIRPORTS } from '../../core/data/airports.data';
import { DurationPipe } from '../../shared/pipes/duration.pipe';

/** Sort options available in the results table */
type SortKey = 'price-asc' | 'price-desc' | 'duration' | 'departure';

@Component({
  selector: 'app-search-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe, DurationPipe],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.css',
})
export class SearchPageComponent implements OnInit {

  private readonly fb             = inject(FormBuilder);
  private readonly flightService  = inject(FlightService);
  private readonly bookingState   = inject(BookingStateService);
  private readonly router         = inject(Router);

  // ── Data ────────────────────────────────────────────────────────────────────
  readonly airports = AIRPORTS;
  readonly cabinClasses: CabinClass[] = ['Economy', 'Business', 'First'];
  readonly passengerOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9];

  // ── State (signals) ─────────────────────────────────────────────────────────
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly results = signal<FlightOffer[]>([]);
  readonly sortKey = signal<SortKey>('price-asc');
  readonly hasSearched = signal(false);

  /**
   * Derived list: re-sorts every time results or sortKey changes.
   * No extra API call needed — sorting is purely frontend (per spec).
   */
  readonly sortedResults = computed(() => {
    const offers = [...this.results()];
    const key = this.sortKey();

    switch (key) {
      case 'price-asc':
        return offers.sort((a, b) => a.totalPrice - b.totalPrice);
      case 'price-desc':
        return offers.sort((a, b) => b.totalPrice - a.totalPrice);
      case 'duration':
        return offers.sort((a, b) =>
          a.flightOffer.duration.localeCompare(b.flightOffer.duration));
      case 'departure':
        return offers.sort((a, b) =>
          a.flightOffer.departureTime.localeCompare(b.flightOffer.departureTime));
    }
  });

  // ── Form ────────────────────────────────────────────────────────────────────
  searchForm!: FormGroup;

  /** Minimum date for the departure date picker (today) */
  readonly minDate = new Date().toISOString().split('T')[0];

  ngOnInit(): void {
    this.searchForm = this.fb.group({
      origin:        ['', Validators.required],
      destination:   ['', Validators.required],
      departureDate: ['', Validators.required],
      passengers:    [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass:    ['Economy' as CabinClass, Validators.required],
    });
  }

  // ── Actions ─────────────────────────────────────────────────────────────────

  onSearch(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }

    const { origin, destination, departureDate, passengers, cabinClass } = this.searchForm.value;

    if (origin === destination) {
      this.errorMessage.set('Origin and destination must be different airports.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.results.set([]);
    this.hasSearched.set(false);

    this.flightService.searchFlights({
      origin,
      destination,
      // Send as "YYYY-MM-DDT00:00:00" — no UTC conversion so the date
      // never shifts by a day due to the user's local timezone offset.
      departureDate: `${departureDate}T00:00:00`,
      passengers,
      cabinClass,
    }).subscribe({
      next: (offers) => {
        this.results.set(offers);
        this.hasSearched.set(true);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Search failed', err);
        this.errorMessage.set('Could not load flights. Please try again.');
        this.isLoading.set(false);
        this.hasSearched.set(true);
      },
    });
  }

  onSelectFlight(offer: FlightOffer): void {
    const passengers = this.searchForm.value.passengers as number;
    this.bookingState.setSelectedOffer(offer, passengers);
    this.router.navigate(['/booking']);
  }

  onSortChange(key: SortKey): void {
    this.sortKey.set(key);
  }

  // ── Template helpers ─────────────────────────────────────────────────────────

  /** Returns true when a form control has been touched and is invalid */
  isFieldInvalid(controlName: string): boolean {
    const ctrl = this.searchForm.get(controlName);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
