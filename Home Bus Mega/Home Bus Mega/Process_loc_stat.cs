using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Bus_Mega
{
    class Process_loc_stat
    {
        const string file_path = "user_data/user_data.loc_stat";
        public string ReadFile()
        {
            string result = "";
            var linesRead = File.ReadLines(file_path);
            foreach (var lineRead in linesRead)
            {
                Debug.WriteLine(lineRead);
                result += lineRead + "\n";
            }

            return result;
        }

        public string Read_Data(string key_name)
        {
            var linesRead = File.ReadLines(file_path);
            foreach (var lineRead in linesRead)
            {
                string[] data = lineRead.Split(" = ");
                if (data[0] == key_name)
                {
                    return data[1];
                }
            }

            return "";
        }

        public void Write_Data(string key_name, string value)
        {
            string[] fileRead = File.ReadAllLines(file_path);

            int i = 0;
            foreach (var lineRead in fileRead)
            {
                string[] data = lineRead.Split(" = ");
                if (data[0] == key_name)
                {
                    string result = key_name + " = " + value;
                    fileRead[i] = result;
                    File.WriteAllLines(file_path, fileRead);
                    break;
                }
                //-
                i++;
            }
        }
    }
}
