import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VestingscheduleComponent } from './vestingschedule.component';

describe('VestingscheduleComponent', () => {
  let component: VestingscheduleComponent;
  let fixture: ComponentFixture<VestingscheduleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VestingscheduleComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(VestingscheduleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
