# tlvAuthorization
[Authorize] attribute for services that use corporate TLV OAuth

## How to use
1. Include this file in your C# project
2. Replace [Authorize] to [TLVAuthorize] attributes
3. Provide the endpoint to JWT validation in your web.config, i.e.
  <appSettings>
    <add key="jwtValidationEndpoint" value="http://10.111.51.14/oauth/api/tokeninfo"/>
  </appSettings>
