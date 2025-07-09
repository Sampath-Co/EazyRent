import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { TOKEN_KEY } from '../constants';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) { }

  createUser(formData: any) {
    return this.http.post(environment.apiBaseUrl + '/User/register', formData);
  }

  login(formData: any) {
    return this.http.post(environment.apiBaseUrl + '/User/login', formData);
  }

  getToken() {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  deleteToken() {
    sessionStorage.removeItem(TOKEN_KEY);
  }

  getUserRoles(): string | null {
    const token = sessionStorage.getItem(TOKEN_KEY);
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      return role;
    } catch (e) {
      return null;
    }
  }

  getUsername(): string | null {
    const token = sessionStorage.getItem(TOKEN_KEY);
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || null;
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    return sessionStorage.getItem(TOKEN_KEY) != null ? true : false;
  }
}
