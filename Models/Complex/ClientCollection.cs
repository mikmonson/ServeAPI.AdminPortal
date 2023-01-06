using AdminPortal.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Models.Complex
{


    public class ClientCollection
    {
        public Client myclient;
        public List<ClientMetric> mymetrics;
        public List<ClientTask> mytasks;
        public List<ClientLog> mylog;
        public List<TaskFiles> taskfiles;
        public List<TaskCommands> taskcommands;
    }
}
