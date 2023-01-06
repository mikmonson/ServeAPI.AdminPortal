using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.Lists
{
    public class TaskUploads
    {
        public string tcommands { get; set; }
        public string ttype { get; set; }
        public string sn { get; set; }
        public string location { get; set; }
        public List<string> tfiles { get; set; }
    }

 
}
