import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, LoginResponse } from '../../../core/services/auth.service';
import { FormsModule } from '@angular/forms';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './login.component.html'
})
export class LoginComponent {

  email: string = '';
  password: string = '';
  selectedRole: string = 'Employee';  // Important: capital E

  constructor(private auth: AuthService, private router: Router) {}

  onLogin() {
    this.auth.login(this.email, this.password, this.selectedRole)
      .subscribe({
        next: (res: LoginResponse) => {
          // Validate that the returned role matches the selected role
          if (res.role !== this.selectedRole) {
            alert(`You do not have ${this.selectedRole} privileges. Please select the correct role.`);
            return;
          }

          this.auth.saveToken(res.token);

          if (this.selectedRole === 'Admin') {
            this.router.navigate(['/admin'], { replaceUrl: true });
          } else if (this.selectedRole === 'Employee') {
            this.router.navigate(['/employee'], { replaceUrl: true });
          }
        },
        error: () => {
          alert("Invalid Credentials");
        }
      });
  }
}