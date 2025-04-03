import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common'; // ✅ Import CommonModule
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';


@Component({
  selector: 'app-matching-question',
  templateUrl: './matching-question.component.html',
  styleUrl: './matching-question.component.scss',

  standalone: true, // ✅ Ensure it is standalone

  imports: [CommonModule, DragDropModule], // ✅ Add CommonModule for *ngFor
})

export class MatchingQuestionComponent {
  @Input() question: any;
  leftSide: any[] = [];
  rightSide: any[] = [];

  ngOnInit() {
    this.leftSide = [...this.question.leftItems];
    this.rightSide = [...this.question.rightItems];
  }

  drop(event: CdkDragDrop<any[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
  }
}
