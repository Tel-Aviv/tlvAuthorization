# tlvAuthorization
[Authorize] attribute for C# Web API services that want to validate the JWTs issued by TLV OAuth system

## How to use

Add NuGet Package [System.IdentityModel.Tokens.Jwt Version 6.5.1](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/)

1. Include this file in your C# project and refine the dependencies
2. Replace [Authorize] to [TLVAuthorize] attributes in your controllers
3. Provide the endpoint to JWT validation in your web.config, i.g.
```xml
  <appSettings>
    <add key="jwtValidationEndpoint" value="http://10.111.51.14/oauth/api/tokeninfo"/>
  </appSettings>
```
