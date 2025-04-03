import { Component, Input, OnInit } from '@angular/core';
import { ExamService } from '../exam.service';

@Component({
  selector: 'app-question-content',
  imports: [],
  templateUrl: './question-content.component.html',
  styleUrl: './question-content.component.scss'
})

export class QuestionContentComponent implements OnInit {
  @Input() question: any;
  selectedAnswer: any;

  constructor(private examService: ExamService) { }

  ngOnInit() {
   // this.selectedAnswer = this.examService.getSavedAnswer(this.question.id);
  }

  saveAnswer(optionId: number) {
    this.selectedAnswer = optionId;
    //this.examService.saveAnswer(this.question.id, optionId);
  }
}
