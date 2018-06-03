using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MentorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using MentorAPI.Models.Extensions;

namespace MentorAPI.Controllers {
    [Produces("application/json")]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class RatingController : Controller
    {


        [HttpPost]
        [AllowAnonymous]
        public bool Post([FromBody] Rating rate)
        {
            try {
                if (rate.Rate.Equals(string.Empty) || rate.Comment.Equals(string.Empty) || rate.RaterUsername.Equals(string.Empty) || rate.For.Equals(string.Empty)) {
                    throw new Exception();
                } else {
                    rate.dateCreated = DateTime.Now;
                    if (rate.InsertIntoDocument()) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            catch(Exception e) {
                return false;
            }
        }


        [HttpPost("getAllRatings")]
        [AllowAnonymous]
        public IEnumerable<Rating> getAllRatings([FromBody] JObject forIndividual)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (forIndividual.Equals(string.Empty))
            {
                return null;
            }
            else
            {
                if (!forIndividual.Property("Rating").IsNullOrEmpty())
                {
                    StartUp s = new StartUpController().GetCompanyByOwner(forIndividual);
                    
                    parameters.Add("For", s.CompanyName);
                    Console.WriteLine(s.OwningUsername);
                    return new Rating().SearchDocument(parameters);
                }
                else
                {
                    parameters.Add("For", forIndividual.Property("SearchInput").Value);
                    return new Rating().SearchDocument(parameters);
                }
                
            }
        }



        [HttpPost("getAllRatingsByUsername")]
        [AllowAnonymous]
        public IEnumerable<Rating> getAllRatingsByUsername([FromBody] JObject forIndividual)
        {

            if (forIndividual.Equals(string.Empty))
            {
                return null;
            }
            else
            {
                if (!forIndividual.Property("Rating").IsNullOrEmpty())
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("For", forIndividual.Properties().ToArray()[0].Value);
                    return new Rating().SearchDocument(parameters);

                }
                else
                {
                    
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("RaterUsername", forIndividual.Properties().ToArray()[0].Value);
                    return new Rating().SearchDocument(parameters);
                }
            }
        }



    }
}