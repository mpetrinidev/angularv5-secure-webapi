import { Injectable } from '@angular/core';
import { AppUserAuth } from './app-user-auth';
import { AppUser } from './app-user';
import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { tap } from 'rxjs/operators/tap';
import { LOGIN_MOCKS } from './login-mocks';
import { HttpHeaders, HttpClient } from '@angular/common/http';

const API_URL = "http://localhost:5000/api/security/"
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json'
  })
};

@Injectable()
export class SecurityService {
  securityObject: AppUserAuth = new AppUserAuth();

  constructor(private http: HttpClient) { }

  logout(): void{
    this.resetSecurityObject();
  }

  resetSecurityObject(): void {
    this.securityObject.userName = "";
    this.securityObject.bearerToken = "";
    this.securityObject.isAuthenticated = false;

    this.securityObject.canAccessProducts = false;
    this.securityObject.canAddProduct = false;
    this.securityObject.canSaveProduct = false;
    this.securityObject.canAccessCategories = false;
    this.securityObject.canAddCategory = false;

    localStorage.removeItem("bearerToken");
  }

  login(entity: AppUser): Observable<AppUserAuth> {
    this.resetSecurityObject();

    return this.http.post<AppUserAuth>(API_URL + "login", 
                                      entity, 
                                      httpOptions).pipe(
                                        tap(
                                          resp => {
                                            Object.assign(this.securityObject, resp);

                                            localStorage.setItem("bearerToken", this.securityObject.bearerToken);
                                          }
                                        )
                                      );

    //use assign to update the current object.
    //don't create a new appuserauth object because that
    //destroy all references to object

    // Object.assign(this.securityObject, LOGIN_MOCKS.find(
    //   user => user.userName.toLowerCase() === entity.userName.toLowerCase()
    // ));

    // if(this.securityObject.userName !== "") {
    //   localStorage.setItem("bearerToken", this.securityObject.bearerToken);
    // }

    // return of<AppUserAuth>(this.securityObject);
  }
}