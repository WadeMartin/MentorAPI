using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MentorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MentorAPI.Services;

namespace MentorAPI.Controllers {
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ReportController : Controller
    {

                       //remember that this must work with the email service
        [HttpPost]
        [AllowAnonymous]
        public bool Post([FromBody] JObject report)
        {
            Report r = JsonConvert.DeserializeObject<Report>(report.ToString());
            if (r.Description.Equals(string.Empty))
            {
                return false;
            }
            else
            {
                if (r.InsertIntoDocument()) {
                    new EmailService().PrepareEmail("Report", "Mentor's Lab Member Reported", new List<string>() { "wademartin909@gmail.com" }, new List<dynamic>() { r });
                    return true;
                } else {
                    return false;
                }
            }
        }


        [HttpPost("getAllReports")]
        [AllowAnonymous]
        public IEnumerable<Rating> getAllReports([FromBody] JObject forIndividual)
        {
            
            if (forIndividual.Equals(string.Empty))
            {
                return null;
            }
            else
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("For", forIndividual.Property("SearchInput").Value);
                return new Rating().SearchDocument(parameters);
            }
        }


    }
}