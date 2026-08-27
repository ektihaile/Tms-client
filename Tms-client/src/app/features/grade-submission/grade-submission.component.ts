

import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { exhaustMap, tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { GradeService, GradePayload } from '../../services/grade.service';

@Component({
  selector: 'tms-grade-submission',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './grade-submission.component.html',
  styleUrl: './grade-submission.component.scss'
})
export class GradeSubmissionComponent {
  private fb = inject(FormBuilder);
  private gradeService = inject(GradeService);

  isSubmitting = signal(false);
  submitSubject = new Subject<GradePayload>();

  gradeForm = this.fb.nonNullable.group({
    studentId: [1, [Validators.required]],
    courseId: [1, [Validators.required]],
    score: [85, [Validators.required, Validators.min(0), Validators.max(100)]]
  });

  constructor() {
    this.submitSubject.pipe(
      tap(() => this.isSubmitting.set(true)),
      exhaustMap(payload => 
        this.gradeService.postGrade(payload).pipe(
          catchError(() => of({ id: '', success: false }))
        )
      ),
      tap(() => this.isSubmitting.set(false))
    ).subscribe(response => {
      if (response.success) {
        console.log('Grade submitted successfully!', response.id);
      }
    });
  }

  onSubmit() {
    if (this.gradeForm.valid) {
      this.submitSubject.next(this.gradeForm.getRawValue());
    }
  }
}