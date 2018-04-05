using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.Services.Permission;
using Relativity.Services.ServiceProxy;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // introduction
            const string groupsFileName = "groups.txt";
            Console.WriteLine("This console app will disable the 'add document' permission for a list of groups.");
            Console.WriteLine($"Please ensure that there is a text file named '{groupsFileName}'.");
            Console.WriteLine("Each line in the file should indicate a group's Artifact ID");
            string url = String.Empty;
            string user = String.Empty;
            string pw = String.Empty;
            bool successfulLogin = false;
            while (!successfulLogin)
            {
                successfulLogin = ReadUserInput(out url, out user, out pw);
                if (!successfulLogin)
                {
                    Console.WriteLine("Failed to login! Try again.");
                    Console.WriteLine("---");
                }
            }
            var connHelper = new ConnectionHelper(url, user, pw);

            string currDir = Environment.CurrentDirectory;          
            string groupsFilePath = currDir + @"\" + groupsFileName;
            string[] groupsIdsStr;
            try
            {
                groupsIdsStr = File.ReadAllLines(groupsFilePath);
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read file at {groupsFilePath}.");
                Console.WriteLine($"Ensure that {groupsFileName} exists in the same folder as the executable.");
                return;
            }        

            var groupIds = new List<int>();
            foreach (var groupStr in groupsIdsStr)
            {
                int groupId = Int32.Parse(groupStr);
                groupIds.Add(groupId);
            }

            // get workspace IDs
            List<int> workspaceIds;

            // instantiate IRSAPIClient
            using (IRSAPIClient proxy = connHelper.GetRsapiClient())
            {
                workspaceIds = MassEditPermissions.Methods.GetAllWorkspaceIds(proxy);
            }

            if (workspaceIds.Count > 0)
            {
                // instantiate IPermissionManager
                ServiceFactory serviceFactory = connHelper.GetServiceFactory();
                using (IPermissionManager mgr = serviceFactory.CreateProxy<IPermissionManager>())
                using (IRSAPIClient rsapi = connHelper.GetRsapiClient())
                {
                    int successCount = 0;
                    foreach (int workspaceId in workspaceIds)
                    {
                        bool success = false;
                        Task.Run(async () =>
                        {
                            success = await MassEditPermissions.Methods.DisableAddDocInWorkspaceForGroups(mgr, rsapi, workspaceId, groupIds);
                        }).Wait();
                        if (success)
                        {
                            successCount++;
                            Console.WriteLine($"Successfully updated {successCount} of {workspaceIds.Count} workspaces. ({workspaceId})");
                        }
                    }
                }
            }

            Pause();
        }


        /// <summary>
        /// Reads in and validates the user credentials by attempting to log in
        /// </summary>
        /// <param name="url"></param>
        /// <param name="user"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        private static bool ReadUserInput(out string url, out string user, out string pw)
        {
            Console.WriteLine("Please enter your Relativity instance URL (e.g. https://my-instance.com).");
            url = Console.ReadLine();
            Console.WriteLine("Please enter your Relativity username (e.g. albert.einstein@relativity.com).");
            user = Console.ReadLine();
            Console.WriteLine("Please enter your Relativity password. The cursor will not move.");
            StringBuilder pwBuilder = new StringBuilder();
            // hide password
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    // remove last element
                    pwBuilder.Remove(pwBuilder.Length - 1, 1);                    
                }
                else
                {
                    pwBuilder.Append(key.KeyChar);
                }
            }
            pw = pwBuilder.ToString();
            
            var connHelper = new ConnectionHelper(url, user, pw);
            return connHelper.TestLogin();
        }


        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
