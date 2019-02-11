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
            groupList = GetAllFromCist();
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

        public static List<Group> GetAllFromCist(string url = Urls.CistAllKNGroupsSource)
        {
            List<Group> groups = new List<Group>();
            try
            {
                string timetableSelectionPage;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.CookieContainer = new CookieContainer();
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (Stream resStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(resStream, Encoding.GetEncoding("Windows-1251"));
                        timetableSelectionPage = reader.ReadToEnd();
                    }
                }

                foreach (string part in timetableSelectionPage.Split(new string[] { "IAS_ADD_Group_in_List(" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
                {
                    string[] groupInfo = part
                        .Remove(part.IndexOf(')'))
                        .Split(new char[] { ',', '\'' }, StringSplitOptions.RemoveEmptyEntries);

                    Group group = new Group
                    {
                        ID = int.Parse(groupInfo[1]),
                        Name = groupInfo[0]
                    };
                    groups.Add(group);
                }
                
                Serialisation.ToJsonFile(groups, FilePath.AllGroupsList);
            }
            catch (Exception ex)
            {
                groups = null;
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(Application.Current, MessageTypes.ExceptionOccurred, ex);
                });
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
            Serialisation.ToJsonFile(savedGroups, FilePath.SavedGroupsList);
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(Application.Current, MessageTypes.SavedGroupsChanged, savedGroups);
            });
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
