using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.Auth
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username not defined")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password not defined")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
