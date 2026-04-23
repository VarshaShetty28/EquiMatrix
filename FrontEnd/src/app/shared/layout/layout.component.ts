import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminNavBarComponent } from '../admin-nav-bar/admin-nav-bar.component';
import { FooterComponent } from '../footer/footer.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, AdminNavBarComponent, FooterComponent],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {}