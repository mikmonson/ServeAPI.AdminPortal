using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Data
{
    public class DataItem
    {
        [Key]
        [Required]
        public long id { get; set; }
        public string sn { get; set; }
        public int customer_id { get; set; }
        public DateTime timestamp { get; set; }
        public string content_type { get; set; } //binary, text, ...
        public int tasktype { get; set; } //1 - Client Task, 2 - Agent Task
        public string task_id { get; set; }
        public byte[] content { get; set; } //File data
        public string file_name { get; set; }
        public string checksum { get; set; } //reserved for md5 checksum

        public DataItem()
        {
            task_id = "";
        }
    }
}
