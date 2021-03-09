using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DistSysAcw.Controllers
{
    public class TalkbackController : BaseController
    {
        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkbackController(Models.UserContext dbcontext) : base(dbcontext) { }

        [ActionName("Hello")]
        [HttpGet]
        public string HelloWorld()
        {
            #region TASK1
            return "Hello world";
            #endregion
        }

        [ActionName("Sort")]
        [HttpGet]
        public IActionResult Sort()
        {
            var items = Request.Query["integers"].ToArray();

            var numbers = new List<int>();

            for (int i = 0; i < items.Length; i++)
            {
                bool valid = false;
                int result;

                valid = int.TryParse(items[i], out result);

                if (!valid)
                {
                    // Handle not valid
                    return new ContentResult()
                    {
                        Content = "Bad Request",
                        StatusCode = 400
                    };
                }
                else
                {
                    numbers.Add(result);
                }

                
            }

            var response = new JsonResult(numbers)
            {
                StatusCode = 200
            };

            return response;
        }
    }
}
