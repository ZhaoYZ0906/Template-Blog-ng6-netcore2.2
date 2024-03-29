import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppComponent } from './app.component';
import { Routes, RouterModule } from '@angular/router';
import { RequireAuthenticatedUserRouteGuard } from './shared/oidc/require-authenticated-user-route.guard';
import { RedirectSilentRenewComponent } from './shared/oidc/redirect-silent-renew/redirect-silent-renew.component';
import { SigninOidcComponent } from './shared/oidc/signin-oidc/signin-oidc.component';
import { OpenIdConnectService } from './shared/oidc/open-id-connect.service';

const routes: Routes = [
  { path: 'blog', loadChildren: './blog/blog.module#BlogModule' },
  { path: 'signin-oidc', component: SigninOidcComponent },
  { path: 'redirect-silentrenew', component: RedirectSilentRenewComponent },
  { path: '**', redirectTo: 'blog' }
];

@NgModule({
  declarations: [
    AppComponent,
    RedirectSilentRenewComponent,
    SigninOidcComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes),
    HttpClientModule
  ],
  providers: [RequireAuthenticatedUserRouteGuard, OpenIdConnectService],
  bootstrap: [AppComponent]
})
export class AppModule {}
