import { Component, Input, OnInit } from '@angular/core';


@Component({
  selector: 'app-exam-timer',
  imports: [],
  template: `<div class="alert alert-warning text-center">
              <strong>Time Left: {{ timeLeft }}</strong>
             </div>`,
  styleUrl: './exam-timer.component.scss'
})
export class ExamTimerComponent implements OnInit {

  @Input() examEndTime!: number;
  timeLeft: string = '';

  ngOnInit() {
    setInterval(() => {
      const now = new Date().getTime();
      const timeLeftMs = this.examEndTime - now;

      if (timeLeftMs > 0) {
        const minutes = Math.floor(timeLeftMs / (1000 * 60));
        const seconds = Math.floor((timeLeftMs % (1000 * 60)) / 1000);
        this.timeLeft = `${minutes}m ${seconds}s`;
      } else {
        this.timeLeft = 'Time is up!';
      }
    }, 1000);
  }
}
