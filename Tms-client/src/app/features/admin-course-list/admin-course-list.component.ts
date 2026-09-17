import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { CourseService } from '../../services/course.service';

@Component({
  selector: 'app-admin-course-list',
  standalone: true,
  templateUrl: './admin-course-list.component.html',
  styleUrl: './admin-course-list.component.scss',
})
export class AdminCourseListComponent {
  private readonly courseService = inject(CourseService);

  readonly coursesResource = rxResource({
    stream: () => this.courseService.getAll(),
  });

  deleteCourse(id: number): void {
    this.courseService.delete(id).subscribe(() => {
      this.coursesResource.reload();
    });
  }
}
