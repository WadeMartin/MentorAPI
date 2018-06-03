using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MentorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using MentorAPI.Models.Extensions;
using Newtonsoft.Json;
using MentorAPI.Services;

namespace MentorAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class UserStatsController : Controller
    {

        [HttpPost("GetUserStats")]
        [AllowAnonymous]
        public UserStats GetUserStats([FromBody] JObject searchParameters) {
           
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", searchParameters.Property("Username").Value);
            return new UserStats().SearchDocument(parameters)[0];
        
        }

        [HttpPost("UpdateUserStats")]

        public void UpdateCompanyStats([FromBody] JObject startup) {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "Username", startup.Property("Username").Value }
            };
            UserStats stat = new UserStats().SearchDocument(parameters)[0];
            if (!startup.Property("FollowingAdd").IsNullOrEmpty())
            {
                stat.Following.Add(startup.Property("FollowingAdd").Value.ToString());
            }
            if (!startup.Property("FollowingRemove").IsNullOrEmpty())
            {
                stat.Following.Remove(startup.Property("FollowingRemove").Value.ToString());
            }
            if (!startup.Property("AddMemberOff").IsNullOrEmpty())
            {
                stat.Following.Add(startup.Property("MemberOff").Value.ToString());
            }
            if (!startup.Property("RemoveMemberOff").IsNullOrEmpty())
            {
                stat.Following.Remove(startup.Property("RemoveMemberOff").Value.ToString());
            }
            stat.UpdateDocument(stat);

        }

     
    }
}