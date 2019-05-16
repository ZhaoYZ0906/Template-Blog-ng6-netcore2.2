// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  apiurlbase: '/api',
  openIdConnectSettings: {
    authority: 'https://localhost:5001/',
    client_id: 'blog-client',
    //成功之后跳转
    redirect_uri: 'http://localhost:4200/signin-oidc',
    //请求范围，要和idp端一致
    scope: 'openid profile email restapi',

    response_type: 'id_token token',
    //登出之后
    post_logout_redirect_uri: 'http://localhost:4200/',
    //是否可一重新获取token
    automaticSilentRenew: true,
    silent_redirect_uri: 'http://localhost:4200/redirect-silentrenew'
  }
};

/*
 * In development mode, for easier debugging, you can ignore zone related error
 * stack frames such as `zone.run`/`zoneDelegate.invokeTask` by importing the
 * below file. Don't forget to comment it out in production mode
 * because it will have a performance impact when errors are thrown
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
