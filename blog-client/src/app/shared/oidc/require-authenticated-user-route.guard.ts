import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router
} from '@angular/router';
import { Observable } from 'rxjs';
import { OpenIdConnectService } from './open-id-connect.service';

@Injectable({
  providedIn: 'root'
})
export class RequireAuthenticatedUserRouteGuard implements CanActivate {
  constructor(
    private openIdConnectService: OpenIdConnectService,
    private router: Router
  ) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    //看看用户有没有登陆
    if (this.openIdConnectService.userAvailable) {
      return true;
    } else {
      // 返回false之前触发一下登陆页面
      this.openIdConnectService.triggerSignIn();
      return false;
    }
  }
}
