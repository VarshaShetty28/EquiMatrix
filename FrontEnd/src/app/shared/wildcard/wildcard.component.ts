import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-wildcard',
  standalone: true,
  template: ''
})
export class WildcardComponent implements OnInit {
  
  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    const role = this.auth.getUserRole();
    
    if (role === 'Admin') {
      this.router.navigate(['/admin'], { replaceUrl: true });
    } else if (role === 'Employee') {
      this.router.navigate(['/employee'], { replaceUrl: true });
    } else {
      this.router.navigate(['/'], { replaceUrl: true });
    }
  }
}
