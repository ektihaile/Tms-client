import { Component, signal, computed } from "@angular/core";

@Component({
  selector: "app-student-dashboard",
  standalone: true,
  templateUrl: "./student-dashboard.html", 
  styleUrl: "./student-dashboard.scss",       
})
export class StudentDashboardComponent {
  
  studentName = signal("Liya Kebede");
  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120 ? "Eligible for Graduation" : "In Progress"
  );

  registerForClass() {
    this.earnedCredits.update((c) => c + 3);
  }
}