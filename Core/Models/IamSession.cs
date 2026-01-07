using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class IamSession
    {
        public class User
        {
            public string username;
            private string password;
            public string role;
        }

        public class Role
        {
            static string STUDENT = "student";
            static string MANAGER = "manager";
        }

        public string token;
        public User user;
        public string ip_address;
    }
}
