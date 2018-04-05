using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace MassEditPermissions
{
    public static class Methods
    {
        private static HashSet<string> GetGroupNames(IRSAPIClient proxy, List<int> artifactIds)
        {
            var names = new HashSet<string>();
            foreach (int artifactId in artifactIds)
            {
                // query for the name
                ResultSet<Group> resultSet = proxy.Repositories.Group.Read(artifactIds);
                if (resultSet.Success)
                {
                    foreach (var result in resultSet.Results)
                    {
                        names.Add(result.Artifact.Name);
                    }
                }
            }
            return names;
        }

        public static List<int> GetAllWorkspaceIds(IRSAPIClient proxy)
        {
            List<int> workspaceIds = new List<int>();
            proxy.APIOptions.WorkspaceID = -1;

            var query = new kCura.Relativity.Client.DTOs.Query<Workspace>
            {
                Condition = new kCura.Relativity.Client.WholeNumberCondition("Artifact ID", NumericConditionEnum.IsSet)
            };

            // should be less than 10000 results
            QueryResultSet<Workspace> resultSet = proxy.Repositories.Workspace.Query(query, 0);
            if (resultSet.Success)
            {
                workspaceIds.AddRange(resultSet.Results.Select(result => result.Artifact.ArtifactID));
            }

            return workspaceIds;
        }

        public static async Task<bool> DisableAddDocInWorkspaceForGroups(
            IPermissionManager mgr,
            IRSAPIClient rsapi,
            int workspaceId,
            List<int> groupIds)
        {
            // get group selector for workspace
            GroupSelector sel;
            try
            {
                sel = await mgr.GetWorkspaceGroupSelectorAsync(workspaceId);
            }
            catch (Exception)
            {
                return false;
            }

            // get group names
            HashSet<string> groupNames = GetGroupNames(rsapi, groupIds);

            foreach (GroupRef group in sel.EnabledGroups)
            {
                // see if the group is one whose permissions
                // we would like to disable
                if (groupNames.Contains(group.Name))
                {
                    GroupPermissions permissions = await mgr.GetWorkspaceGroupPermissionsAsync(workspaceId, group);
                    // get object permissions
                    List<ObjectPermission> objPermissions = permissions.ObjectPermissions;

                    // get the document permission
                    ObjectPermission docPerm = objPermissions.FirstOrDefault(x => x.Name == "Document");
                    if (docPerm != null && docPerm.AddSelected)
                    {
                        // disable Add permission
                        docPerm.AddSelected = false;
                    }
                    
                    // make changes
                    try
                    {
                        await mgr.SetWorkspaceGroupPermissionsAsync(workspaceId, permissions);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        ///// <summary>
        ///// Disables the "add document" permission for specified workspaces and group combinations
        ///// </summary>
        ///// <param name="mgr">Permission manager service</param>
        ///// <param name="workspaceIds">The list of workspace IDs that we want to do this for</param>
        ///// <param name="groupIds">The artifact IDs of the groups whose add document permission we want to revoke</param>
        ///// <returns></returns>
        //public static async Task<bool> DisableAddDocAsync(IPermissionManager mgr, ISet<int> workspaceIds, ISet<int> groupIds)
        //{
        //    bool success = false;

        //    foreach (int workspaceId in workspaceIds)
        //    {
        //        success = await DisableAddDocInWorkspaceForGroups(mgr, workspaceId, groupIds);
        //        // if we failed, break
        //        if (!success)
        //        {
        //            break;
        //        }
        //    }

        //    return success;
        //}
    }
}
