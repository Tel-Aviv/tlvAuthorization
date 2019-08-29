using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Products.OAuthProvider
{
    public class TLVAuthorizeAttribute: AuthorizeAttribute
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
            if(authHeader == null)
            {
                HandleUnauthorizedRequest(actionContext);
            } else {
                string[] tokens = authHeader.Split(' ');
                if( tokens[0] == "Bearer" )
                {
                    string jwt = tokens[1];
                    if( !isValidRequest(jwt) )
                        HandleUnauthorizedRequest(actionContext);

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
            using (var httpClient = new HttpClient() )
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", jwt)
                });
                var response = httpClient.PostAsJsonAsync(validationEndpoint, content).Result;
                return response.IsSuccessStatusCode;
            }
        }
    }
}
