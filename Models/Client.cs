using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models
{
    
    public class Client  //EDGE Client Description
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "Identifier")]
        [Key]
        public string sn { get; set; }
        [Required]
        [StringLength(17)]
        [Display(Name = "MAC-address")]
        public string mac { get; set; }
        [Required]
        [StringLength(30)]
        [Display(Name = "Model")]
        public string model { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Device name")]
        public string hostname { get; set; }
        [StringLength(30)]
        [Display(Name = "Location")]
        public string location { get; set; }
        [Required]
        [Display(Name = "Owner ID")]
        public int customer_id { get; set; }
        [Required]
        [StringLength(30)]
        [Display(Name = "Server name")]
        public string servapi_host { get; set; }
        [Required]
        [StringLength(5)]
        [Display(Name = "Server port")]
        public string servapi_port { get; set; }
        public string provisioned { get; set; }
        [Required]
        [StringLength(4)]
        [Display(Name = "SSH")]
        public string ssh_enabled { get; set; }
        [Display(Name = "Last seen online")]
        public DateTime lastseenonline { get; set; }
        [Display(Name = "Status")]
        public string status { get; set; } //Provisioned, Deleted, Online
        [Display(Name = "Type")]
        public int type { get; set; } //1 - openwrt nw, 2 - sensors
        [Display(Name = "SSH command")]
        public string ssh_command { get; set; }
        [Display(Name = "Client crontab attribute")]
        public string client_update_freq { get; set; } //shoule have crontab format like "0 0 0 0 0  python /root/client.py"
        [Display(Name = "Agent crontab attribute")]
        public string program_update_freq { get; set; } //shoule have crontab format like "0 0 0 0 0 ./root/program"
        [Display(Name = "Client version")]
        public string client_version { get; set; }
        [Display(Name = "Agent directory")]
        public string program_dir { get; set; }
        public string public_key { get; set; }
        //More fields to be added
    }
}
