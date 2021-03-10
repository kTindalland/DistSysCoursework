using DistSysAcw.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DistSysAcw.Controllers
{
    public class UserController : BaseController
    {
        public UserController(UserContext dbcontext) : base(dbcontext)
        {
        }

        [ActionName("New")]
        [HttpGet]
        public IActionResult GetNew()
        {
            const string trueResp = "True - User Does Exist! Did you mean to do a POST to create a new user?";
            const string falseResp = "False - User Does Not Exist! Did you mean to do a POST to create a new user?";

            var response = new ContentResult()
            {
                StatusCode = 200
            };

            // Check for value in query string
            var valueExists = Request.Query.ContainsKey("username");

            if (!valueExists)
            {
                response.Content = falseResp;
                return response;
            }

            string username = Request.Query["username"];

            var userExists = UserDatabaseAccess.CheckUsername(DbContext, username);

            if (userExists)
            {
                response.Content = trueResp;
            }
            else
            {
                response.Content = falseResp;
            }

            return response;
        }

        [ActionName("New")]
        [HttpPost]
        public IActionResult PostNew()
        {
            // Get if it's in JSON
            var contentType = Request.Headers["Content-Type"][0];

            if (contentType != "application/json")
            {
                // Not JSON
                return new UnsupportedMediaTypeResult();
            }

            
            string username;
            using (var reader = new StreamReader(this.Request.Body))
            {
                var bodyTask = reader.ReadToEndAsync();

                bodyTask.Wait();
                var body = bodyTask.Result;

                if (body == "")
                {
                    return new ContentResult()
                    {
                        Content = "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json",
                        StatusCode = 400
                    };
                }

                try
                {
                    username = (string)JsonSerializer.Deserialize(body, typeof(string));
                }
                catch
                {
                    return new ContentResult()
                    {
                        Content = "Request Body was improperly formatted.",
                        StatusCode = 400
                    };
                }
            }

            // Check if username is taken already
            var usernameTaken = UserDatabaseAccess.CheckUsername(DbContext, username);

            if (usernameTaken)
            {
                return new ContentResult()
                {
                    Content = "Oops. This username is already in use. Please try again with a new username.",
                    StatusCode = 403,
                    ContentType = "text/plain"
                };
            }
            else
            {
                // Create new user and return guid
                var guid = UserDatabaseAccess.CreateUser(DbContext, username);

                return new ContentResult()
                {
                    Content = guid.ToString(),
                    StatusCode = 200,
                    ContentType = "text/plain"
                };
            }
        }

        [Authorize(Roles = "Admin, User")]
        [HttpDelete]
        public bool RemoveUser([FromQuery]string username, [FromHeader]string ApiKey)
        {
            Response.StatusCode = 200; // Set status code to OK 200

            // Check if ApiKey is in db
            if (!UserDatabaseAccess.CheckGuid(DbContext, ApiKey))
            {
                return false;
            }

            // Check ApiKey and username line up
            var apiKeyUser = UserDatabaseAccess.GetUser(DbContext, ApiKey);

            if (apiKeyUser.UserName != username)
            {
                return false;
            }

            // Delete user from db
            UserDatabaseAccess.DeleteUser(DbContext, ApiKey);

            return true;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ChangeRole()
        {
            // Get if it's in JSON
            var contentType = Request.Headers["Content-Type"][0];

            if (contentType != "application/json")
            {
                // Not JSON
                return new UnsupportedMediaTypeResult();
            }

            // Get body as string
            string body;
            using (var reader = new StreamReader(this.Request.Body))
            {
                var bodyTask = reader.ReadToEndAsync();

                bodyTask.Wait();
                body = bodyTask.Result;
            }

            // Get Json object
            var items = new Dictionary<string, string>();
            items = (Dictionary<string, string>)JsonSerializer.Deserialize(body, items.GetType());

            // Check keys
            if (items.ContainsKey("username") && items.ContainsKey("role"))
            {
                // Correct keys
                // Check if user exists
                if (!UserDatabaseAccess.CheckUsername(DbContext, items["username"]))
                {
                    // User doesn't exist
                    return new ContentResult()
                    {
                        Content = "NOT DONE: Username does not exist",
                        StatusCode = 400,
                        ContentType = "text/plain"
                    };
                }

                // Check if role exists
                if (!(items["role"] == "User" || items["role"] == "Admin"))
                {
                    // Role doesn't exist
                    return new ContentResult()
                    {
                        Content = "NOT DONE: Role does not exist",
                        StatusCode = 400,
                        ContentType = "text/plain"
                    };
                }

                // All checks done. Do work
                UserDatabaseAccess.ChangeRole(DbContext, items["username"], items["role"]);

                return new ContentResult()
                {
                    Content = "DONE",
                    StatusCode = 200,
                    ContentType = "text/plain"
                };
            }
            else // Incorrect keys
            {
                return new ContentResult()
                {
                    Content = "NOT DONE: An error occured",
                    StatusCode = 400,
                    ContentType = "text/plain"
                };
            }
        }
    }
}
