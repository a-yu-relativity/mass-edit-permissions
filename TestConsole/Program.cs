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
            string currDir = Environment.CurrentDirectory;
            string groupsFileName = "groups.txt";
            string groupsFilePath = currDir + @"\" + groupsFileName;
            string[] groupsIdsStr = File.ReadAllLines(groupsFilePath);

            // store in hash set for faster inclusion testing
            HashSet<int> groupIds = new HashSet<int>();
            foreach (var groupStr in groupsIdsStr)
            {
                int groupId = Int32.Parse(groupStr);
                groupIds.Add(groupId);
            }

            // get workspace IDs
            List<int> workspaceIds;

            // instantiate IRSAPIClient
            const string credsFile = @"C:\Creds\creds.txt";
            var connHelper = new ConnectionHelper(credsFile);
            using (IRSAPIClient proxy = connHelper.GetRsapiClient())
            {
                workspaceIds = MassEditPermissions.Methods.GetAllWorkspaceIds(proxy);
            }

            if (workspaceIds.Count > 0)
            {
                // instantiate IPermissionManager
                ServiceFactory serviceFactory = connHelper.GetServiceFactory();
                using (IPermissionManager mgr = serviceFactory.CreateProxy<IPermissionManager>())
                {
                    int successCount = 0;
                    foreach (int workspaceId in workspaceIds)
                    {
                        bool success = false;
                        Task.Run(async () =>
                        {
                            success = await MassEditPermissions.Methods.DisableAddDocInWorkspaceForGroups(mgr, workspaceId, groupIds);
                        }).Wait();
                        if (success)
                        {
                            successCount++;
                            Console.WriteLine($"Successfully updated {successCount} of {workspaceIds.Count} workspaces");
                        }
                    }
                }
            }

            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
