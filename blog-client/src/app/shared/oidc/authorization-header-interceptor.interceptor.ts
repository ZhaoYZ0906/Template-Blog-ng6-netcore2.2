import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpHandler,
  HttpEvent,
  HttpRequest
} from '@angular/common/http';
import { OpenIdConnectService } from './open-id-connect.service';
import { Observable } from 'rxjs';

@Injectable()
export class AuthorizationHeaderInterceptor implements HttpInterceptor {
  constructor(private openIdConnectService: OpenIdConnectService) {}

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // add the access token as bearer token
    //这是一个拦截器，如果登陆了就克隆一个request并且组装返回
    if (this.openIdConnectService.userAvailable) {
      request = request.clone({
        setHeaders: {
          Authorization: `${this.openIdConnectService.user.token_type} ${
            this.openIdConnectService.user.access_token
          }`
        }
      });
    }
    return next.handle(request);
  }
}
