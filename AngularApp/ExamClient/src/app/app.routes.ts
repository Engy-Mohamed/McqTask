import { Routes } from '@angular/router';
import { TakeExamComponent } from './take-exam/take-exam.component';
import { AppComponent } from './app.component';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: AppComponent },
  { path: 'take-exam/:code', component: TakeExamComponent },
  { path: '**', redirectTo: 'home' } // Wildcard route for 404 handling
];

