using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DistSysAcwClient
{
    #region Task 10 and beyond

    class User
    {
        public string Apikey { get; set; }
        public string Username { get; set; }
    }


    class Client
    {
        static readonly HttpClient client = new HttpClient();
        static User clientUser = new User();
        static string baseUri = "https://localhost:44394/api/";
        static string pubKey;

        static void Main(string[] args)
        {
            bool flag = false;
            Console.WriteLine("Hello. What would you like to do?");

            while (true)
            {
                try
                {
                    if (flag)
                    {
                        Console.WriteLine("What would you like to do next?");
                    }
                    var resp = Console.ReadLine();
                    if (resp == "Exit") { break; }
                    Console.Clear();

                    var words = resp.Split(new char[] { ' ' });
                    if (words.Length == 0)
                    {
                        Console.WriteLine("Please enter a command.");
                        continue;
                    }

                    HandleRequest(words);

                    flag = true;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();
                }
                
            }
        }

        static void HandleRequest(string[] words)
        {
            switch(words[0])
            {
                case "TalkBack":
                    HandleTalkback(words);
                    break;

                case "User":
                    HandleUser(words);
                    break;

                case "Protected":
                    HandleProtected(words);
                    break;

                default:
                    Console.WriteLine("Unsupported action. Please try again.");
                    break;
            }
        }

        static void HandleTalkback(string[] words)
        {
            if (words.Length < 2)
            {
                Console.WriteLine("Please Enter TalkBack command.");
                return;
            }
            Task<HttpResponseMessage> resultTask;
            Task<string> stringTask;

            switch (words[1])
            {
                case "Hello":
                    resultTask = SendRequest("talkback/hello", HttpMethod.Get, false);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);
                    break;

                case "Sort":
                    if (words.Length < 3)
                    {
                        Console.WriteLine("Please Enter Numbers array.");
                        return;
                    }

                    Regex format = new Regex("^\\[[0-9]*(,[0-9]+)*\\]$");
                    var numbers = words[2];

                    if (!format.IsMatch(numbers))
                    {
                        Console.WriteLine("Incorrect numbers format.");
                        break;
                    }

                    numbers = numbers.Replace("[", string.Empty).Replace("]", string.Empty);
                    var numbersArray = numbers.Split(new char[] { ',' });

                    var uri = "talkback/sort";

                    for (int i = 0; i < numbersArray.Length; i++)
                    {
                        if (i == 0) { uri = uri + "?"; }
                        uri = uri + $"integers={numbersArray[i]}";
                        if (i != numbersArray.Length - 1) { uri = uri + "&"; }
                    }

                    resultTask = SendRequest(uri, HttpMethod.Get, false);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                default:
                    Console.WriteLine("Unsupported action. Please try again.");
                    break;
            }
        }

        static void HandleUser(string[] words)
        {
            Task<HttpResponseMessage> resultTask;
            Task<string> stringTask;
            if (words.Length < 2)
            {
                Console.WriteLine("Please Enter User command.");
                return;
            }

            switch (words[1])
            {
                case "Get":
                    if (words.Length < 3)
                    {
                        Console.WriteLine("Please Enter User Name.");
                        return;
                    }
                    resultTask = SendRequest($"user/new?username={words[2]}", HttpMethod.Get, false);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                case "Post":
                    if (words.Length < 3)
                    {
                        if (words[2].Length < 1)
                        {
                            Console.WriteLine("Please Enter User Name");
                        }
                        Console.WriteLine("Please Enter User Name.");
                        return;
                    }

                    resultTask = SendRequest($"user/new", HttpMethod.Post, false, $"\"{words[2]}\"");
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    if (resultTask.Result.StatusCode == HttpStatusCode.OK)
                    {
                        clientUser.Apikey = stringTask.Result;
                        clientUser.Username = words[2];

                        Console.WriteLine("Got API Key");
                    }
                    else
                    {
                        Console.WriteLine(stringTask.Result);
                    }

                    break;

                case "Set":
                    if (words.Length < 4)
                    {
                        Console.WriteLine("Please Enter Name and ApiKey.");
                        return;
                    }

                    clientUser.Username = words[2];
                    clientUser.Apikey = words[3];

                    Console.WriteLine("Stored");

                    break;

                case "Delete":

                    // Check locals
                    if (!CheckLocals()) { break; }

                    resultTask = SendRequest($"user/removeuser?username={clientUser.Username}", HttpMethod.Delete, true);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    string result = char.ToUpper(stringTask.Result[0]) + stringTask.Result.Substring(1);

                    Console.WriteLine(result);

                    if (result == "True")
                    {
                        clientUser.Username = "";
                        clientUser.Apikey = "";
                    }

                    break;

                case "Role":
                    if (words.Length < 4)
                    {
                        Console.WriteLine("Please Enter Name and Role.");
                        return;
                    }

                    // Check locals
                    if (CheckLocals()) { break; }

                    var jsonString = "{\"username\":\"" + words[2] + "\", \"role\":\"" + words[3] + "\"}";

                    resultTask = SendRequest($"user/changerole", HttpMethod.Post, true, jsonString);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                default:
                    Console.WriteLine("Unsupported action. Please try again.");
                    break;
            }
        }

        static void HandleProtected(string[] words)
        {
            Task<HttpResponseMessage> resultTask;
            Task<string> stringTask;

            if (words.Length < 2)
            {
                Console.WriteLine("Please Enter Protected command.");
                return;
            }

            switch (words[1])
            {
                case "Hello":
                    // Check locals
                    if (CheckLocals()) { break; }

                    resultTask = SendRequest($"protected/hello", HttpMethod.Get, true);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                case "SHA1":
                    if (words.Length < 3)
                    {
                        Console.WriteLine("Please Enter Message.");
                        return;
                    }

                    // Check locals
                    if (CheckLocals()) { break; }

                    resultTask = SendRequest($"protected/sha1?message={words[2]}", HttpMethod.Get, true);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                case "SHA256":
                    if (words.Length < 3)
                    {
                        Console.WriteLine("Please Enter Message.");
                        return;
                    }

                    // Check locals
                    if (CheckLocals()) { break; }

                    resultTask = SendRequest($"protected/sha256?message={words[2]}", HttpMethod.Get, true);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    Console.WriteLine(stringTask.Result);

                    break;

                case "Get":
                    if (words.Length < 3)
                    {
                        Console.WriteLine("Please Enter Message.");
                        return;
                    }
                    if (words[2] != "PublicKey")
                    {
                        Console.WriteLine("Please Enter a Valid Command.");
                        return;
                    }

                    // Check locals
                    if (CheckLocals()) { break; }

                    resultTask = SendRequest($"protected/getpublickey", HttpMethod.Get, true);
                    Console.WriteLine("...please wait...");
                    resultTask.Wait();

                    stringTask = resultTask.Result.Content.ReadAsStringAsync();
                    stringTask.Wait();

                    if (resultTask.Result.StatusCode == HttpStatusCode.OK)
                    {
                        pubKey = stringTask.Result;

                        Console.WriteLine("Got Public Key");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't Get the Public Key");
                    }

                    break;

                default:
                    Console.WriteLine("Unsupported action. Please try again.");
                    break;
            }
        }

        static async Task<HttpResponseMessage> SendRequest(string uriExtension, HttpMethod method, bool useApiKey, string content = "")
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(baseUri + uriExtension);
            request.Method = method;
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            if (useApiKey)
            {
                request.Headers.Add("ApiKey", clientUser.Apikey);
            }


            return await client.SendAsync(request);
        }

        static bool CheckLocals()
        {
            bool result = clientUser.Username == "" || clientUser.Apikey == "";
            if (result)
            {
                Console.WriteLine("You need to do a User Post or User Set first");
            }

            return result;
        }

    }
    #endregion
}
