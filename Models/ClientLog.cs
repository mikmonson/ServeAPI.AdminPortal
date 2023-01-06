using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models
{
    public class ClientLog
    {
        [Key]
        [Required]
        public long id { get; set; }
        [Required]
        public string sn { get; set; }
        [Required]
        public int customer_id { get; set; }
        public DateTime timestamp { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public long clienttask_id { get; set; }
        public string reserved { get; set; } //error, info (client log), task (task update)
        public string _not_exported_hostname; //not exported to DB
    }
}
