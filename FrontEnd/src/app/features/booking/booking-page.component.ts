import {
  Component, OnInit, inject, signal, ChangeDetectionStrategy
} from '@angular/core';
import { Router } from '@angular/router';
import {
  FormBuilder, FormGroup, FormArray, Validators,
  AbstractControl, ReactiveFormsModule
} from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';

import { FlightService }       from '../../core/services/flight.service';
import { BookingStateService } from '../../core/services/booking-state.service';
import { BookingPassenger, DocumentType } from '../../core/models/flight.models';
import { isInternationalRoute }  from '../../core/data/airports.data';
import { DurationPipe }          from '../../shared/pipes/duration.pipe';

@Component({
  selector: 'app-booking-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe, DurationPipe],
  templateUrl: './booking-page.component.html',
  styleUrl: './booking-page.component.css',
})
export class BookingPageComponent implements OnInit {

  private readonly fb            = inject(FormBuilder);
  private readonly flightService = inject(FlightService);
  private readonly bookingState  = inject(BookingStateService);
  private readonly router        = inject(Router);

  // ── Read state set by the search page ───────────────────────────────────────
  readonly offer         = this.bookingState.selectedOffer;
  readonly passengerCount = this.bookingState.passengerCount;

  // ── State ────────────────────────────────────────────────────────────────────
  readonly isSubmitting  = signal(false);
  readonly errorMessage  = signal<string | null>(null);

  /**
   * Whether the route is international — determines the document field label
   * and validation rules (Passport vs National ID).
   * Computed once in ngOnInit from the selected offer.
   */
  isInternational = false;
  documentLabel   = 'National ID';

  // ── Form ─────────────────────────────────────────────────────────────────────
  bookingForm!: FormGroup;

  /**
   * Shortcut to access the passengers FormArray.
   * Using a getter keeps the template clean: `passengersArray.controls`.
   */
  get passengersArray(): FormArray {
    return this.bookingForm.get('passengers') as FormArray;
  }

  ngOnInit(): void {
    const offer = this.offer();

    // Guard: if someone navigates directly to /booking without selecting a flight,
    // send them back to search.
    if (!offer) {
      this.router.navigate(['/']);
      return;
    }

    this.isInternational = isInternationalRoute(
      offer.flightOffer.origin,
      offer.flightOffer.destination
    );
    this.documentLabel = this.isInternational ? 'Passport Number' : 'National ID';

    // Build a FormGroup with one passenger group per seat
    this.bookingForm = this.fb.group({
      passengers: this.fb.array(
        Array.from({ length: this.passengerCount() }, () => this.buildPassengerGroup())
      )
    });
  }

  // ── Private form builder ─────────────────────────────────────────────────────

  private buildPassengerGroup(): FormGroup {
    const documentValidators = this.isInternational
      ? [Validators.required, Validators.minLength(6), Validators.maxLength(12)]
      : [Validators.required, Validators.minLength(5), Validators.pattern(/^\d+$/)];

    return this.fb.group({
      firstName:      ['', [Validators.required, Validators.minLength(2)]],
      lastName:       ['', [Validators.required, Validators.minLength(2)]],
      email:          ['', [Validators.required, Validators.email]],
      dateOfBirth:    ['', Validators.required],
      documentNumber: ['', documentValidators],
    });
  }

  // ── Actions ──────────────────────────────────────────────────────────────────

  onConfirm(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const offer    = this.offer()!;
    const formData = this.bookingForm.value;

    const documentType: DocumentType = this.isInternational ? 'Passport' : 'NationalId';

    const passengers: BookingPassenger[] = formData.passengers.map((p: any) => ({
      firstName:      p.firstName,
      lastName:       p.lastName,
      email:          p.email,
      dateOfBirth:    new Date(p.dateOfBirth).toISOString(),
      documentType,
      documentNumber: p.documentNumber,
    }));

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.flightService.createBooking({
      provider:       offer.flightOffer.provider,
      flightNumber:   offer.flightOffer.flightNumber,
      departureTime:  offer.flightOffer.departureTime,
      arrivalTime:    offer.flightOffer.arrivalTime,
      origin:         offer.flightOffer.origin,
      destination:    offer.flightOffer.destination,
      cabinClass:     offer.flightOffer.cabinClass,
      totalPrice:     offer.totalPrice,
      currency:       offer.currency,
      passengers,
    }).subscribe({
      next: (booking) => {
        this.bookingState.setConfirmedBooking(booking);
        this.router.navigate(['/confirmation']);
      },
      error: (err) => {
        console.error('Booking failed', err);
        const apiError = err.error?.error ?? 'Booking failed. Please try again.';
        this.errorMessage.set(apiError);
        this.isSubmitting.set(false);
      },
    });
  }

  onBack(): void {
    this.router.navigate(['/']);
  }

  // ── Template helpers ─────────────────────────────────────────────────────────

  isFieldInvalid(group: AbstractControl, controlName: string): boolean {
    const ctrl = group.get(controlName);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  getFieldError(group: AbstractControl, controlName: string): string {
    const ctrl = group.get(controlName);
    if (!ctrl || !ctrl.errors || !ctrl.touched) return '';

    if (ctrl.errors['required'])   return 'This field is required.';
    if (ctrl.errors['email'])      return 'Enter a valid email address.';
    if (ctrl.errors['minlength'])  return `Minimum ${ctrl.errors['minlength'].requiredLength} characters.`;
    if (ctrl.errors['maxlength'])  return `Maximum ${ctrl.errors['maxlength'].requiredLength} characters.`;
    if (ctrl.errors['pattern'])    return 'Only digits are allowed.';
    return 'Invalid value.';
  }

  /** Max date for date-of-birth: today (you can't be born in the future) */
  readonly maxDob = new Date().toISOString().split('T')[0];
}
