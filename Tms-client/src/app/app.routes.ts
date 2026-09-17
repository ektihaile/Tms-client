import { Routes } from "@angular/router";
import { roleGuard } from "./guards/role-guard";

export const routes: Routes = [
  {
    path: "dashboard",
    loadComponent: () =>
      import("./features/student-dashboard/student-dashboard.component").then(
        (m) => m.StudentDashboardComponent
      ),
  },
  {
    path: "courses",
    loadComponent: () =>
      import("./features/student-dashboard/student-dashboard.component").then(
        (m) => m.StudentDashboardComponent
      ),
  },
  {
    path: "courses/:id",
    loadComponent: () =>
      import("./features/course-detail/course-detail.component").then(
        (m) => m.CourseDetailComponent
      ),
  },
  {
    path: "enroll",
    loadComponent: () =>
      import("./features/enrollment-form/enrollment-form.component").then(
        (m) => m.EnrollmentFormComponent
      ),
  },
  {
    path: 'grade-submission',
    loadComponent: () =>
      import('./features/grade-submission/grade-submission.component')
        .then(m => m.GradeSubmissionComponent)
  },
  // 👉 Exercise 6 የሚጠይቀው አዲስ የ Admin ራውት (በ Guard የተጠበቀ)
  {
    path: 'admin/courses',
    loadComponent: () =>
      import('./features/admin-course-list/admin-course-list.component')
        .then(m => m.AdminCourseListComponent),
    canActivate: [roleGuard('Admin')]
  },
  { path: "", redirectTo: "dashboard", pathMatch: "full" },
  { path: "**", redirectTo: "dashboard" },
];