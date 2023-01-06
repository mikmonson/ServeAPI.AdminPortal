using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.List
{
      public class TaskFiles
    {
        public List<JSON_File> _files = new List<JSON_File>();
        public string location="";

        public TaskFiles(string _location)
        {
            location = _location;
        }
    }

    public class File
    {
        public long id { get; set; }
        public long content_index { get; set; }
        public string content_type { get; set; } //binary, text, ...
        public string file_name { get; set; }
    }

    public class JSON_File
    {
        public long content_index;
        public string content_type; //binary, text, ...
        public string file_name;

        public JSON_File(long i, string ct, string fn)
        {
            content_index = i;
            content_type = ct;
            file_name = fn;
        }
    }

}
