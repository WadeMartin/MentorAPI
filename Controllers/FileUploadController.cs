using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace MentorAPI.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class FileUploadController : Controller
    {
        [HttpPost]
        public  ContentResult Upload() {
            String fileName = "";
            
            try {
                var context = HttpContext.Request.Form;
                if (context.Files.Count > 0) {

                    long size = context.Files.Sum(f => f.Length);
                       
                    foreach (var formFile in context.Files) {
                        //fileName = Guid.NewGuid().ToString() + "."+ formFile.FileName.Split('.')[1];
                        //fileName = fileName.Replace("-", "");
                        //if (formFile.Length > 0) {
                        //    using (var stream = new FileStream(@"Files\" + fileName, FileMode.Create)) {
                        //         formFile.CopyTo(stream);
                        //    }
                        //}
                        using (var ms = new MemoryStream())
                        {
                            formFile.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            fileName = Convert.ToBase64String(fileBytes);
                            fileName = $"Data:image/png;base64," + fileName;
                            // act on the Base64 data
                        }
                    }
                    
                }
            } catch (Exception ex) {
                return null;
            }
            return Content(fileName); 
        }

        [HttpPost("UploadBulk")]
        [AllowAnonymous]
        public string[] UploadBulk() {
            List<string> fileNames = new List<string>();
            try {
                
                var context = HttpContext.Request.Form;
                if (context.Files.Count > 0) {

                    long size = context.Files.Sum(f => f.Length);

                    foreach (var formFile in context.Files) {
                        if (formFile.Length > 0) {
                            string fileName;
                            using (var ms = new MemoryStream())
                            {
                                formFile.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                fileName = Convert.ToBase64String(fileBytes);
                                fileName = $"Data:image/png;base64,"+fileName ;
                                // act on the Base64 data
                            }
                            fileNames.Add(fileName);
                        }
                    }
                }
            } catch (Exception ex) {
                return null;
            }
            return fileNames.ToArray();
        }

        [HttpPost("GetCVDocument")] // redo this to work with base 64
        [AllowAnonymous]
        public IActionResult GetCVDocument([FromBody] JObject filePath) {
            string fileName = filePath.Properties().ToArray()[0].Value.ToString();
            try {

                var split = fileName.Split("base64,");
                var bytes = Convert.FromBase64String(split[1]);
                Console.WriteLine(split[1]);

                return new FileContentResult(bytes, new
                    MediaTypeHeaderValue("application/octet")) {
                    FileDownloadName = "eg.png"
                };

            } catch (Exception ex) {
                return null;
            }
        }

    }
}