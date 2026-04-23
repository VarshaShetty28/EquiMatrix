import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmplayoutComponent } from './emplayout.component';

describe('EmplayoutComponent', () => {
  let component: EmplayoutComponent;
  let fixture: ComponentFixture<EmplayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmplayoutComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EmplayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
