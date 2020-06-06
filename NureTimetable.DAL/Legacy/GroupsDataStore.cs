using NureTimetable.DAL.Legacy.Models;
using NureTimetable.DAL.Legacy.Models.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NureTimetable.DAL.Legacy
{
    [Obsolete("", true)]
    static class GroupsDataStore
    {
        public static List<Group> GetAllFromCist()
        {
            //if (SettingsRepository.CheckCistAllGroupsUpdateRights() == false)
            //{
            //    return null;
            //}

            List<Group> groups = new List<Group>();
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.GetEncoding("Windows-1251");
                try
                {
                    List<int> branches = new List<int>();

                    // Getting branches
                    Uri uri = new Uri(Urls.CistAllGroupsSource(null));
                    string branchesListPage = client.DownloadString(uri);
                    foreach (string part in branchesListPage.Split(new string[] { "IAS_Change_Groups(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                    {
                        string branchIdStr = part.Remove(part.IndexOf(')'));
                        if (!int.TryParse(branchIdStr, out int branchID))
                        {
                            continue;
                        }
                        branches.Add(branchID);
                    }

                    //Getting groups
                    foreach (int branchID in branches)
                    {
                        uri = new Uri(Urls.CistAllGroupsSource(branchID));
                        string branchGroupsPage = client.DownloadString(uri);
                        foreach (string part in branchGroupsPage.Split(new string[] { "IAS_ADD_Group_in_List(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                        {
                            string[] groupInfo = part
                                .Remove(part.IndexOf(")\">"))
                                .Split(new char[] { ',', '\'' }, StringSplitOptions.RemoveEmptyEntries);

                            if (groupInfo.Length < 2 || !int.TryParse(groupInfo.Last(), out int groupID))
                            {
                                continue;
                            }

                            for (int i = 0; i < groupInfo.Length - 1; i++)
                            {
                                Group group = new Group
                                {
                                    ID = groupID,
                                    Name = groupInfo[i]
                                };
                                groups.Add(group);
                            }
                        }
                    }

                    //Serialisation.ToJsonFile(groups, FilePath.AllGroupsList);
                    //SettingsRepository.UpdateCistAllGroupsUpdateTime();
                }
                catch (Exception)
                {
                    //MainThread.BeginInvokeOnMainThread(() =>
                    //{
                    //    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    //});
                    groups = null;
                }
            }
            return groups;
        }
    }
}
