using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentorAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MentorAPI.Controllers {
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class UserController : Controller
    {
        [HttpPost("CreateNewUser")]
        [AllowAnonymous]
        public User CreateNewUser([FromBody] User user) {
            if (user == null) {
                return null;
            } else {
                if (user.InsertIntoDocument()) {
                    return user;
                } else {
                    return null;
                }
            }
        }

        [HttpPost("GetUserDetails")]
        [AllowAnonymous]
        public User getAllRatings([FromBody] JObject user) {
            if (user.Equals(string.Empty)) {
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(user.Properties().ToArray()[0].Name, user.Properties().ToArray()[0].Value);
                List<User> u =  new User().SearchDocument(parameters);
               if(u.Count > 0) {
                    return u.ToArray()[0];
                }
                else{
                    return null;
                }
            }
        }

        [HttpPost("GetUserById")]
        [AllowAnonymous]
        public User GetUserById([FromBody] JObject user) {
            if (user.Equals(string.Empty)) {
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(user.Properties().ToArray()[0].Name, user.Properties().ToArray()[0].Value);
                List<User> u = new User().SearchDocument(parameters);
                if (u.Count > 0) {
                    return u.ToArray()[0];
                } else {
                    return null;
                }
            }
        }

        [HttpPost("UpdateUserDetails")]
        [AllowAnonymous]
        public ContentResult updateUser([FromBody] JObject usesr) {
            var user = JsonConvert.DeserializeObject<User>(usesr.ToString());
            if (user.Equals(string.Empty)) {
                return null;
            } else {
                bool result = user.UpdateDocument(user);

                if (result) {
                    return Content("true");
                } else {
                    return Content("false");
                }
            }
        }

        [HttpPost("UpdateUserDescription")]
        [AllowAnonymous]
        public ContentResult updateUserDescription([FromBody] JObject usesr)
        {
            var user = JsonConvert.DeserializeObject<User>(usesr.ToString());
            if (user.Equals(string.Empty))
            {
                return null;
            }
            else
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("Username", user.Username);
                List<User> u = new User().SearchDocument(parameters);
                u[0].Description = user.Description;
                bool result = u[0].UpdateDocument(u[0]);

                if (result)
                {
                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }
        }

    }
}