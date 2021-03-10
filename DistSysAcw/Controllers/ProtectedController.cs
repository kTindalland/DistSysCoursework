using DistSysAcw.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DistSysAcw.Controllers
{
    public class ProtectedController : BaseController
    {
        public ProtectedController(UserContext context) : base(context)
        {

        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string Hello()
        {
            var username = User.FindFirst(ClaimTypes.Name).Value;

            return $"Hello {username}";
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string SHA1([FromQuery]string message)
        {
            var hashServ = new SHA1CryptoServiceProvider();
            return HashBackend(hashServ, message);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string SHA256([FromQuery]string message)
        {
            var hashServ = new SHA256CryptoServiceProvider();
            return HashBackend(hashServ, message);
        }

        public string HashBackend(HashAlgorithm hashServ, string message)
        {
            // Check string exists
            if (string.IsNullOrEmpty(message))
            {
                Response.StatusCode = 400;
                return "Bad Request";
            }
            var messageBytes = Encoding.ASCII.GetBytes(message);

            hashServ.ComputeHash(messageBytes);

            var hashResult = BitConverter.ToString(hashServ.Hash).Replace("-", string.Empty);

            return hashResult;
        }
    }
}
