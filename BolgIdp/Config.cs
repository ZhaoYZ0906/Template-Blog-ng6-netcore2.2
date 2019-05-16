
// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace BolgIdp
{
    public static class Config
    {
        //用户域包含用户信息
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
                
            };
        }

        //被保护资源
        public static IEnumerable<ApiResource> GetApis()
        {
            //将所有api划分为一个作用域，好像可以这么理解
            return new ApiResource[]
            {
                new ApiResource("restapi", "myrestapi",new List<string>{
                    "name",
                    "gender",
                    JwtClaimTypes.PreferredUserName,
                    JwtClaimTypes.Picture})
            };
        }

        //客户端配置
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {


                // MVC client using hybrid flow
                new Client
                {
                    ClientId = "mvcclient",
                    ClientName = "MVC 客户端",

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    //在授权端点（就是这个）授权之后将token和accesstoken带到这个uri
                    RedirectUris = { "https://localhost:7001/signin-oidc" },
                    //前端通过这个登出
                    FrontChannelLogoutUri = "https://localhost:7001/signout-oidc",
                    //登出之后重定向
                    PostLogoutRedirectUris = { "https://localhost:7001/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    //客户端可以请求的范围，必须带openid证明是个openid 类型的请求
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "restapi"
                    }
                },

                // angular客户端配置  SPA client using implicit flow
                new Client
                {
                    //和ng那个一致
                    ClientId = "blog-client",
                    ClientName = "Blog Client",
                    //ng地址
                    ClientUri = "http://localhost:4200",

                    //服务器类型
                    AllowedGrantTypes = GrantTypes.Implicit,
                    //控制是否通过浏览器为此客户端传输访问令牌（默认值为false）。当允许多个响应类型时，这可以防止访问令牌的意外泄漏。
                    AllowAccessTokensViaBrowser = true,
                    //认证的时候弹出同意确认框
                    RequireConsent=true,
                    //accsee有效期 秒
                    AccessTokenLifetime=180,

                    RedirectUris =
                    {
                        "http://localhost:4200/signin-oidc",
                        "http://localhost:4200/redirect-silentrenew"
                    },

                    //登出后跳转
                    PostLogoutRedirectUris = { "http://localhost:4200/" },
                    //允许浏览器客户端跨域的地址，所以idp也要配置跨域
                    AllowedCorsOrigins = { "http://localhost:4200" },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "restapi"
                    }
                }
            };
        }
    }
}