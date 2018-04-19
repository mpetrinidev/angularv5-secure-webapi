import { Injectable } from "@angular/core";
import { AppUserAuth } from "./app-user-auth";
import { AppUser } from "./app-user";
import { Observable } from "rxjs/Observable";
import { of } from "rxjs/observable/of";
import { tap } from "rxjs/operators/tap";
import { HttpHeaders, HttpClient } from "@angular/common/http";

const API_URL = "http://localhost:5000/api/security/";
const httpOptions = {
  headers: new HttpHeaders({
    "Content-Type": "application/json"
  })
};

@Injectable()
export class SecurityService {
  securityObject: AppUserAuth = new AppUserAuth();

  constructor(private http: HttpClient) {}

  logout(): void {
    this.resetSecurityObject();
  }

  resetSecurityObject(): void {
    this.securityObject.userName = "";
    this.securityObject.bearerToken = "";
    this.securityObject.isAuthenticated = false;

    this.securityObject.claims = [];

    localStorage.removeItem("bearerToken");
  }

  login(entity: AppUser): Observable<AppUserAuth> {
    this.resetSecurityObject();

    return this.http
      .post<AppUserAuth>(API_URL + "login", entity, httpOptions)
      .pipe(
        tap(resp => {
          Object.assign(this.securityObject, resp);

          localStorage.setItem("bearerToken", this.securityObject.bearerToken);
        })
      );
  }

  hasClaim(claimType: any, claimValue?: any) {
    let ret: boolean = false;

    if (typeof claimType === "string") {
      ret = this.isClaimValid(claimType, claimValue);
    } else {
      let claims: string[] = claimType;
      if (claims) {
        for (let index = 0; index < claims.length; index++) {
          ret = this.isClaimValid(claims[index]);
          //si uno es true, entonces que termine ok
          if (ret) break;
        }
      }
    }
    return ret;
  }

  private isClaimValid(claimType: string, claimValue?: string): boolean {
    let ret: boolean = false;
    let auth: AppUserAuth = null;

    auth = this.securityObject;
    if (auth) {
      // see if the claim type has a value
      // *hasClaim="'claimType:value'"
      if (claimType.indexOf(":") >= 0) {
        let words: string[] = claimType.split(":");
        claimType = words[0].toLowerCase();
        claimValue = words[1];
      } else {
        claimType = claimType.toLowerCase();
        //obtener el valor o asumir que es true
        claimValue = claimValue ? claimValue : "true";
      }

      ret =
        auth.claims.find(
          c =>
            c.claimType.toLowerCase() == claimType && c.claimValue == claimValue
        ) != null;
    }

    return ret;
  }
}
