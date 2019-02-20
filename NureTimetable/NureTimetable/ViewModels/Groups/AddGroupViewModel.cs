using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NureTimetable.DAL;
using NureTimetable.Models;
using NureTimetable.Services.Helpers;
using NureTimetable.ViewModels.Core;
using Xamarin.Forms;

namespace NureTimetable.ViewModels.Groups
{
    public class AddGroupViewModel : BaseViewModel
    {

        List<Group> AllGroups;

        List<SavedGroup> SavedGroups;

        public ObservableCollection<Group> Groups { get => _groups; set => SetProperty(ref _groups, value); }

        private ObservableCollection<Group> _groups;

        public bool ProgressLayoutIsVisable
        {
            get => _progressLayoutIsVisable;
            set => SetProperty(ref _progressLayoutIsVisable, value);
        }

        public ICommand UpdateCommand { get; protected set; }

        public bool ProgressLayoutIsEnable
        {
            get => _progressLayoutIsEnable;
            set => SetProperty(ref _progressLayoutIsEnable, value);
        }

        private bool _progressLayoutIsVisable;
        private bool _progressLayoutIsEnable;

        public string SearchBarText { get => _searchBarText; set => SetProperty(ref _searchBarText, value); }

        private string _searchBarText;

        public ICommand SearchBarTextChangedCommand { get; protected set; }

        public ICommand ContentPageAppearingCommand { get; protected set; }


        public AddGroupViewModel(INavigation navigation) : base(navigation)
        {
            SearchBarTextChangedCommand = CommandHelper.CreateCommand(SearchBarTextChanged);
            ContentPageAppearingCommand = CommandHelper.CreateCommand(UpdateGroups);
            UpdateCommand = CommandHelper.CreateCommand(Update);
            Device.BeginInvokeOnMainThread((async () =>
            {
                await UpdateGroups(false);
            }));
        }


        public Group SelectedGroup
        {
            get => _selectedGroupt;
            set
            {
                if (value != null)
                {
                    Device.BeginInvokeOnMainThread(async () => { await GroupSelected(value); });
                    
                }

                _selectedGroupt = value;
            }
        }

        private Group _selectedGroupt;


        private async Task GroupSelected(Group group)
        {
            if (SavedGroups.Exists(g => g.ID == group.ID))
            {
                await App.Current.MainPage.DisplayAlert("Добавление группы", "Группа уже находится в сохранённых", "OK");
                return;
            }

            SavedGroups.Add(new SavedGroup(group));
            GroupsDataStore.UpdateSaved(SavedGroups);
            await App.Current.MainPage.DisplayAlert("Добавление группы", "Группа добавлена в сохранённые", "OK");
        }

        private async Task SearchBarTextChanged()
        {
            if (AllGroups == null) return;

            if (string.IsNullOrEmpty(SearchBarText))
            {
                Groups = new ObservableCollection<Group>(AllGroups.OrderBy(g => g.Name));
            }
            else
            {
                string searchQuery = SearchBarText.ToLower();
                Groups =
                    new ObservableCollection<Group>(
                        AllGroups
                            .Where(g => g.Name.ToLower().Contains(searchQuery) || g.Name.ToLower().Contains(searchQuery.Replace('и', 'і')) || g.ID.ToString() == searchQuery)
                            .OrderBy(g => g.Name)
                    );
            }
        }

        private async Task Update()
        {
            if (!ProgressLayoutIsVisable && await App.Current.MainPage.DisplayAlert("Загрузка списка групп",
                    "Вы уверенны, что хотите загрузить список групп из Cist?", "Да", "Отмена"))
            {
                await UpdateGroups(true);
            }
        }

        private async Task UpdateGroups()
        {
            await UpdateGroups(false);
        }
        private async Task UpdateGroups(bool fromCistOnly = false)
        {
            ProgressLayoutIsVisable = true;
            ProgressLayoutIsEnable = false;

            await Task.Factory.StartNew(() =>
            {
                if (fromCistOnly)
                {
                    AllGroups = GroupsDataStore.GetAllFromCist();
                }
                else
                {
                    AllGroups = GroupsDataStore.GetAll();
                }
                if (AllGroups == null)
                {
                    App.Current.MainPage.DisplayAlert("Загрузка списка групп", "Не удолось загрузить список групп. Пожалуйста, попробуйте позже.", "Ok");

                    ProgressLayoutIsVisable = false;
                    ProgressLayoutIsEnable = true;

                    return;
                }

                SavedGroups = GroupsDataStore.GetSaved();
                Groups = new ObservableCollection<Group>(AllGroups.OrderBy(g => g.Name));


                if (SearchBarTextChangedCommand.CanExecute(null))
                    SearchBarTextChangedCommand.Execute(null);

                ProgressLayoutIsVisable = false;
                ProgressLayoutIsEnable = true;

            });
        }

      
    }
}