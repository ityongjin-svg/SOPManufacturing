using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOP_API.Models
{
    public class Auth
    {
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}