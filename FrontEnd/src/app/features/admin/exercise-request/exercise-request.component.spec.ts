import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExerciseRequestComponent } from './exercise-request.component';

describe('ExerciseRequestComponent', () => {
  let component: ExerciseRequestComponent;
  let fixture: ComponentFixture<ExerciseRequestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExerciseRequestComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ExerciseRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
