using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models
{
    public class TaskCommands
    {
        public List<JSON_Command> _commands = new List<JSON_Command>();
    }
    public class Command
    {
        public long id { get; set; }
        public string command { get; set; }

    }

    public class JSON_Command
    {
        public string command;

        public JSON_Command(string _cmd)
        {
            command = _cmd;
        }
    }
}
