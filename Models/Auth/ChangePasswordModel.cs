using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.Auth
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Password not defined")]
        public string Oldpassword { get; set; }

        [Required(ErrorMessage = "Password not defined")]
        [DataType(DataType.Password)]
        public string Newpassword { get; set; }
    }
}
