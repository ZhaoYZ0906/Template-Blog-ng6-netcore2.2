import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OpenIdConnectService } from '../open-id-connect.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-signin-oidc',
  templateUrl: './signin-oidc.component.html',
  styleUrls: ['./signin-oidc.component.scss']
})
export class SigninOidcComponent implements OnInit {
  constructor(
    private openIdConnectService: OpenIdConnectService,
    private router: Router
  ) {}

  ngOnInit() {
    this.openIdConnectService.userLoaded$.subscribe(userLoaded => {
      //判断登陆是否成功
      if (userLoaded) {
        this.router.navigate(['./']);
      } else {
        if (!environment.production) {
          console.log('An error happened: user wasn\'t loaded.');
        }
      }
    });

    //登陆的回掉函数
    this.openIdConnectService.handleCallback();
  }
}
