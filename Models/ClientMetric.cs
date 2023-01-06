using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models
{
    public class ClientMetric
    {
        [Key]
        [Required]
        public long id { get; set; }
        [Required]
        public string sn { get; set; }
        [Required]
        public int customer_id { get; set; }
        public long uptime { get; set; }
        public string last_ip { get; set; }
        public double memory { get; set; }
        public double cpu { get; set; }
        public double free_disk { get; set; }
        public DateTime lastseenonline { get; set; }
        public double network_load { get; set; }
        public string reserved1 { get; set; }
        public string reserved2 { get; set; }
        public string reserved3 { get; set; }
        public string _not_exported_hostname; //not exported to DB
    }
}
