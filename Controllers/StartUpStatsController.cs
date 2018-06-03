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
    public class StartUpStatsController : Controller
    {

        [HttpPost("GetCompanyStats")]
        [AllowAnonymous]
        public StartUpStats GetCompanyStats([FromBody] JObject searchParameters) {
           
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Username", searchParameters.Property("Username").Value);
            return new StartUpStats().SearchDocument(parameters)[0];
        
        }

        [HttpPost("UpdateCompanyStats")]

        public void UpdateCompanyStats([FromBody] JObject startup) {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "Username", startup.Property("Username").Value }
            };
            StartUpStats stat = new StartUpStats().SearchDocument(parameters)[0];
            if(!startup.Property("View").IsNullOrEmpty())
            {
                JObject view = (JObject)startup.Property("View").Value;
                if (stat.Views == null)
                {
                    
                        Dictionary<string, int> statDic = new Dictionary<string, int>();
                        statDic.Add(view.Property("ViewKey").Value.ToString(),1); // fix this part
                        stat.Views = statDic;

                }
                else
                {
                    if (stat.Views.Keys.Contains(view.Property("ViewKey").Value.ToString()))
                    {
                        int val = stat.Views[view.Property("ViewKey").Value.ToString()];
                        stat.Views[view.Property("ViewKey").Value.ToString()] = val++;
                        foreach (var item in stat.Views)
                        {
                            if (item.Key.ToString().Equals(view.Property("ViewKey").Value.ToString()))
                            {
                                stat.Views[item.Key]++;
                                break;
                            }
                        }
                    }
                    else
                    {
                        stat.Views.Add(view.Property("ViewKey").Value.ToString(), 1);
                    }
                    
                }
            }
            if (!startup.Property("FollowerAdd").IsNullOrEmpty())
            {
                stat.Followers.Add(startup.Property("FollowerAdd").Value.ToString());
            }
            if (!startup.Property("MembersRejectedIncrease").IsNullOrEmpty())
            {
                stat.AmountOfMembersRejected = stat.AmountOfMembersRejected++;
            }
            if (!startup.Property("MembersApprovedIncrease").IsNullOrEmpty())
            {
                stat.AmountOfMembersApproved = stat.AmountOfMembersApproved++;
            }
            if (!startup.Property("AmountOfMembersIncrease").IsNullOrEmpty())
            {
                stat.AmountOfMembers = stat.AmountOfMembers++;
            }
            
            if (!startup.Property("SocialMedia").IsNullOrEmpty())
            {
                JObject view = (JObject)startup.Property("SocialMedia").Value;
                if (stat.SocialMediaClicks == null)
                {

                    Dictionary<string, Dictionary<string,int>> statDic = new Dictionary<string, Dictionary<string, int>>();
                    statDic.Add(view.Property("SocialMonth").Value.ToString(), new Dictionary<string, int>() { { view.Property("SocialKey").Value.ToString(), 1} }); // fix this part
                    stat.SocialMediaClicks = statDic;

                }
                else
                {
                    if (stat.SocialMediaClicks.Keys.Contains(view.Property("SocialMonth").Value.ToString()))
                    {
                        var val = stat.SocialMediaClicks[view.Property("SocialMonth").Value.ToString()];
                        
                        if (stat.SocialMediaClicks[view.Property("SocialMonth").Value.ToString()].ContainsKey(view.Property("SocialKey").Value.ToString()))
                        {
                            stat.SocialMediaClicks[view.Property("SocialMonth").Value.ToString()][view.Property("SocialKey").Value.ToString()] = val[view.Property("SocialKey").Value.ToString()]++;
                        }
                        else
                        {
                            stat.SocialMediaClicks[view.Property("SocialMonth").Value.ToString()].Add(view.Property("SocialKey").Value.ToString(), 1);
                        }
                        
                        //foreach (var item in stat.SocialMediaClicks)
                        //{
                        //    if (item.Key.ToString().Equals(view.Property("SocialMonth").Value.ToString()))
                        //    {
                        //        foreach (var soc in item.Value)
                        //        {
                        //            if (soc.Key.ToString().Equals(view.Property("SocialKey").Value.ToString()))
                        //            {
                        //                stat.SocialMediaClicks[item.Key][soc.Key]++;
                        //                break;
                        //            }
                                        
                        //        }
                               
                        //        break;
                        //    }
                        //}
                    }
                    else
                    {
                        stat.SocialMediaClicks.Add(view.Property("SocialMonth").Value.ToString(), new Dictionary<string, int>() { { view.Property("SocialKey").Value.ToString(), 1 } });
                    }

                }
            }

            stat.UpdateDocument(stat);

        }

     
    }
}