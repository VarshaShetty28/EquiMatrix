import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyrequestsComponent } from './myrequests.component';

describe('MyrequestsComponent', () => {
  let component: MyrequestsComponent;
  let fixture: ComponentFixture<MyrequestsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyrequestsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MyrequestsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
