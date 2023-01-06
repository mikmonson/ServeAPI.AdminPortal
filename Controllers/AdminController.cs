using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AdminPortal.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System.Net;
using Microsoft.AspNetCore.Http;
using AdminPortal.Models.Complex;
using Newtonsoft.Json;
using AdminPortal.Models.List;
using AdminPortal.Models.Lists;
using Nancy.Json;
using System.IO;
using AdminPortal.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SERVAPI;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Fabric;
using System.IO.Compression;

namespace AdminPortal.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        EdgeDBContext db;
        public AdminController(EdgeDBContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            if (role == "admin")
            {
                return View(db.Clients.ToList()); //Not needed
            } else
            {
                return View(db.Clients.Where(s => s.customer_id==customer_id).ToList()); //Not needed
            }           
        }

        public IActionResult GetFile(string filename, string sn) //Reads file from filesystem or <sn>.zip file
        {
            bool access = false;
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string err = "Specified resource couldn't be retrieved";
            if ((filename != null) || (sn!=null))
            {
                try
                {
                    string filename1 = "";
                    if (sn == null)
                    {
                        filename1 = FilePath.file_path + Path.GetFileName(filename);
                        access = true;
                    }
                    else
                    {
                        Client ct = db.Clients.First(s => s.sn == sn.ToLower() && ((s.customer_id == customer_id) || (role == "admin")));
                        if (ct != null)
                        {
                            filename1 = FilePath.device_path + Path.GetFileName(sn)+".zip";
                            err = "The archive is not yet available. Please try again later.";
                            access = true;
                        }
                    }
                    if (access == true)
                    {
                        using FileStream fsSource = new FileStream(filename1, FileMode.Open, FileAccess.Read);
                        // Read the source file into a byte array.
                        byte[] bytes = new byte[fsSource.Length];
                        int numBytesToRead = (int)fsSource.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            // Read may return anything from 0 to numBytesToRead.
                            int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);
                            // Break when the end of the file is reached.
                            if (n == 0)
                                break;
                            numBytesRead += n;
                            numBytesToRead -= n;
                        }
                        return File(bytes, "application/force-download", Path.GetFileName(filename1));
                    }
                }
                catch
                {
                    Debug.WriteLine("Error occurred while downloading file");
                }
            }
            return RedirectToAction("Error", "Admin", new { mes = err });
        }

        public IActionResult Clients(string filter)
        {
            try
            {
                string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                if (role == "admin") ViewData["admin"] = true;
                else ViewData["admin"] = false;
                if (filter == null) //no fliter set
                {
                    return View(db.Clients.Where(s => s.customer_id == customer_id || role == "admin").ToList());
                }
                else // filter set
                {
                    string ft = filter.Substring(0,1);
                    string fc = filter.Substring(1, filter.Length - 1);

                    return View(db.Clients.Where(s => 
                    (((ft=="1") && (s.hostname.Contains(fc))) || ((ft == "2") && (s.location.Contains(fc))) || ((ft == "3") && (s.model.Contains(fc))))
                    && ((s.customer_id == customer_id) || (role == "admin"))
                    ).ToList());
                }
            } catch
            {
                Debug.WriteLine("Error occurred in Clients method");
            }
            return RedirectToAction("Error");
        }

        public ActionResult Error(string mes)
        {
            
            if (mes != null) {
                ViewData["error"] = mes;
            }  else  {
                ViewData["error"] = "Error - Operation couldn't be completed!";
            }
            return View();
        }

        public ActionResult Info(string mes, string url, string link)
        {

            if (mes != null)
            {
                ViewData["logo"] = mes;
                if (url != null) ViewData["url"] = url;
            }
            else
            {
                ViewData["logo"] = "Operation has been completed";
                ViewData["url"] = "Index";
            }
            
            if (link!=null)
            {
                ViewData["link"] = link;
            } else
            {
                ViewData["link"] = "";
            }
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult test()
        {
            ViewData["info"] = "< a href = \"~/Admin/Clients\" > Back </ a >";
            return View();
        }


        [HttpGet]
        public IActionResult NewTask(string id, string sn)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            List<string> sns = new List<string>();
            string idd;
            if (id == null) idd = "";
            else idd = id;
          
            if ((idd.Length>1) || (sn!=null))
            {
                try
                {
                    if (sn != null) //Multiple clients from sn var -> validate customer_id for all selected clients
                    {
                        sns = sn.Substring(1).Split(":").ToList();
                        idd = sns[0];
                        int lastid = 0;
                        for (int k = 0; k < sns.Count; k++)
                        {
                            Client temp_cl = db.Clients.FirstOrDefault(s => s.sn == sns[k]);                            
                            if (k == 0) lastid = temp_cl.customer_id;
                            if (temp_cl.customer_id!=lastid)
                            {
                                return RedirectToAction("Error", "Admin", new { mes = "Error - can't create tasks for clients that belong to different customers" });
                            }
                            if ((temp_cl.customer_id!=customer_id) && (role!="admin")) {
                                return RedirectToAction("Error", "Admin", new { mes = "Authorization error - not all selected clients are associated with your account" });
                            }
                        }
                    } else
                    {
                        sn = id; //If it's a single task job
                    }
                    Client cl = db.Clients.First(s => s.sn == idd);
                    if ((cl != null) && ((customer_id == cl.customer_id) || (role == "admin")))
                    {
                        //ViewData["sn"] = cl.sn;
                        ViewData["sn"] = sn;
                        ViewData["customer_id"] = cl.customer_id;
                        ViewData["program_dir"] = cl.program_dir;
                        ViewData["error"] = "";
                        TaskUploads tu = new TaskUploads();
                        tu.sn = cl.sn;
                        db.mytestvar = cl.sn;
                        Debug.WriteLine("NewTask POST" + db.mytestvar);
                        return View(tu);
                    }
                    else return RedirectToAction("Error");

                }
                  catch
                  {
                      return RedirectToAction("Error");
                  }
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> NewTaskAsync(TaskUploads tu, [FromForm(Name = "file")] IFormFileCollection file)
        {
            bool success = false;
            string errmes = "Error in the form data";
            List<string> _commands = new List<string>();
            long lasttaskid = 0;
            Debug.WriteLine("NewTask POST"+db.mytestvar);
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            List<DataItem> dal = new List<DataItem>();
            bool da1_complete = false;
            string da_taskids = "";
            string listsns = tu.sn.ToLower();
            if (tu.location == null) tu.location = "";
            TaskFiles tf = new TaskFiles(tu.location);

            if ((tu != null) && (tu.sn != null) && (tu.ttype != null))
            {
                List<string> sns = new List<string>();
                if (tu.sn.Contains(":")) //multiclient task
                {
                    sns = tu.sn.Substring(1).Split(":").ToList();
                    listsns = listsns.Substring(1, listsns.Length - 1); //listsns stores original sequence of sn
                }
                else sns.Add(tu.sn); //Single client task

                for (int k = 0; k < sns.Count; k++)
                {
                    ClientTask newtask = new ClientTask();
                    Client _myclient = new Client();
                    tu.sn = sns[k];
                    _myclient = db.Clients.First(s => s.sn == tu.sn);
                    if ((_myclient.customer_id != customer_id) && (role != "admin"))
                    {
                        return RedirectToAction("Error", "Admin", new { mes = "Authorization error - customer id from device doesn't match your customer id." });
                    }
                    newtask.sn = tu.sn.ToLower();
                    newtask.timestamp = DateTime.Now;
                    newtask.customer_id = _myclient.customer_id;
                    newtask.direction = 1;
                    newtask.status = "pending";
                    newtask.result_file = 0;
                    newtask.client_log_id = 0;
                    if (tu.tcommands == null) tu.tcommands = "";
                    if (((tu.ttype.Contains("Execute command(s)") || (tu.ttype.Contains("Upload files(s) and execute 1 command"))) && (tu.tcommands.Length > 1)))
                    {
                        string[] cmd = tu.tcommands.Replace("\r", "").Split("\n");
                        TaskCommands tk = new TaskCommands();
                        for (int i = 0; i < cmd.Length; i++)
                        {
                            if (cmd[i].Length > 0)
                            {
                                Debug.WriteLine("aaaaa" + cmd[i] + "ssssss");
                                tk._commands.Add(new JSON_Command(cmd[i]));
                                if (tu.ttype.Contains("Upload files(s) and execute 1 command")) break;
                            }
                        }
                        newtask.JSON_Command_commands = new JavaScriptSerializer().Serialize(tk);
                        Debug.WriteLine(newtask.JSON_Command_commands);
                        if (tu.ttype.Contains("Execute command(s)"))
                        {
                            newtask.task_type = "command";
                        }
                        success = true;

                    }
                    else if ((tu.ttype.Contains("Execute command(s)")) && (tu.tcommands.Length <= 1)) errmes = "Error in \"Command\" field";
                    if (tu.ttype.Contains("Upload files(s)"))
                    {
                        if (file.Count > 0)
                        {
                            newtask.task_type = "update";
                            long totalsize = 0;
                            if (da1_complete == false)
                            {
                                foreach (var uploadedFile in file)
                                {
                                    if (uploadedFile.Length > 0)
                                    {
                                        using var ms = new MemoryStream();
                                        await uploadedFile.CopyToAsync(ms);
                                        DataItem da = new DataItem();
                                        da.file_name = Path.GetFileName(uploadedFile.FileName);
                                        da.content_type = "binary";
                                        //da.sn = _myclient.sn;
                                        da.sn = listsns; //add entire task sequence in data item
                                        da.customer_id = _myclient.customer_id;
                                        da.tasktype = 1;
                                        da.content = ms.ToArray();
                                        totalsize = totalsize + da.content.Length;
                                        if (totalsize > 10485760)
                                        {
                                            return RedirectToAction("Error", "Admin", new { mes = "Total size of files can't exceed 10MB" });
                                        }
                                        dal.Add(da);
                                    }
                                }
                                await db.DataItems.AddRangeAsync(dal);
                                await db.SaveChangesAsync();

                                for (int i = 0; i < dal.Count; i++)
                                {
                                    tf._files.Add(new JSON_File(dal[i].id, dal[i].content_type, dal[i].file_name));
                                }
                                da1_complete = true;
                            }
                            newtask.JSON_File_source_files = new JavaScriptSerializer().Serialize(tf);
                            success = true;
                        }
                        else
                        {
                            errmes = "Files were not attached";
                            success = false;
                        }
                    }
                    if (success == true)
                    {
                        await db.ClientTasks.AddAsync(newtask);
                        await db.SaveChangesAsync();
                        lasttaskid = newtask.id;

                        //Теперь нам надо перебрать все загруженные файлы и привязать список task id к каждому.
                        if (k > 0) da_taskids = da_taskids + ":" + Convert.ToString(lasttaskid); //Multiple tasks as k>0
                        else da_taskids = Convert.ToString(lasttaskid); //First itterruption of k-loop
                        if (dal.Count > 0)
                        {
                            for (int i = 0; i < dal.Count; i++)
                            {
                                //db.DataItems.First(s => s.id == dal[i].id).task_id = newtask.id;
                                dal[i].task_id = da_taskids;
                            }
                            await db.SaveChangesAsync();
                        }
                        //return RedirectToAction("Tasks", "Admin", new { id = newtask.id, sn = _myclient.sn });
                    }
                }
                if (sns.Count > 1)
                {
                    return RedirectToAction("Tasks", "Admin");
                }
                else
                {
                    return RedirectToAction("Tasks", "Admin", new { id = lasttaskid, sn = sns[0] });
                }

            }
            else errmes = "The form didn't contain some mandatory data";
            
            return RedirectToAction("Error", "Admin", new { mes = errmes });
        }

        public IActionResult Cancel(long id)
        {
            if (id>0)
            {
                try
                {
                    int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                    string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                    ClientTask ct = new ClientTask();
                    if (role == "admin") {
                        ct = db.ClientTasks.First(s => s.id == id);
                    } else {
                        ct = db.ClientTasks.First(s => s.id == id && s.customer_id == customer_id);
                    }
                    if ((ct.status != "completed") && (ct.status != "canceled"))
                    {
                        string mess = "Task has been canceled";
                        if (ct.direction == 1) ct.status = "canceled";
                        else
                        {
                            ct.status = "completed"; //For upload tasks we call it "completed"
                            mess = "Job has been acknowledged";
                        }
                        db.ClientTasks.Update(ct);
                        db.SaveChanges();
                        return RedirectToAction("Info", "Admin", new { mes = mess, url="Tasks" });
                    }
                    else 
                    {
                        return RedirectToAction("Error", "Admin", new { mes = "Status of the task doesn't allow it to be canceled" });
                    }
                } catch
                {
                    return RedirectToAction("Error");
                }
            }
            return RedirectToAction("Error");
        }

        private bool DeleteTask(long? task_id)
        {
            bool res = false;
            try
            {
                int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                ClientTask ct = db.ClientTasks.First(s => s.id == task_id && ((s.customer_id == customer_id) || (role == "admin")));
                if ((ct.status == "canceled") || (ct.status == "completed"))
                {

                    if (ct.JSON_File_source_files != null) //Deleting related data items
                    {
                        TaskFiles tf = JsonConvert.DeserializeObject<TaskFiles>(ct.JSON_File_source_files);
                        for (int i = 0; i < tf._files.Count; i++)
                        {
                            long did = tf._files[i].content_index;
                            DataItem da = db.DataItems.FirstOrDefault(sta => sta.id == did);

                            if (da.task_id.Contains(":")) //Multitask link
                            {
                                List<string> lst = new List<string>();
                                lst = da.task_id.Split(":").ToList(); //Remove last : and convert to list
                                lst.Remove(Convert.ToString(ct.id));
                                string sas = "";
                                for (int k = 0; k < lst.Count; k++)
                                {
                                    if (k == 0) sas = lst[k];
                                    else sas = sas + ":" + lst[k];
                                }
                                if (sas != "") //No dependent tasks left
                                {
                                    da.task_id = sas;
                                    //Remove related sn from dataitem.sn in case of sequence
                                    da.sn = da.sn.Replace(ct.sn, "");
                                    if (da.sn.Contains("::")) da.sn = da.sn.Replace("::", ":");
                                    else
                                    {
                                        if (da.sn.Substring(0, 1) == ":") da.sn = da.sn.Substring(1, da.sn.Length - 1);
                                        if (da.sn.Substring(da.sn.Length - 1, 1) == ":") da.sn = da.sn.Substring(0, da.sn.Length - 1);
                                    }
                                    db.DataItems.Update(da);
                                }
                                else
                                {
                                    db.DataItems.Remove(da);
                                }
                            }
                            else //Single task
                            {
                                db.DataItems.Remove(da);
                            }
                            
                        }
                    }
                    if (ct.result_file > 0) //Deleting related result dataitem
                    {
                        db.DataItems.Remove(db.DataItems.First(sta => sta.id == ct.result_file));
                    }
                    db.ClientTasks.Remove(ct);
                    db.SaveChanges();
                    Debug.WriteLine("Client task has been deleted: " + task_id);
                    res = true;
                    //return RedirectToAction("Info", "Admin", new { mes = "Task and related data items have been deleted" });
                }
            }
            catch
            {
                Debug.WriteLine("Error identifying task for deletion");
            }
            return res;
        }

        public async Task<IActionResult> DeleteAsync(long? task_id, long? data_id, string sn)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            try
            {
                if (task_id != null)
            {
                if (DeleteTask(task_id) == true)
                {
                    return RedirectToAction("Info", "Admin", new { mes = "Task and related data items have been deleted", url= "Tasks" });
                }
                else RedirectToAction("Error");
            } else if (data_id!=null)
            {
               
                    DataItem di = db.DataItems.First(da1 => da1.id == data_id && ((da1.customer_id == customer_id) || (role=="admin")));                      
                    if (di.task_id.Length <= 0)
                    {
                        db.DataItems.Remove(di);
                        db.SaveChanges();
                        Debug.WriteLine("Data item has been deleted: " + data_id);
                        return RedirectToAction("Info", "Admin", new { mes = "Data item has been deleted", url = "Data" });
                    }
                    else
                    {
                        return RedirectToAction("Error", "Admin", new { mes = "Data item is assigned to a task. You should delete the task first." });
                    }                
            } else if (sn!=null)
            {             
                    Client ct = db.Clients.First(s => (s.sn == sn) && ((s.customer_id == customer_id) || (role == "admin")));
                    List<ClientMetric> cm = db.ClientMetrics.Where(s => (s.sn == sn) && ((s.customer_id == customer_id) || (role == "admin"))).ToList();
                    List<ClientLog> cl = db.ClientLogs.Where(s => (s.sn == sn) && ((s.customer_id == customer_id) || (role == "admin"))).ToList();
                    List<ClientTask> cs = db.ClientTasks.Where(s => (s.sn == sn) && ((s.customer_id == customer_id) || (role == "admin"))).ToList();
                    List<DataItem> da1 = db.DataItems.Where(s => (s.sn.Contains(sn)) && ((s.customer_id == customer_id) || (role == "admin"))).ToList();
                    for (int k=0; k < da1.Count; k++)
                    {
                        if (da1[k].sn.Contains(":")) {
                            List<string> allsns = da1[k].sn.Split(":").ToList();
                            List<string> alltasks = da1[k].task_id.Split(":").ToList();
                            string newsn = "";
                            string newtaskid = "";
                            for (int n = 0; n < allsns.Count; n++)
                            {
                                if (allsns[n] == ct.sn)
                                {
                                    // do nothing, we are not copying this items
                                } else
                                {
                                    if (newsn=="")
                                    {
                                        newsn = allsns[n];
                                        newtaskid = alltasks[n];
                                    } else
                                    {
                                        newsn = newsn + ":" + allsns[n];
                                        newtaskid = newtaskid + ":" + alltasks[n];
                                    }
                                }
                            }

                            da1[k].sn = newsn;
                            da1[k].task_id = newtaskid;                            
                            db.DataItems.Update(da1[k]);
                        } else
                        {
                            db.DataItems.Remove(da1[k]);
                        }
                    }
                    //Remove trusted thumbprint
                    TrustedList tl = new TrustedList();
                    tl.RemoveItem(ct.public_key);
                    //Delete device files/folder
                    string targetPath = FilePath.device_path + ct.sn;
                    string targetFile = FilePath.device_path + ct.sn+".zip";
                    if (Directory.Exists(targetPath) == true) Directory.Delete(targetPath, true);
                    if (System.IO.File.Exists(targetFile) == true) System.IO.File.Delete(targetFile);
                    if (cl.Count>0) db.ClientLogs.RemoveRange(cl);
                    if (cm.Count > 0) db.ClientMetrics.RemoveRange(cm);
                    if (cs.Count > 0) db.ClientTasks.RemoveRange(cs);
                    db.Clients.Remove(ct);
                    await db.SaveChangesAsync();                    
                    return RedirectToAction("Info", "Admin", new { mes = "Client and its related data were deleted.", url = "Clients" });
                }
            }
            catch
            {
                Debug.WriteLine("Error searching data item in DB");
            }
            return RedirectToAction("Error", "Admin", new { mes = "Couldn't delete specified item" });

        }

        public IActionResult Data(long? id)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            if (id==null)
            {
                ViewData["logo"] = "Data";
                if (role == "admin")
                {
                    return View(db.DataItems.ToList());
                } else
                {
                    return View(db.DataItems.Where(s => s.customer_id==customer_id).ToList());
                }               
            } else
            {
                try
                {
                    DataItem di = new DataItem();
                    if (role == "admin")
                    {
                        di = db.DataItems.First(s => s.id == id);
                    }
                    else
                    {
                        di = db.DataItems.First(s => s.id == id && s.customer_id==customer_id);
                    }
                    return File(di.content, "application/force-download", di.file_name);
                } catch
                {
                    Debug.WriteLine("Data Method -> Couldn't find or download data!");
                    return RedirectToAction("Error", "Admin", new { mes = "Couldn't get specified file" });
                }
            }
        }

            public IActionResult Log(long? id, string? sn, long? taskid)
        {
            ViewData["logo"] = "Logs";
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            try
            {
                if (role == "admin")
                {
                    if ((id == null) && (sn == null) && (taskid == null))
                    {
                        ViewData["logo"] = "All logs";
                        return View(db.ClientLogs.ToList());
                    }
                    if ((id != null) && (sn == null))
                    {
                        ViewData["logo"] = "Selected log";
                        return View(db.ClientLogs.Where(s => s.id == id).ToList());
                    }
                    if ((id == null) && (sn != null))
                    {
                        ViewData["logo"] = "Logs for \"" + sn + "\"";
                        return View(db.ClientLogs.Where(s => s.sn == sn).ToList());
                    }
                    if ((id != null) && (sn != null))
                    {
                        ViewData["logo"] = "Selected log(s)";
                        return View(db.ClientLogs.Where(s => s.id == id && s.sn == sn).ToList()); ;
                    }
                    if (taskid != null)
                    {
                        ViewData["logo"] = "Selected log(s)";
                        return View(db.ClientLogs.Where(s => s.clienttask_id == taskid).ToList()); ;
                    }
                } else
                {
                    if ((id == null) && (sn == null) && (taskid == null))
                    {
                        ViewData["logo"] = "All logs";
                        return View(db.ClientLogs.Where(s => s.customer_id==customer_id).ToList());
                    }
                    if ((id != null) && (sn == null))
                    {
                        ViewData["logo"] = "Selected log";
                        return View(db.ClientLogs.Where(s => s.id == id && s.customer_id == customer_id).ToList());
                    }
                    if ((id == null) && (sn != null))
                    {
                        ViewData["logo"] = "Logs for \"" + sn + "\"";
                        return View(db.ClientLogs.Where(s => s.sn == sn && s.customer_id == customer_id).ToList());
                    }
                    if ((id != null) && (sn != null))
                    {
                        ViewData["logo"] = "Selected log(s)";
                        return View(db.ClientLogs.Where(s => s.id == id && s.sn == sn && s.customer_id == customer_id).ToList()); ;
                    }
                    if (taskid != null)
                    {
                        ViewData["logo"] = "Selected log(s)";
                        return View(db.ClientLogs.Where(s => s.clienttask_id == taskid && s.customer_id == customer_id).ToList()); ;
                    }
                }
            } catch
            {
                Debug.WriteLine("ERROR: Could find logs with provided parameters");
            }
            return RedirectToAction("Error");
        }

        public IActionResult Metrics(long? id, string sn)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            try
            {
                ViewData["graph"] = "false";
                ViewData["logo"] = "Metrics";
                if (id != null)
                {
                    ViewData["logo"] = "Selected metric";
                    if (role == "admin")
                    {
                        return View(db.ClientMetrics.Where(s => s.id == id).ToList());
                    }
                    else {
                        return View(db.ClientMetrics.Where(s => s.id == id && s.customer_id==customer_id).ToList());
                    }
                }
                if (sn != null)
                {
                    ViewData["graph"] = "true";
                    ViewData["logo"] = "Metrics of device \""+sn+"\"";
                    if (role == "admin")
                    {
                        return View(db.ClientMetrics.Where(s => s.sn == sn).ToList());
                    }
                    else
                    {
                        return View(db.ClientMetrics.Where(s => s.sn == sn && s.customer_id==customer_id).ToList());
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error while searching metrics in the DataBase.");
                return RedirectToAction("Error", "Admin", new { mes = "Couldn't find requested data in the DataBase." });
            }
            if (role == "admin")
            {
                return View(db.ClientMetrics.ToList());
            }
            else
            {
                return View(db.ClientMetrics.Where(s => s.customer_id==customer_id).ToList());
            }
        }

        public IActionResult Tasks(string? sn, long? id)
        {
            if (sn != null)
            {
                ViewData["logo"] = "Tasks for \"" + sn + "\"";
            }
            else
            {
                if (id != null)
                {
                    ViewData["logo"] = "Selected task(s)";
                }
                else
                {
                    ViewData["logo"] = "All tasks";
                }
            }
            List<ClientTask> cc = new List<ClientTask>();
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            List<ClientTask> clienttasks = new List<ClientTask>();
            if (role == "admin")
            {
                clienttasks = db.ClientTasks.ToList();
            } else
            {
                clienttasks = db.ClientTasks.Where(s => s.customer_id==customer_id).ToList();
            }
            foreach (var m in clienttasks)
            {
                if (((sn == null) || (m.sn == sn)) && ((id == null) || (m.id == id))) //Критерий фильтра
                {
                    ClientTask h = m;
                    string tasktype = "";
                    h._not_exported_hostname = db.Clients.FirstOrDefault(s => s.sn == h.sn).hostname;
                    if (m.JSON_Command_commands != null)
                    {
                        h._commands = JsonConvert.DeserializeObject<TaskCommands>(m.JSON_Command_commands);
                        tasktype = "Execute command";
                    }
                    if (m.JSON_File_source_files != null)
                    {
                        h._files = JsonConvert.DeserializeObject<TaskFiles>(m.JSON_File_source_files);
                        if (tasktype != "") tasktype = tasktype + ", push file(s)";
                        else tasktype = "Push file(s)";
                    }
                    if (m.json_params != null)
                    {
                        string g = m.json_params.Substring(m.json_params.IndexOf("\"settings\",") + 11);
                        g = g.Substring(0, g.Length - 1);
                        h._json_settings_simple = g;
                        tasktype = "Change settings";
                    }
                    if (m.direction == 2) tasktype = "Pull file";
                    h._type_simple = tasktype;
                    cc.Add(h);
                }
            }

            return View(cc);
        }

        [HttpGet]
        public IActionResult EditClient(string? id)
        {
            if (id != null)
            { 
                try 
                {
                    int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                    string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                    if (role == "admin")
                    {
                        ViewData["admin"] = true;
                        return View(db.Clients.First(s => s.sn == id));
                    }
                    else
                    {
                        ViewData["admin"] = false;
                        return View(db.Clients.FirstOrDefault(s => s.sn == id && s.customer_id==customer_id));
                    }                  
                }
                catch
                {
                    return RedirectToAction("Error");
                }
            } else
            {
                    return RedirectToAction("Error");
            }

        }
        class Response_Settings
        {
            public string action = ""; //command > settings
            public string servapi_host;
            public string servapi_port;
            public string provisioned;
            public string ssh_enabled;
            public string ssh_command;
            public string client_update_freq; //shoule have crontab format like "0 0 0 0 0  python /root/client.py"
            public string program_update_freq; //shoule have crontab format like "0 0 0 0 0 ./root/program"
            public string client_version;
            public string program_dir;
            public string id; //request id -> to figure out folder
        }
        [HttpPost]
        public IActionResult EditClient(string sn, string mac, string hostname, string model, string location, string servapi_host, string servapi_port,
            string provisioned, string ssh_enabled, int type, string ssh_command, string client_update_freq, string program_update_freq,
            string client_version, string program_dir)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;

            try
            {
                if ((sn != null) && (sn.Length>4) && (sn.Length<20) && (!sn.Contains(" ")) && (!sn.Contains(":")) && (!sn.Contains(" ")) && (hostname!=null) && (mac != null) && (model != null) && (location != null) && (servapi_host != null) && (servapi_port != null)
           && (provisioned != null) && (ssh_enabled != null) && (ssh_command != null) && (client_update_freq != null) && (program_update_freq != null)
                 && (client_version != null) && (program_dir != null)) {
                
                    Client cl = new Client();
                    if (role == "admin")
                    {
                        cl = db.Clients.FirstOrDefault(s => s.sn == sn);
                    } else {
                        cl = db.Clients.FirstOrDefault(s => s.sn == sn && s.customer_id==customer_id);
                    }
                    cl.hostname = hostname;
                    cl.location = location;
                    cl.mac = mac;
                    cl.model = model;
                    cl.type = type;
                    db.Clients.Update(cl);
                    db.SaveChanges();
                    if ((cl.program_dir != program_dir)
                    || (cl.program_update_freq != program_update_freq)
                    || (cl.provisioned != provisioned)
                    || (cl.servapi_host != servapi_host)
                    || (cl.servapi_port != servapi_port)
                    || (cl.ssh_command != ssh_command)
                    || (cl.ssh_enabled != ssh_enabled)
                    || (cl.client_update_freq != client_update_freq)
                    || (cl.client_version != client_version))
                    {
                        cl.program_dir = program_dir;
                        cl.program_update_freq = program_update_freq;
                        cl.provisioned = provisioned;
                        cl.servapi_host = servapi_host;
                        cl.servapi_port = servapi_port;
                        cl.ssh_command = ssh_command;
                        cl.ssh_enabled = ssh_enabled;
                        cl.client_update_freq = client_update_freq;
                        cl.client_version = client_version;

                        ClientTask ct = new ClientTask();
                        ct.client_log_id = 0;
                        ct.customer_id = cl.customer_id;
                        ct.direction = 1;
                        ct.sn = cl.sn;
                        ct.status = "pending";
                        ct.task_type = "settings";
                        ct.timestamp = DateTime.Now;
                        Response_Settings rs = new Response_Settings();
                        rs.action = "settings";
                        rs.client_update_freq = cl.client_update_freq;
                        rs.client_version = cl.client_version;
                        rs.program_dir = cl.program_dir;
                        rs.program_update_freq = cl.program_update_freq;
                        rs.provisioned = cl.provisioned;
                        rs.servapi_host = cl.servapi_host;
                        rs.servapi_port = cl.servapi_port;
                        rs.ssh_command = cl.ssh_command;
                        rs.ssh_enabled = cl.ssh_enabled;
                        db.ClientTasks.Add(ct);
                        db.SaveChanges();
                        rs.id = Convert.ToString(ct.id);
                        ct.json_params= JsonConvert.SerializeObject(rs);
                        db.ClientTasks.Update(ct);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Info", "Admin", new { mes = "Settings have been changed", url = "Client?id="+cl.sn });                
            }
            }
            catch
            {
                RedirectToAction("Error");
            }
            return RedirectToAction("Error");
        }


        [HttpGet]
        public IActionResult NewClient(int? template)
        {
            int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
            string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            Client ct = new Client();
            int? templ;
            if (template != null) templ = template;
            else templ = 0;
            if (role == "admin")
            {
                ViewData["admin"] = true;
                ct.customer_id = 1;
            } else
            {
                ViewData["admin"] = false;
                ct.customer_id = customer_id;
            }
            if (templ == 0)
            {
                //Linux client
                ViewData["template"] = 0;
                ct.servapi_host = "prod.edgeservapi.com";
                ct.servapi_port = "5001";
                ct.provisioned = "yes";
                ct.client_update_freq = "0 0 0 0 0 ./root/client";
                ct.client_version = "";
                ct.model = "";
                ct.program_dir = "/var/servapi";
                ct.program_update_freq = "0 0 0 0 0 ./root/program";
                ct.ssh_enabled = "no";
                ct.status = "provisioned";
                ct.ssh_command = "autossh";
                ct.type = 1;
            } else if (templ == 1)
            {
                //Openwrt client
                ViewData["template"] = 1;
                ct.servapi_host = "prod.edgeservapi.com";
                ct.servapi_port = "5001";
                ct.provisioned = "yes";
                ct.client_update_freq = "0 0 0 0 0 ./root/client";
                ct.client_version = "";
                ct.model = "";
                ct.program_dir = "/var/servapi";
                ct.program_update_freq = "0 0 0 0 0 ./root/program";
                ct.ssh_enabled = "no";
                ct.status = "provisioned";
                ct.ssh_command = "autossh";
                ct.type = 2;
            } else if (templ == 2)
            {
                //Windows client
                ViewData["template"] = 2;
                ct.servapi_host = "prod.edgeservapi.com";
                ct.servapi_port = "5001";
                ct.provisioned = "yes";
                ct.client_update_freq = "/sc minute /mo 1 /tr c:\\servapi\\edgeclient.exe /RL HIGHEST";
                ct.client_version = "";
                ct.model = "";
                ct.program_dir = "c:\\servapi";
                ct.program_update_freq = "";
                ct.ssh_enabled = "no";
                ct.status = "provisioned";
                ct.ssh_command = "autossh";
                ct.type = 3;
            }
            
            return View(ct);
        }

        private static void CreateZipFile(IEnumerable<FileInfo> files, string archiveName)
        {
            using (var stream = System.IO.File.OpenWrite(archiveName))
            using (ZipArchive archive = new ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var item in files)
                {
                    archive.CreateEntryFromFile(item.FullName, item.Name, CompressionLevel.Optimal);
                }
            }
        }

        [HttpPost]
        public IActionResult NewClient(Client client)
        {
            if (client != null)
            {
                try
                {
                    int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                    string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                    if ((role == "admin") || (client.customer_id == customer_id))
                    {
                        bool error = false;
                        client.sn = client.sn.ToLower();
                        if ((client.sn.Length < 5) || (client.sn.Length > 20) || (client.sn.Contains(" ")) || (client.sn.Contains(":"))) error = true;
                        if (client.mac.Length < 5) error = true;
                        if (client.location == "") error = true;
                        if (client.servapi_host == "") error = true;
                        if (client.servapi_host == "") error = true;
                        if (client.provisioned != "yes") error = true;
                        if (client.client_update_freq == null) client.client_update_freq = "";
                        if (client.client_version == null) client.client_version = "0";
                        if (client.model == "") error = true;
                        if (client.program_dir == "") error = true;
                        if (client.program_update_freq == null) client.program_update_freq = "";
                        if ((client.ssh_enabled != "no") && (client.ssh_enabled != "yes") && (client.ssh_enabled != "auto")) error = true;
                        if (client.status != "provisioned") error = true;
                        if (client.ssh_command == "") error = true;
                        if ((client.type < 0) && (client.type > 2)) error = true;

                        if (error == true)
                        {
                            return RedirectToAction("Error", "Admin", new { mes = "Some data in the form is missing or wrong format" });
                        }
                        error = false;
                        if (db.Clients.FirstOrDefault(s => s.sn.Contains(client.sn) || client.sn.Contains(s.sn)) != null) error = true;
                        if (db.Clients.FirstOrDefault(s => s.hostname.ToLower() == client.hostname.ToLower()) != null) error = true;
                        if (error == true)
                        {
                            return RedirectToAction("Error", "Admin", new { mes = "Hostname or device ID overlap with existing devices" });
                        }

                        //Create client directory
                        if (Directory.Exists(FilePath.device_path + client.sn) == false)
                        {
                            Directory.CreateDirectory(FilePath.device_path + client.sn);
                        }
                        //Writing settings
                        string[] lines = { "#SERVAPI host name/IP", client.servapi_host,
                            "#SERVAPI host port", client.servapi_port,
                            "#Provisioned?", client.provisioned,
                            "#SSH enabled? (yes/no/auto)", client.ssh_enabled,
                            "#SSH tunneling command", client.ssh_command,
                            "#HTTP client update frequency", client.client_update_freq,
                            "#Program update frequency", client.program_update_freq,
                            "#Client version", client.client_version,
                            "#Program dir", client.program_dir};
                        string filename = FilePath.device_path + client.sn + FilePath.path_separator + "settings.txt";
                        System.IO.File.WriteAllLines(filename, lines);

                        //Generating certificates

                        //var ecdsa = ECDsa.Create(); // generate asymmetric key pair
                        //var req = new CertificateRequest("CN="+client.sn+ ",OU=Edge Automation Framework", ecdsa, HashAlgorithmName.SHA256);
                        RSA rsa = RSA.Create(2048);
                        CertificateRequest req = new CertificateRequest(
                                "CN=" + client.sn + ",OU=Edge Automation Framework",
                                rsa,
                                HashAlgorithmName.SHA256,
                                RSASignaturePadding.Pkcs1);

                        var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
                        client.public_key = cert.Thumbprint;
                        // Create PFX (PKCS #12) with private key
                        //File.WriteAllBytes("c:\\temp\\mycert.pfx", cert.Export(X509ContentType.Pfx, "P@55w0rd"));

                        // Create Base 64 encoded CER (public key only)
                        System.IO.File.WriteAllText(FilePath.device_path + client.sn + FilePath.path_separator + "clientcert.pem",
                            "-----BEGIN CERTIFICATE-----\r\n"
                            + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                            + "\r\n-----END CERTIFICATE-----");
                        System.IO.File.WriteAllText(FilePath.device_path + client.sn + FilePath.path_separator + "clientkey.pem",
                            "-----BEGIN RSA PRIVATE KEY-----\r\n"
                            + Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks)
                            + "\r\n-----END RSA PRIVATE KEY-----");

                        //We skip SSH keys part. It's supposed that customer should do it manually.

                        //Copy templates
                        string sourcePath = FilePath.template_path + Convert.ToString(client.type);
                        string targetPath = FilePath.device_path + client.sn;
                        foreach (var srcPath in Directory.GetFiles(sourcePath))
                        {
                            //Copy the file from sourcepath and place into mentioned target path, 
                            //Overwrite the file if same file is exist in target path
                            System.IO.File.Copy(srcPath, srcPath.Replace(sourcePath, targetPath), true);
                        }

                        //Creating archive
                        List<FileInfo> fi = new List<FileInfo>();
                        foreach (var _file in Directory.GetFiles(targetPath))
                        {
                            fi.Add(new FileInfo(_file));
                        }

                        CreateZipFile(fi, FilePath.device_path + client.sn + ".zip");

                        db.Clients.Add(client);
                        db.SaveChanges();
                        TrustedList tl = new TrustedList();
                        tl.AddItem(client.public_key);
                        return RedirectToAction("Info", "Admin", new { mes = "New client has been provisioned. Device ID="+
                            client.sn+". You may want to create SSH key pair to use SSH tunnelling from the client to some SSH server (currently it's not provisioned automatically). Below you can find a link to a zip-archive with client files which you need to extract on the device (recommended to use "+client.program_dir+" directory). Please follow provisioning instructions from readme.txt file.",
                            url = "Client?id="+client.sn, link = "GetFile?sn="+client.sn });
                    }
                }
                catch
                {
                    Debug.WriteLine("Error while processing new client params");
                }
            }
            return null;
        }

        [HttpGet]
        public IActionResult Client(string? id)
        {
            if (id != null)
            {
                int customer_id = Convert.ToInt32(User.FindFirst(x => x.Type == ClaimsIdentity.DefaultIssuer).Value);
                string role = User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
                try
                {
                    ViewData["admin"] = false;
                    ClientCollection cc = new ClientCollection();
                    cc.myclient = new Client();
                    if (role == "admin")
                    {
                        ViewData["admin"] = true;
                        cc.myclient = db.Clients.First(s => s.sn == id);
                    } else
                    {
                        cc.myclient = db.Clients.First(s => s.sn == id && s.customer_id==customer_id);
                    }                    
                    cc.mymetrics = db.ClientMetrics.Where(s => s.sn == id).ToList();
                    cc.mylog = db.ClientLogs.Where(s => s.sn == id).ToList();
                    cc.mytasks = new List<ClientTask>();

                    foreach (var m in db.ClientTasks.Where(s => s.sn == id).ToList())
                    {
                        ClientTask h = m;
                        string tasktype = "";
                        if (m.JSON_Command_commands != null)
                        {
                            h._commands = JsonConvert.DeserializeObject<TaskCommands>(m.JSON_Command_commands);
                            tasktype = "Execute command";
                        }
                        if (m.JSON_File_source_files != null)
                        {
                            h._files = JsonConvert.DeserializeObject<TaskFiles>(m.JSON_File_source_files);
                            if (tasktype != "") tasktype = tasktype + ", push file(s)";
                            else tasktype = "Push file(s)";
                        }
                        if (m.json_params != null)
                        {
                            string g = m.json_params.Substring(m.json_params.IndexOf("\"settings\",") + 11);
                            g = g.Substring(0, g.Length - 1);
                            h._json_settings_simple = g;
                            tasktype = "Change settings";
                        }
                        if (m.direction == 2) tasktype = "Pull file";
                        h._type_simple = tasktype;
                        cc.mytasks.Add(h);


                    }

                    //cc.mytasks = db.ClientTasks.Where(s => s.sn == id).ToList();
                    return View(cc);
               } catch
                {
                    return RedirectToAction("Error");
                }
            } else
                {
                    return RedirectToAction("Error");
                }

        }

    }
}
