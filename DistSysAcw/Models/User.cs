using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DistSysAcw.Models
{
    public class User
    {
        public User()
        {

        }

        [Key]
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }

    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public static class UserDatabaseAccess
    {
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 
        #endregion

        public static Guid CreateUser(UserContext context, string username)
        {
            var guid = Guid.NewGuid();

            bool valid = false;
            while (!valid)
            {
                if (CheckGuid(context, guid.ToString()))
                {
                    // Already exists
                    guid = Guid.NewGuid();
                }
                else
                {
                    valid = true;
                }
            }

            var roleString = context.Users.Count() == 0 ? "Admin" : "User";

            var newUser = new User()
            {
                ApiKey = guid.ToString(),
                Role = roleString,
                UserName = username
            };

            context.Users.Add(newUser);

            context.SaveChanges();

            return guid;
        }

        public static bool CheckGuid(UserContext context, string guid)
        {
            return context.Users.Any(u => u.ApiKey == guid);
        }

        public static bool CheckUsername(UserContext context, string username)
        {
            return context.Users.Any(u => u.UserName == username);
        }

        public static bool CheckUser(UserContext context, string guid, string username)
        {
            return context.Users.Any(u => u.ApiKey == guid && u.UserName == username);
        }

        public static User GetUser(UserContext context, string guid)
        {
            var user = new User()
            {
                ApiKey = "UNDEFINED",
                Role = "UNDEFINED",
                UserName = ""
            };

            var exists = CheckGuid(context, guid);

            if (exists)
            {
                user = context.Users.First(u => u.ApiKey == guid);
            }

            return user;
        }

        public static User GetUserUsername(UserContext context, string username)
        {
            var user = new User()
            {
                ApiKey = "UNDEFINED",
                Role = "UNDEFINED",
                UserName = ""
            };

            var exists = CheckUsername(context, username);

            if (exists)
            {
                user = context.Users.First(u => u.UserName == username);
            }

            return user;
        }

        public static void DeleteUser(UserContext context, string guid)
        {
            var exists = CheckGuid(context, guid);

            if (exists)
            {
                var user = GetUser(context, guid);
                context.Users.Remove(user);
            }

            context.SaveChanges();
        }

        public static void ChangeRole(UserContext context, string username, string role)
        {
            var user = GetUserUsername(context, username);

            user.Role = role;

            context.SaveChanges();
        }

    }


}