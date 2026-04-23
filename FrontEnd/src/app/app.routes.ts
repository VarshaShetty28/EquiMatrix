import { Routes } from '@angular/router';
import { LayoutComponent } from './shared/layout/layout.component';
import { EmplayoutComponent } from './shared/emplayout/emplayout.component';
import { WildcardComponent } from './shared/wildcard/wildcard.component';
import { LoginComponent } from './features/auth/login/login.component';

// Admin
import { DashboardComponent } from './features/admin/dashboard/dashboard.component';
import { AwardsComponent } from './features/admin/awards/awards.component';
import { CreateAwardComponent } from './features/admin/awards/create-award/create-award.component';
import { EmployeeComponent } from './features/admin/employees/employees.component';
import { ExerciseRequestComponent } from './features/admin/exercise-request/exercise-request.component';
import { NotificationsComponent } from './features/admin/notifications/notifications.component';
import { ReportsComponent } from './features/admin/reports/reports.component';

// Employee
import { EmployeeDashboardComponent } from './features/employee/edashboard/edashboard.component';
import { TaxCalculatorComponent } from './features/employee/tax-calculator/tax-calculator.component';
import { VestingScheduleComponent } from './features/employee/vestingschedule/vestingschedule.component';
import { MyRequestsComponent } from './features/employee/myrequests/myrequests.component';
import { EmployeeNotificationsComponent } from './features/employee/enotifications/enotifications.component';

// Guards
import { adminGuard } from './core/guards/admin.guard';
import { employeeGuard } from './core/guards/employee.guard';
import { loginGuard } from './core/guards/login.guard';

export const routes: Routes = [

  { path: '', component: LoginComponent, canActivate: [loginGuard] },

  {
    path: 'admin',
    component: LayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', component: DashboardComponent },
      { path: 'awards', component: AwardsComponent },
      { path: 'awards/create', component: CreateAwardComponent },
      { path: 'employees', component: EmployeeComponent },
      { path: 'exercise', component: ExerciseRequestComponent },
      { path: 'notifications', component: NotificationsComponent },
      { path: 'reports', component: ReportsComponent }
    ]
  },

  {
    path: 'employee',
    component: EmplayoutComponent,
    canActivate: [employeeGuard],
    children: [
      { path: '', component: EmployeeDashboardComponent },
      { path: 'tax-calculator', component: TaxCalculatorComponent },
      { path: 'vesting', component: VestingScheduleComponent },
      { path: 'requests', component: MyRequestsComponent },
      { path: 'notifications', component: EmployeeNotificationsComponent }
    ]
  },

  // Wildcard route - must be last
  { path: '**', component: WildcardComponent }
];