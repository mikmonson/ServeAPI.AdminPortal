using AdminPortal.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace SERVAPI
{
    public class TrustedList
    {
        private List<string> trustedcerts = null;
        private static string path = FilePath.cert_path + "trustedcerts.txt";
        public TrustedList()
        {
            ReadList();
        }

        public void ReadList()
        {
            trustedcerts = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        trustedcerts.Add(reader.ReadLine());
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("ERROR!!! Can't load trusted list!!!");
            }
        }

        public void AddItem(string st)
        {
            trustedcerts.Add(st);
            SaveFile();
        }
        public void RemoveItem(string st)
        {
            trustedcerts.Remove(st);
        }
        public void SaveFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    foreach (string line in trustedcerts)
                        writer.WriteLine(line);
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("ERROR!!! Can't save trusted list!!!");
            }
        }

        public bool IsTrusted(string s)
        {
            if (trustedcerts.IndexOf(s) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
