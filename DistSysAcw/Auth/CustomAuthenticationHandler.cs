using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using DistSysAcw.Models;

namespace DistSysAcw.Auth
{
    /// <summary>
    /// Authenticates clients by API Key
    /// </summary>
    public class CustomAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationHandler
    {
        private Models.UserContext DbContext { get; set; }

        public CustomAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            Models.UserContext dbContext)
            : base(options, logger, encoder, clock) 
        {
            DbContext = dbContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then create the correct Claims, add these to a ClaimsIdentity, create a ClaimsPrincipal from the identity 
            //        Then use the Principal to generate a new AuthenticationTicket to return a Success AuthenticateResult
            #endregion

            var apiKeyExists = Request.Headers.ContainsKey("ApiKey");

            if (!apiKeyExists)
            {
                return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
            }

            string apikey = Request.Headers["ApiKey"];

            var validKey = UserDatabaseAccess.CheckGuid(DbContext, apikey);

            if (!validKey)
            {
                return Task.FromResult(AuthenticateResult.Fail("Not Authenticated"));
            }

            // Get user and set claims
            User user = UserDatabaseAccess.GetUser(DbContext, apikey);

            


            var nameClaim = new Claim(ClaimTypes.Name, user.UserName);
            var roleclaim = new Claim(ClaimTypes.Role, user.Role);

            var iden = new ClaimsIdentity(new Claim[] { nameClaim, roleclaim }, "ApiKey");

            var principle = new ClaimsPrincipal(iden);

            var ticket = new AuthenticationTicket(principle, this.Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));


        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            byte[] messagebytes = Encoding.ASCII.GetBytes("Unauthorized. Check ApiKey in Header is correct.");
            Context.Response.StatusCode = 401;
            Context.Response.ContentType = "application/json";
            await Context.Response.Body.WriteAsync(messagebytes, 0, messagebytes.Length);
        }
    }
}