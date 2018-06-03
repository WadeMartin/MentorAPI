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
    public class StartUpController : Controller
    {
        [HttpPost("Search")]
        [AllowAnonymous]
        public IEnumerable<StartUp> getAllFieldStartups([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {       
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                JToken token = searchParameters["SearchInput"];
                if (!token.IsNullOrEmpty()) {
                    if (searchParameters.Properties().Count() <= 1) {
                        parameters.Add("CompanyName", searchParameters.Property("SearchInput").Value);
                    } else {
                        parameters.Add(searchParameters.Property("SearchLookup").Value.ToString(), searchParameters.Property("SearchInput").Value);
                    }
                   
                }     
                List<StartUp> startups = new StartUp().SearchDocument(parameters);
                foreach (var item in startups)
                {
                    if (item.LogoLoc != null)
                    {
                        item.LogoLoc = new StringCompressor().UnzipStr(item.LogoLoc);
                    }
                }
                return startups;
            }
        }

        [HttpPost("SearchForAll")]
        [AllowAnonymous]
        public IEnumerable<StartUp> getAllFieldAndNameStartups([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return new StartUp().SearchDocument(new Dictionary<string, object>()).FindAll(com => com.CompanyName != string.Empty && com.CompanyName != null);
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                if (searchParameters.Property("SearchInput").Value.ToString() != "n/a") {
                    parameters.Add("CompanyName", searchParameters.Property("SearchInput").Value);
                }
              
                List<StartUp> returnList = new StartUp().SearchDocument(parameters).FindAll(com => com.CompanyName != string.Empty && com.CompanyName != null);
                parameters.Clear();
                parameters.Add("Expertises", searchParameters.Property("SearchInput").Value);
                return returnList.Union( new StartUp().SearchDocument(parameters).FindAll(com => com.CompanyName != string.Empty && com.CompanyName != null));
            }
        }

        [HttpPost("GetCompanyByOwner")]
        [AllowAnonymous]
        public StartUp GetCompanyByOwner([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("OwningUsername", searchParameters.Property("SearchInput").Value);
                List<StartUp> startups = new StartUp().SearchDocument(parameters);
                foreach (var item in startups)
                {
                    if (item.LogoLoc != null)
                    {
                        item.LogoLoc = new StringCompressor().UnzipStr(item.LogoLoc);
                    }
                    
                }
                return startups[0];
            }
        }

        [HttpPost("RemoveMemberFromStartup")]
        [AllowAnonymous]
        public StartUp RemoveMemberFromStartup([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return null;
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("OwningUsername", searchParameters.Property("Username").Value);
                StartUp s = new StartUp().SearchDocument(parameters)[0];
                s.Members.Remove(searchParameters.Property("MemberName").Value.ToString());
                s.UpdateDocument(s);
                return s;
            }
        }

        [HttpPost("SearchForCompanyByUsername")]
        [AllowAnonymous]
        public IEnumerable<StartUp> SearchForCompanyByUsername([FromBody] JObject searchParameters) {
            if (searchParameters.Count == 0) {
                return new StartUp().SearchDocument(new Dictionary<string, object>());
            } else {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("OwningUsername", searchParameters.Property("SearchInput").Value);
                 List<StartUp> startups = new StartUp().SearchDocument(parameters);
                foreach (var item in startups)
                {
                    if (item.LogoLoc != null)
                    {
                        item.LogoLoc = new StringCompressor().UnzipStr(item.LogoLoc);
                    }
                }
                return startups;
            }
        }

        [HttpPost("CreateNewOrUpdateStartUp")]

        public StartUp CreateNewStartUp([FromBody] JObject startup) {
            var heads = Request.Headers["Authorization"];
            StartUp s = JsonConvert.DeserializeObject<StartUp>(startup.ToString()); 
            if (startup == null) {
                return null;
            } else {
                Console.WriteLine(s);
                List<string> newPhotoList = new List<string>();
                foreach (var item in s.Photos)
                {
                    if(item[0] == ',')
                    {
                        newPhotoList.Add (item.Substring(1, item.Length - 1));
                    }
                    else
                    {
                        newPhotoList.Add(item);
                    }
                }
                s.Photos = newPhotoList;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("OwningUsername", s.OwningUsername);
                if(s.LogoLoc !=null && s.LogoLoc.equals(string.Empty))
                {
                    s.LogoLoc = new StringCompressor().ZipStr(s.LogoLoc);
                }
                List<StartUp> startUps = new StartUp().SearchDocument(parameters).ToList();
                if (startUps.Count > 0) {
                    if (s.CompanyName != null && !s.CompanyName.Equals(string.Empty))
                    {
                        if (s.CompanyName != startUps[0].CompanyName)
                        {
                            UpdateAllRecords(startUps[0], s);
                        }
                        startUps[0].UpdateDocument(s);
                    }
                    return startUps[0];
                } else {
                    if (s.InsertIntoDocument()) {
                        return s;
                    } else {
                        return null;
                    }
                }
               
            }
        }

        private void UpdateAllRecords(StartUp oldStartup, StartUp newStartup)
        {

            //UpdateStats(oldStartup,newStartup);
            UpdateMembersRequest(oldStartup, newStartup);
            UpdateRatings(oldStartup, newStartup);
            UpdateReports(oldStartup, newStartup);
            
        }

        //private void UpdateStats(StartUp oldStartup, StartUp newStartup)
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("CompanyName", oldStartup.CompanyName);
        //    List<StartUpStats> stats = new StartUpStats().SearchDocument(parameters);
        //    if(stats.Count > 0)
        //    {
        //        StartUpStats workingStat = stats[0];
        //        workingStat.CompanyName = newStartup.CompanyName;
        //        workingStat.UpdateDocument(workingStat);
        //    }
        //    else
        //    {
        //        new StartUpStats() { CompanyName = newStartup.CompanyName}.InsertIntoDocument();
        //    }
        //}

        private void UpdateMembersRequest(StartUp oldStartup, StartUp newStartup)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("StartupName", oldStartup.CompanyName);
            List<MemberRequest> stats = new MemberRequest().SearchDocument(parameters);
            if (stats.Count > 0)
            {
                foreach (var item in stats)
                {
                    item.StartupName = newStartup.CompanyName;
                    item.UpdateDocument(item);
                }
                
            }
        }

        private void UpdateRatings(StartUp oldStartup, StartUp newStartup)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("For", oldStartup.CompanyName);
            List<Rating> stats = new Rating().SearchDocument(parameters);
            if (stats.Count > 0)
            {
                foreach (var item in stats)
                {
                    item.For = newStartup.CompanyName;
                    item.UpdateDocument(item);
                }
            }
        }

        private void UpdateReports(StartUp oldStartup, StartUp newStartup)
        {
            string[] cols = new string[] { "Reporter", "Reported" };
            foreach (var col in cols)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(col, oldStartup.CompanyName);
                List<Report> stats = new Report().SearchDocument(parameters);
                if (stats.Count > 0)
                {
                    foreach (var item in stats)
                    {
                        if(col == "Reporter")
                        {
                            item.Reporter = newStartup.CompanyName;
                            item.UpdateDocument(item);
                        }
                        else
                        {
                            item.Reported = newStartup.CompanyName;
                            item.UpdateDocument(item);
                        } 
                    }
                }
            }
        }
    }
}