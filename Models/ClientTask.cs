using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using AdminPortal.Models.List;

namespace AdminPortal.Models
{

   
    public class ClientTask //Edge Client Task
    {
        [Key]
        [Required]
        public long id { get; set; }
        [Required]
        [Display(Name = "Identifier")]
        public string sn { get; set; }
        [Required]
        public int customer_id { get; set; }
        [Display(Name = "Timestamp")]
        public DateTime timestamp { get; set; }
        public int direction { get; set; } //1 - outgoing, 2 - incoming
        public string task_type { get; set; } //Outgoing: command, update, settings; Incoming: update.
        [Display(Name = "Status")]
        public string status { get; set; }  //pending, complete
        public string json_params { get; set; } //json file for settings change
//        public List<File> source_files { get; set; } //index of dataitem with file content
        public string JSON_File_source_files { get; set; } //index of dataitem with file content
//        public List<Command> commands { get; set; } //index of dataitem with file content
        public string JSON_Command_commands { get; set; } //index of dataitem with file content
        [Display(Name = "Result")]
        public long result_file { get; set; } //index of dataitem with task result
        [Display(Name = "Log")]
        public long client_log_id { get; set; } //index of clientlog with task journal record
        
        [Display(Name = "Commands execution")]        
        public TaskCommands _commands; //not exported to DB
        [Display(Name = "Files upload/download")]
        public TaskFiles _files; //not exported to DB
        [Display(Name = "Settings change")]
        public string _json_settings_simple; //not exported to DB
        [Display(Name = "Type")]
        public string _type_simple; //not exported to DB
        public string _not_exported_hostname; //not exported to DB
    }
}
