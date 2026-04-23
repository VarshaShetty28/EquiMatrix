import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// ✅ Import the components used in template
import { EmployeeNavBarComponent } from '../employee-nav-bar/employee-nav-bar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-emplayout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    EmployeeNavBarComponent,
    FooterComponent
  ],
  templateUrl: './emplayout.component.html',
  styleUrl: './emplayout.component.css'
})
export class EmplayoutComponent {}
