import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'tms-grade-submission',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './grade-submission.component.html',
  styleUrl: './grade-submission.component.scss'
})
export class GradeSubmissionComponent {
  gradeForm: FormGroup;
  isSubmitting = signal<boolean>(false);
  submissionStatus = signal<string | null>(null);

  constructor(private fb: FormBuilder) {
    this.gradeForm = this.fb.group({
      studentId: [null, [Validators.required]],
      courseId: [null, [Validators.required]],
      score: [null, [Validators.required, Validators.min(0), Validators.max(100)]]
    });
  }

  onSubmit(): void {
    if (this.gradeForm.valid) {
      this.isSubmitting.set(true);
      this.submissionStatus.set(null);

      
      setTimeout(() => {
        this.isSubmitting.set(false);
        this.submissionStatus.set('Grade submitted successfully!');
        this.gradeForm.reset();
      }, 1500);
    }
  }
}