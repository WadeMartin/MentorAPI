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

namespace MentorAPI.Controllers {
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MemberRequestController : Controller {
        [HttpPost("GetAllForStartup")]
        [AllowAnonymous]
        public IEnumerable<MemberRequest> getAllRequestForStartup([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object> {
                    { "OwningUsername", searchParameters.Property("SearchInput").Value }
                };
                StartUp s = new StartUp().SearchDocument(parameters)[0];
                parameters.Clear();
                parameters.Add("StartupName", s.CompanyName);
                return new MemberRequest().SearchDocument(parameters);
            }
        }

        [HttpPost("SearchForAll")]
        [AllowAnonymous]
        public IEnumerable<StartUp> getAllFieldAndNameStartups([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return new StartUp().SearchDocument(new Dictionary<string, object>());
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("CompanyName", searchParameters.Property("SearchInput").Value);
                List<StartUp> returnList = new StartUp().SearchDocument(parameters);
                parameters.Clear();
                parameters.Add("Field", searchParameters.Property("SearchInput").Value);
                return returnList.Union(new StartUp().SearchDocument(parameters));
            }
        }

        [HttpPost("CreateNewRequest")]
        [AllowAnonymous]
        public MemberRequest CreateNewRequest([FromBody] JObject request) {
            var heads = Request.Headers["Authorization"];
            MemberRequest s = JsonConvert.DeserializeObject<MemberRequest>(request.ToString());
            if (request == null) {
                return null;
            } else {
                s.InsertIntoDocument();
                //Dictionary<string, object> parameters = new Dictionary<string, object>();
                //parameters.Add("Username", s.Username);
                //Login e = new Login().SearchDocument(parameters)[0];
                //User u = new User().SearchDocument(parameters)[0];
                //new EmailService().PrepareEmail("MentorApplication", "Mentor's Lab - You have a new Mentor Application", new List<string>() { e.Email }, new List<dynamic>() { u,s });
                return s;

            }
        }

        [HttpPost("ApproveRequest")]
        [AllowAnonymous]
        public bool ApproveRequest([FromBody] JObject request) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("OwningUsername", request.Property("Username").Value);
            StartUp s = new StartUp().SearchDocument(parameters)[0];
            var res = request.Property("Response").Value.ToString();
            Console.WriteLine(res);
            if (res == "True") {
                if(s.Members == null)
                {
                    s.Members = new List<string>() { request.Property("RequestingUsername").Value.ToString() };
                }
                else
                {
                    s.Members.Add(request.Property("RequestingUsername").Value.ToString());
                }
               
                s.UpdateDocument(s);
            }
            parameters.Clear();
            parameters.Add("StartupName", s.CompanyName);
            parameters.Add("Username", request.Property("RequestingUsername").Value);
            return new MemberRequest().DeleteFromDocument(parameters);
        }



    }
}