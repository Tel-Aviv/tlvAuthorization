/*
 * Written by Ronen Beniaminov
 * (c) 2020, Tel-Aviv Municipality
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace tagmulim.api
{
    class TlvIdentity : IIdentity
    {
        public string Name { get; set; }

        public bool IsAuthenticated { get; set; }

        public string AuthenticationType => "OAuth";
    }

    class TlvPrincipal : IPrincipal
    {
        private IEnumerable<Claim> Claims { get; set; }

        public TlvPrincipal(TlvIdentity identity, IEnumerable<Claim> claims)
        {
            this.Identity = identity;
            this.Claims = claims;
        }

        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            var isInRole = this.Claims.Where(x => x.Type == role)
                        .Select(x => x.Value)
                        .FirstOrDefault();
            bool result = false;
            Boolean.TryParse(isInRole, out result);
            return result;
        }
    }

    public class TLVAuthorizeAttribute : AuthorizeAttribute
    {
        public Uri validationEndpoint { get; private set; }

        public TLVAuthorizeAttribute()
        {
            validationEndpoint = new Uri(ConfigurationManager.AppSettings["jwtValidationEndpoint"]);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string authHeader = (from h in actionContext.Request.Headers
                                 where h.Key == "Authorization"
                                 select h.Value.First()).FirstOrDefault();

            if (authHeader == null)
            {
                HandleUnauthorizedRequest(actionContext);
            }
            else
            {

                string[] tokens = authHeader.Split(' ');
                if (tokens[0] == "Bearer")
                {
                    string jwt = tokens[1];
                    if (isValidRequest(jwt) == false) {
                        HandleUnauthorizedRequest(actionContext);
                    }

                    var controller = actionContext.ControllerContext.Controller as ApiController;
                    if( controller != null )
                    {
                        var jwtToken = new JwtSecurityToken(tokens[1]);
                        var claims = jwtToken.Claims;
                        
                        var name = claims.Where(x => x.Type == "nameid")
                                    .Select(x => x.Value)
                                    .FirstOrDefault();
                        
                        TlvIdentity identity = new TlvIdentity()
                        {
                            Name = name,
                            IsAuthenticated = true
                        };

                        controller.User = new TlvPrincipal(identity, claims);
                    }

                    return;
                }
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var challengeMessage = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            throw new HttpResponseException(challengeMessage);
        }

        private bool isValidRequest(string jwt)
        {
            return true;

            //using (var httpClient = new HttpClient())
            //{
            //    var content = new FormUrlEncodedContent(new[]
            //    {
            //        new KeyValuePair<string, string>("token", jwt)
            //    });
            //    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            //    var response = httpClient.PostAsJsonAsync(validationEndpoint, content).Result;
            //    return response.IsSuccessStatusCode;
            //}
        }
    }

}
