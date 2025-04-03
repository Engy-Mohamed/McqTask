import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
//import { CommonModule } from '@angular/common'; // ✅ Import CommonModule
//import { provideHttpClient } from '@angular/common/http'; // ✅ Provide HttpClient in standalone mode


@Injectable({
  providedIn: 'root',
  
})
export class ExamService {
    
  private baseUrl = 'http://localhost:5198/api/exam';
  private examUrl = ''; // Replace with your actual API URL

  constructor(private http: HttpClient) { }

  // Save exam progress
  saveProgress(progressData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/save-progress`, progressData);
  }

  // Start the exam
  takeExam(code: string): Observable<any> {
    this.examUrl = `${this.baseUrl}/take-exam/${code}`;
    console.log('📡 Fetching data from API:', this.examUrl); // ✅ Log request
    return this.http.get(this.examUrl).pipe(
      tap((data) => console.log('✅ Data received:', data)), // ✅ Log response
      catchError((error) => {
        console.error('❌ Error fetching data:', error); // ✅ Log error
        throw error; // Rethrow the error for further debugging
      })
    );
  }

  // Navigate to the next or previous question
  navigateQuestion(navigationData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/navigate-question`, navigationData);
  }

  // Submit the exam
  submitExam(studentId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/submit-exam`, { studentId });
  }
}
