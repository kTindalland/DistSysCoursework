using DistSysAcw.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.IO;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddFifty([FromQuery]string encryptedInteger, [FromQuery]string encryptedSymKey, [FromQuery]string encryptedIV)
        {
            // Check if strings are filled.
            if (string.IsNullOrEmpty(encryptedInteger) || string.IsNullOrEmpty(encryptedSymKey) || string.IsNullOrEmpty(encryptedIV))
            {
                // Not filled.
                UserDatabaseAccess.WriteLog(DbContext, Request.Headers["ApiKey"], "Tried to use AddFifty but didn't fill one of the parameters.");

                return new ContentResult()
                {
                    Content = "Bad Request.",
                    StatusCode = 400,
                    ContentType = "text/plain"
                };
            }

            // First convert all strings to byte[]s
            var intbytes = StringToByteArray(encryptedInteger);
            var keybytes = StringToByteArray(encryptedSymKey);
            var ivbytes = StringToByteArray(encryptedIV);

            // Decrypt all params
            var decrypted_int = _cryptoService.Decrypt(intbytes, true);
            var decrypted_key = _cryptoService.Decrypt(keybytes, true);
            var decrypted_iv  = _cryptoService.Decrypt(ivbytes,  true);

            // Create AES instance
            var aesProvider = new AesCryptoServiceProvider();
            aesProvider.Key = decrypted_key;
            aesProvider.IV = decrypted_iv;

            // Create an encryptor.
            var encryptor = aesProvider.CreateEncryptor();

            // Add 50
            var number = BitConverter.ToInt32(decrypted_int);
            number += 50;

            byte[] encryptedMessageBytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(number.ToString());
                    }

                    encryptedMessageBytes = ms.ToArray();
                }
            }

            var result = BitConverter.ToString(encryptedMessageBytes);


            return new ContentResult()
            {
                Content = result,
                StatusCode = 200,
                ContentType = "text/plain"
            };
        }

        // Hex convertion code form StackOverflow, slightly modified.
        // https://stackoverflow.com/a/9995303
        public static byte[] StringToByteArray(string hex)
        {
            // Strip the delimiters
            hex = hex.Replace("-", string.Empty);

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }

        // </stackoverflow>
    }
}
