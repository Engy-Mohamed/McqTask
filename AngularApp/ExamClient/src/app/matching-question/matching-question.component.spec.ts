import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MatchingQuestionComponent } from './matching-question.component';

describe('MatchingQuestionComponent', () => {
  let component: MatchingQuestionComponent;
  let fixture: ComponentFixture<MatchingQuestionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatchingQuestionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MatchingQuestionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
