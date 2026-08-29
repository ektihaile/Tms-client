
export class InstructorDashboard {}
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnrollmentStore } from '../../store/enrollment.store';
import { AnalyticsChartComponent } from '../../ui/analytics-chart/analytics-chart.component';

@Component({
  selector: 'tms-instructor-dashboard',
  standalone: true,
  imports: [CommonModule, AnalyticsChartComponent],
  templateUrl: './instructor-dashboard.html',
  styleUrl: './instructor-dashboard.scss'
})
export class InstructorDashboardComponent {
  
  store = inject(EnrollmentStore);
}