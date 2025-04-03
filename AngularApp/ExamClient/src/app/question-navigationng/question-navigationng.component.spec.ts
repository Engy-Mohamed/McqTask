import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuestionNavigationngComponent } from './question-navigationng.component';

describe('QuestionNavigationngComponent', () => {
  let component: QuestionNavigationngComponent;
  let fixture: ComponentFixture<QuestionNavigationngComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuestionNavigationngComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuestionNavigationngComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
