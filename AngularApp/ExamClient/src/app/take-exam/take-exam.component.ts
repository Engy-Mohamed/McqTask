import { Component, OnInit } from '@angular/core';
import { ExamService } from '../exam.service';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ExamTimerComponent } from '../exam-timer/exam-timer.component';
import { QuestionContentComponent } from '../question-content/question-content.component';
import { QuestionNavigationngComponent } from '../question-navigationng/question-navigationng.component';
import { MatchingQuestionComponent } from '../matching-question/matching-question.component';



@Component({
  selector: 'app-take-exam',
  standalone: true,
  templateUrl: './take-exam.component.html',
  styleUrls: ['./take-exam.component.scss'],
   imports: [CommonModule, ExamTimerComponent, QuestionContentComponent, QuestionNavigationngComponent, MatchingQuestionComponent], // Import components here
})
export class TakeExamComponent implements OnInit {
  questions: any[] = [];
  currentQuestionIndex = 0;
  totalQuestions = 0;
  examEndTime = new Date().getTime() + 15 * 60 * 1000; // 15 mins
  examCode: string | null = null; // Exam code from URL

  constructor(private route: ActivatedRoute, private examService: ExamService) { }

  ngOnInit() {
    this.examCode = this.route.snapshot.paramMap.get('code') ?? ''; // Default to empty string

    this.examService.takeExam(this.examCode).subscribe((data) => {
      this.questions = data;
      this.totalQuestions = this.questions.length;
    });

    this.updateTimer();
  }

  goToQuestion(index: number) {
    this.currentQuestionIndex = index;
  }

  nextQuestion() {
    if (this.currentQuestionIndex < this.totalQuestions - 1) {
      this.currentQuestionIndex++;
    }
  }

  prevQuestion() {
    if (this.currentQuestionIndex > 0) {
      this.currentQuestionIndex--;
    }
  }

  updateTimer() {
    setInterval(() => {
      const now = new Date().getTime();
      const timeLeft = this.examEndTime - now;
      if (timeLeft <= 0) {
        alert('Time is up! Submitting exam...');
      }
    }, 1000);
  }

  submitExam() {
    alert('Exam Submitted!');
  }
}
