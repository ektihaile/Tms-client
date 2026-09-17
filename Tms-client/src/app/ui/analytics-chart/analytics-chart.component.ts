
export class AnalyticsChart {}
import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'tms-analytics-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="padding: 20px; background: #eef2f7; border-radius: 8px;">
      <h3>Analytics Chart Loaded!</h3>
      <p>Total Enrollments tracked in Store: {{ data().length }}</p>
    </div>
  `,
  styleUrl: './analytics-chart.component.scss'
})
export class AnalyticsChartComponent {
  // 
  data = input<any[]>([]);
}