using NureTimetable.Helpers;
using NureTimetable.Models;
using NureTimetable.Models.Consts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;

namespace NureTimetable.DAL
{
    public static class GroupsDataStore
    {
        #region All Groups
        public static List<Group> GetAll()
        {
            List<Group> groupList;
            groupList = GetAllLocal();
            if (groupList != null && groupList.Count > 0)
            {
                return groupList;
            }
            groupList = GetAllFromCist(false);
            return groupList;
        }

        public static List<Group> GetAllLocal()
        {
            List<Group> loadedGroups;

            string filePath = FilePath.AllGroupsList;
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            loadedGroups = Serialisation.FromJsonFile<List<Group>>(filePath);
            return loadedGroups;
        }

        public static List<Group> GetAllFromCist()
            => GetAllFromCist(true);

        private static List<Group> GetAllFromCist(bool applyToCistRequestRestrictions)
        {
            if (applyToCistRequestRestrictions && SettingsDataStore.CheckGetDataFromCistRights() == false)
            {
                return null;
            }

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

                    Serialisation.ToJsonFile(groups, FilePath.AllGroupsList);

                    if (applyToCistRequestRestrictions)
                    {
                        SettingsDataStore.UpdateLastCistRequestTime();
                    }
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                    });
                    groups = null;
                }
            }
            return groups;
        }
        #endregion

        #region Saved Groups
        public static List<SavedGroup> GetSaved()
        {
            List<SavedGroup> loadedGroups = new List<SavedGroup>();

            string filePath = FilePath.SavedGroupsList;
            if (!File.Exists(filePath))
            {
                return loadedGroups;
            }
            
            loadedGroups = Serialisation.FromJsonFile<List<SavedGroup>>(filePath) ?? loadedGroups;
            return loadedGroups;
        }

        public static void UpdateSaved(List<SavedGroup> savedGroups)
        {
            // Removing cash from deleted saved groups if needed
            List<SavedGroup> deletedGroups = GetSaved()
                .Where(oldGroup => !savedGroups.Exists(group => group.ID == oldGroup.ID))
                .ToList();
            if (deletedGroups.Count > 0)
            {
                deletedGroups.ForEach((dg) =>
                {
                    try
                    {
                        File.Delete(FilePath.SavedTimetable(dg.ID));
                    }
                    catch {}
                });
            }
            // Saving saved groups list
            Serialisation.ToJsonFile(savedGroups, FilePath.SavedGroupsList);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SavedGroupsChanged, savedGroups);
            });
            // Updating selected group if needed
            if (GetSelected() == null && savedGroups.Count > 0)
            {
                UpdateSelected(savedGroups[0]);
            }
        }
        #endregion

        #region Selected Group
        public static Group GetSelected()
        {
            string filePath = FilePath.SelectedGroup;
            if (!File.Exists(filePath))
            {
                return null;
            }

            Group selectedGroup = Serialisation.FromJsonFile<Group>(filePath);
            return selectedGroup;
        }

        public static void UpdateSelected(SavedGroup selectedGroup)
        {
            Serialisation.ToJsonFile((Group)selectedGroup, FilePath.SelectedGroup);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SelectedGroupChanged, (Group)selectedGroup);
            });
        }
        #endregion
    }
}
