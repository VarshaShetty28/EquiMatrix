import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateAwardComponent } from './create-award.component';

describe('CreateAwardComponent', () => {
  let component: CreateAwardComponent;
  let fixture: ComponentFixture<CreateAwardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateAwardComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CreateAwardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
