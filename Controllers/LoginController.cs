using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
using MentorAPI.Models;
using MentorAPI.Options;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace MentorAPI.Controllers {
    [Route("api/[controller]")]
    public class LoginController : Controller {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _serializerSettings;

        public LoginController(IOptions<JwtIssuerOptions> jwtOptions, ILoggerFactory loggerFactory) {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);

            _logger = loggerFactory.CreateLogger<LoginController>();

            _serializerSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented
            };
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<JObject> Get([FromBody] Login applicationUser) {
            var identity = await GetClaimsIdentity(applicationUser);
            if (identity == null) {
                _logger.LogInformation($"Invalid username ({applicationUser.Username}) or password ({applicationUser.Password})");
                return  null;
            }

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, identity.Name),
        new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
        new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
        identity.FindFirst("MentorAPI")
      };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            // Serialize and return the response
            var response = new {
                access_token = encodedJwt,
                expires_in = (int)_jwtOptions.ValidFor.TotalSeconds,
                State = 1,
                expire_datetime = _jwtOptions.IssuedAt
            };

            var json = JsonConvert.SerializeObject(response, _serializerSettings);
          // remeber to change this in order to run it more optimally
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Email", applicationUser.Email);
            parameters.Add("Password", applicationUser.Password);
            Login u = new Login().SearchDocument(parameters)[0];
         ///////////////////////////////////////////////////////
            dynamic jsonObject = new JObject();
            HttpContext.Session.SetString("token", response.access_token);
            HttpContext.Session.SetString("type", u.AccountType);
            HttpContext.Session.SetString("username", u.Username);

            jsonObject.token = response.access_token;
            jsonObject.type = u.AccountType;
            jsonObject.username = u.Username;

            return jsonObject;
        }

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options) {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero) {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null) {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null) {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);


        private static Task<ClaimsIdentity> GetClaimsIdentity(Login user) {
            try {
                if (user.Email.Equals(string.Empty) || user.Password.Equals(string.Empty)) {
                    return null;
                } else {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("Email", user.Email);
                    parameters.Add("Password", user.Password);
                    Login u = new Login().SearchDocument(parameters)[0];
                    if (u !=null) {
                        return Task.FromResult(new ClaimsIdentity(new GenericIdentity(u.Username, "Token"),
                      new[]
                      {
            new Claim("MentorAPI", u.AccountType)
                      }));
                    }
                    // Credentials are invalid, or account doesn't exist
                    return Task.FromResult<ClaimsIdentity>(null);
                }
            }
            catch(Exception e) {
                return null;
            }

        }

        [HttpPost]
        [AllowAnonymous]
        public bool Post([FromBody] Login login) {
            if (login.Username.Equals(string.Empty) || login.Password.Equals(string.Empty)) {
                return false;
            } else {
                login.Username = login.Username.ToLower();
                login.Email = login.Email.ToLower();
                if (login.InsertIntoDocument()) {
                    return true;
                } else {
                    return false;
                }
            }
        }


        [HttpPost("checkUsername")]
        [AllowAnonymous]
        public object checkUsername([FromBody] JObject login) {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", login.Property("Username").Value.ToString().ToLower());
            List<Login> logins = new Login().SearchDocument(parameters);
            if(logins.Count > 0) {
                return true;
            } else {
                return null;
            }
        }

        [HttpPost("checkEmail")]
        [AllowAnonymous]
        public object checkEmail([FromBody] JObject login) {

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Email", login.Property("Email").Value.ToString().ToLower());
            List<Login> logins = new Login().SearchDocument(parameters);
            if (logins.Count > 0) {
                return true;
            } else {
                return null;
            }
        }

        [HttpPost("checkLoggedIn")]
        [AllowAnonymous]
        public object checkLoggedIn([FromBody] JObject usernameObject)
        {
            string username = usernameObject.Property("Username").Value.ToString().ToLower();
            string type = HttpContext.Session.GetString("type");
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", username);
            List<Login> logins = new Login().SearchDocument(parameters);
            if (logins.Count > 0)
            {
                if (logins[0].AccountType.Equals(type))
                {
                    return HttpContext.Session.GetString("type");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

           
        }
    }


    
}