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
        private RSACryptoServiceProvider _cryptoService;
        public ProtectedController(UserContext context, RSACryptoServiceProvider cryptoService) : base(context)
        {
            _cryptoService = cryptoService;
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string Hello()
        {
            var username = User.FindFirst(ClaimTypes.Name).Value;

            UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Asked for a hello.");

            return $"Hello {username}";
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string SHA1([FromQuery]string message)
        {
            var hashServ = new SHA1CryptoServiceProvider();

            UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Asked for a SHA1 hash.");

            return HashBackend(hashServ, message);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public string SHA256([FromQuery]string message)
        {
            var hashServ = new SHA256CryptoServiceProvider();

            UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Asked for a SHA256 hash.");

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

        [Authorize(Roles = "Admin, User")] // Checks valid ApiKey
        [HttpGet]
        public IActionResult GetPublicKey()
        {
            UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Asked for the public key.");

            return new ContentResult()
            {
                Content = _cryptoService.ToXmlString(false),
                StatusCode = 200,
                ContentType = "application/xml"
            };
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public IActionResult Sign([FromQuery]string message)
        {

            UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Asked for a signature.");

            var messageBytes = Encoding.ASCII.GetBytes(message);

            var signedBytes = _cryptoService.SignData(messageBytes, new SHA1CryptoServiceProvider());

            var result = BitConverter.ToString(signedBytes);

            return new ContentResult()
            {
                Content = result,
                StatusCode = 200,
                ContentType = "text/plain"
            };
        }
    }
}
