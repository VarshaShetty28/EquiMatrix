import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnotificationsComponent } from './enotifications.component';

describe('EnotificationsComponent', () => {
  let component: EnotificationsComponent;
  let fixture: ComponentFixture<EnotificationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnotificationsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EnotificationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
