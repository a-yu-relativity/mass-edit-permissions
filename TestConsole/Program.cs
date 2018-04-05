using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string currDir = Environment.CurrentDirectory;
            string groupsFilePath = "";
            string[] groupsIdsStr = File.ReadAllLines(groupsFilePath);

            // store in hash set for faster inclusion testing
            HashSet<int> groupIds = new HashSet<int>();
            foreach (var groupStr in groupsIdsStr)
            {
                int groupId = Int32.Parse(groupStr);
                groupIds.Add(groupId);
            }


        }
    }
}
