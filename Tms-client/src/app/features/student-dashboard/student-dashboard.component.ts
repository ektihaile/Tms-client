import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [],
  template: `
    <div>
      <h1>Welcome, {{ studentName() }}</h1>
      <p>Credits Earned: {{ earnedCredits() }}</p>
      <p>Graduation Status: {{ graduationStatus() }}</p>
      <button (click)="registerForClass()">Register</button>
    </div>
  `
})
export class StudentDashboardComponent {
  studentName = signal('Liya');
  earnedCredits = signal(45);
  graduationStatus = signal('On Track');

  registerForClass() {
    console.log('Register clicked');
  }
}