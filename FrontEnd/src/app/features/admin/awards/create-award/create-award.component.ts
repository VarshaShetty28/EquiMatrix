import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-create-award',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ],
  templateUrl: './create-award.component.html'
})
export class CreateAwardComponent {

  grantTypes = signal(['ESOP', 'RSU']);

  awardForm = this.fb.group({
    employeeId: ['', Validators.required],
    grantType: ['', Validators.required],
    numberOfShares: [null, [Validators.required, Validators.min(1)]],
    strikePrice: [null, [Validators.required, Validators.min(0)]],
    vestingPeriod: [{ value: 5, disabled: true }],
    grantDate: ['', Validators.required]
  });

  constructor(private fb: FormBuilder) {}

  submit(): void {
    if (this.awardForm.invalid) {
      this.awardForm.markAllAsTouched();
      return;
    }

    console.log('Award Created:', this.awardForm.getRawValue());
  }
}