using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Passwordhash { get; set; }
        public string Email { get; set; }
        public int Customer_id { get; set; } //-100=admin, 1,2,....=normal users
        public string Fullname { get; set; }
        public bool Mustchangepassword { get; set; }
        public DateTime Lastpasswordchange { get; set; }
        public string Userclass { get; set; } //user, supervisor, admin
    }
}
