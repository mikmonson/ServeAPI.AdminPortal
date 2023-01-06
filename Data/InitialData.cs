using AdminPortal.Models;
using AdminPortal.Models.List;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPortal.Data
{
   
    public class InitialData
    {
        static private string GetStringCommand()
        {
            string res = "";
            List<JSON_Command> jc = new List<JSON_Command>();
            jc.Add(new JSON_Command("ls"));
            jc.Add(new JSON_Command("pwd"));
            res = JsonConvert.SerializeObject(jc);
            return res;
        }

        static private string GetStringFile()
        {
            string res = "";
            List<JSON_File> jc = new List<JSON_File>();
            jc.Add(new JSON_File(1,"binary","text.txt"));
            jc.Add(new JSON_File(1, "binary","test.txt"));
            res = JsonConvert.SerializeObject(jc);
            return res;
        }
        public static void Initialize(EdgeDBContext context)
        {
            /* if (!context.Users.Any())
            {
                context.Users.Add(new Models.Auth.User { Customer_id = -100, Email = "mikmon@yandex.ru", Fullname = "Mikhail Monko", Lastpasswordchange = DateTime.Now, Mustchangepassword = false, Passwordhash = "7C4A8D09CA3762AF61E59520943DC26494F8941B", Userclass = "admin", Username = "admin" });
                context.Users.Add(new Models.Auth.User { Customer_id = -100, Email = "mikmonson@mail.ru", Fullname = "Mikhail Monko2", Lastpasswordchange = DateTime.Now, Mustchangepassword = false, Passwordhash = "7C4A8D09CA3762AF61E59520943DC26494F8941B", Userclass = "user", Username = "mikmon" });
                context.SaveChanges();
            }

            if (!context.Clients.Any())
            {
                context.Clients.AddRange(
                    new Client
                    {
                        sn = "dscd042030",
                        mac = "e0:ca:94:54:4e:4f",
                        model = "edge_windows_1.0",
                        hostname = "my_windows_edge",
                        location = "dxb",
                        customer_id = 1,
                        servapi_host = "prod.edgeservapi.com",
                        servapi_port = "5001",
                        provisioned = "yes",
                        status = "active",
                        ssh_enabled = "yes",
                        ssh_command = "autossh -p222 -nNTR 2224:localhost:22 -f -p 5544 tester@sectoid.dlinkddns.com",
                        client_update_freq = "0 0 0 0 0 ./root/client",
                        program_update_freq = "0 0 0 0 0 ./root/program",
                        client_version = "1",
                        program_dir = "C:\\Users\\komandor\\AppData\\Local\\Programs\\Python\\Python38-32"
                    },
                    new Client
                    {
                        sn = "000c29342810",
                        mac = "00:0c:29:34:28:10",
                        model = "edge_linux_1.0",
                        location = "dxb",
                        hostname = "mylinuxedge",
                        customer_id = 2,
                        servapi_host = "prod.edgeservapi.com",
                        servapi_port = "5001",
                        provisioned = "yes",
                        status = "active",
                        ssh_enabled = "no",
                        ssh_command = "autossh -p222 -nNTR 2224:localhost:22 -f -p 5544 tester@sectoid.dlinkddns.com",
                        client_update_freq = "0 0 0 0 0 ./root/client",
                        program_update_freq = "0 0 0 0 0 ./root/program",
                        client_version = "1",
                        program_dir = "/home/mikmon/projects/Client/out"

                    }
                );
                context.SaveChanges();
            }
            
            if (!context.ClientTasks.Any())
            {
                context.ClientTasks.AddRange(
                    new ClientTask
                    {
                        sn = "dscd042030",
                        customer_id = 1,
                        timestamp = new DateTime(2015, 12, 31, 5, 10, 20),
                        direction = 1,
                        task_type = "command",
                        status = "completed",
                        json_params = null,
                        JSON_File_source_files = null,
                        //JSON_Command_commands = GetStringCommand(),
                        JSON_Command_commands = "{\"_commands\":[{\"command\":\"ls\"},{\"command\":\"dir\"}]}",
                        result_file = 0,
                        client_log_id = 0
                    },
                    new ClientTask
                    {
                        sn = "000c29342810",
                        customer_id = 2,
                        timestamp = new DateTime(2015, 12, 31, 5, 10, 20),
                        direction = 1,
                        task_type = "update",
                        status = "completed",
                        json_params = null,
                        //JSON_File_source_files = GetStringFile(),
                        JSON_File_source_files = "{\"_files\":[{\"content_index\":1,\"content_type\":\"binary\",\"file_name\":\"test.txt\"},{\"content_index\":2,\"content_type\":\"binary\",\"file_name\":\"test2.txt\"}]}",
                        JSON_Command_commands = null,
                        result_file = 0,
                        client_log_id = 0
                    },
                    new ClientTask
                    {
                        sn = "000c29342810",
                        customer_id = 2,
                        timestamp = new DateTime(2015, 12, 31, 5, 10, 20),
                        direction = 1,
                        task_type = "settings",
                        status = "completed",
                        json_params = "{ \"action\": \"settings\", \"ssh_enabled\": \"no\", \"ssh_command\": \"autossh - p222 - nNTR 2224:localhost:22 - f - p 5544 tester@sectoid.dlinkddns.com\" }",
                        JSON_File_source_files = null,
                        JSON_Command_commands = null,
                        result_file = 0,
                        client_log_id = 0
                    }
                ) ;
                context.SaveChanges();
            }

            if (!context.ClientMetrics.Any())
            {
                context.ClientMetrics.AddRange(
                    new ClientMetric
                    {
                        sn = "000c29342810",
                        customer_id = 2,
                        uptime = 5349,
                        last_ip = "127.0.1.1",
                        memory = 77.5,
                        cpu = 70,
                        free_disk = 10043,
                        lastseenonline = new DateTime(2017, 11, 2, 4, 1, 24)
                    },
                    new ClientMetric
                    {
                        sn = "000c29342810",
                        customer_id = 2,
                        uptime = 6349,
                        last_ip = "127.0.1.1",
                        memory = 57.5,
                        cpu = 50,
                        free_disk = 10041.6,
                        lastseenonline = new DateTime(2015, 12, 31, 5, 10, 20)
                    },
                    new ClientMetric
                    {
                        sn = "dscd042030",
                        customer_id = 1,
                        uptime = 49,
                        last_ip = "192.168.1.1",
                        memory = 52.1,
                        cpu = 10,
                        free_disk = 4041.8,
                        lastseenonline = new DateTime(2019, 1, 1, 15, 20, 20)
                    }
                );
                context.SaveChanges();
            }

            if (!context.ClientLogs.Any())
                {
                    context.ClientLogs.AddRange(
                        new ClientLog
                        {
                            sn = "dscd042030",
                            customer_id = 1,
                            timestamp = new DateTime(2019, 1, 1),
                            message = "HELLO MESSAGE",
                            title = "hello",
                            clienttask_id = 0,
                            reserved ="kkk"
                        },
                        new ClientLog
                        {
                            sn = "000c29342810",
                            customer_id = 2,
                            timestamp = new DateTime(2019, 2, 1),
                            message = "HELLO MESSAGE",
                            title = "hello",
                            clienttask_id = 0,
                        }/*,
                        new ClientLog
                        {
                            sn = "000c29342810",
                            customer_id = 2,
                            timestamp = new DateTime(2015, 12, 31, 5, 10, 24),
                            message = "CONFIRM MESSAGE",
                            title = "confirm",
                            clienttask_id = -1,
                        },
                        new ClientLog
                        {
                            sn = "dscd042030",
                            customer_id = 1,
                            timestamp = new DateTime(2016, 12, 31, 5, 10, 21),
                            message = "ERROR MESSAGE",
                            title = "error",
                            clienttask_id = -1,
                        },
                        new ClientLog
                        {
                            sn = "dscd042030",
                            customer_id = 1,
                            timestamp = new DateTime(2017, 12, 31, 5, 10, 10),
                            message = "HELLO MESSAGE",
                            title = "hello",
                            clienttask_id = -1,
                        },
                        new ClientLog
                        {
                            sn = "dscd042030",
                            customer_id = 1,
                            timestamp = new DateTime(2018, 12, 31, 5, 10, 2),
                            title = "hello",
                            message = "ERROR MESSAGE",
                            clienttask_id = -1,
                        }
                    );
                    context.SaveChanges();
                
            }*/
        }
    }
}
