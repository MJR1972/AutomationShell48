using System.Collections.ObjectModel;
using AutomationShell48.Core.MVVM;
using AutomationShell48.Core.Services;
using AutomationShell48.UI.Features.Users.Models;

namespace AutomationShell48.UI.Features.Users
{
    /// <summary>
    /// ViewModel for an example in-memory CRUD screen for User records.
    /// Uses form buffer properties so list items are only mutated when Update is clicked.
    /// </summary>
    public class UsersPageViewModel : BaseViewModel
    {
        private readonly ILogger _logger;
        private int _nextUserId = 1;
        private User _selectedUser;
        private string _formUserIdDisplay;
        private string _formFirstName;
        private string _formLastName;
        private string _formLocation;
        private string _formEmail;
        private string _formPhoneNumber;

        /// <summary>
        /// Initializes the Users CRUD page and seeds a few sample records.
        /// </summary>
        public UsersPageViewModel(ILogger logger)
        {
            _logger = logger;
            Title = "Users";

            // Core collections bound directly to the view.
            Users = new ObservableCollection<User>();
            States = BuildStates();

            // Commands for all CRUD actions plus form reset.
            AddUserCommand = new RelayCommand(AddUser, CanAddUser);
            UpdateUserCommand = new RelayCommand(UpdateUser, CanUpdateOrDeleteUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanUpdateOrDeleteUser);
            ClearFormCommand = new RelayCommand(ClearForm);

            // Optional seed data so the page is immediately interactive.
            SeedUsers();
            ClearForm();

            _logger?.Info("Users view loaded.");
        }

        /// <summary>
        /// In-memory user list shown in the right list panel.
        /// </summary>
        public ObservableCollection<User> Users { get; }

        /// <summary>
        /// List of two-letter US state codes for the Location dropdown.
        /// </summary>
        public ObservableCollection<string> States { get; }

        /// <summary>
        /// Current selection from the user list.
        /// Changing selection loads values into the form buffer.
        /// </summary>
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (!SetProperty(ref _selectedUser, value))
                {
                    return;
                }

                LoadSelectedUserIntoForm();
                UpdateUserCommand.RaiseCanExecuteChanged();
                DeleteUserCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Read-only display value for User ID in the form.
        /// Shows "(new)" when creating a record.
        /// </summary>
        public string FormUserIdDisplay
        {
            get => _formUserIdDisplay;
            set => SetProperty(ref _formUserIdDisplay, value);
        }

        /// <summary>
        /// Form buffer for first name.
        /// </summary>
        public string FormFirstName
        {
            get => _formFirstName;
            set
            {
                if (SetProperty(ref _formFirstName, value))
                {
                    AddUserCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Form buffer for last name.
        /// </summary>
        public string FormLastName
        {
            get => _formLastName;
            set
            {
                if (SetProperty(ref _formLastName, value))
                {
                    AddUserCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Form buffer for two-letter state code.
        /// </summary>
        public string FormLocation
        {
            get => _formLocation;
            set => SetProperty(ref _formLocation, value);
        }

        /// <summary>
        /// Form buffer for email.
        /// </summary>
        public string FormEmail
        {
            get => _formEmail;
            set => SetProperty(ref _formEmail, value);
        }

        /// <summary>
        /// Form buffer for phone number.
        /// </summary>
        public string FormPhoneNumber
        {
            get => _formPhoneNumber;
            set => SetProperty(ref _formPhoneNumber, value);
        }

        /// <summary>
        /// Creates a new user record from form values.
        /// </summary>
        public RelayCommand AddUserCommand { get; }

        /// <summary>
        /// Applies current form values to the selected record.
        /// </summary>
        public RelayCommand UpdateUserCommand { get; }

        /// <summary>
        /// Deletes the selected user from the list.
        /// </summary>
        public RelayCommand DeleteUserCommand { get; }

        /// <summary>
        /// Clears selection and resets the form for a new record.
        /// </summary>
        public RelayCommand ClearFormCommand { get; }

        /// <summary>
        /// Command predicate for Add action.
        /// Requires first and last name at minimum.
        /// </summary>
        private bool CanAddUser()
        {
            return !string.IsNullOrWhiteSpace(FormFirstName) && !string.IsNullOrWhiteSpace(FormLastName);
        }

        /// <summary>
        /// Command predicate shared by Update/Delete.
        /// </summary>
        private bool CanUpdateOrDeleteUser()
        {
            return SelectedUser != null;
        }

        /// <summary>
        /// Adds a new user with an incrementing integer ID.
        /// Newly added user becomes selected.
        /// </summary>
        private void AddUser()
        {
            var user = new User
            {
                UserId = _nextUserId++,
                FirstName = SafeText(FormFirstName),
                LastName = SafeText(FormLastName),
                Location = SafeText(FormLocation),
                Email = SafeText(FormEmail),
                PhoneNumber = SafeText(FormPhoneNumber)
            };

            Users.Add(user);
            SelectedUser = user;
            _logger?.Info("User added: " + user.DisplayLabel);
        }

        /// <summary>
        /// Copies form buffer values into the selected user.
        /// </summary>
        private void UpdateUser()
        {
            if (SelectedUser == null)
            {
                return;
            }

            SelectedUser.FirstName = SafeText(FormFirstName);
            SelectedUser.LastName = SafeText(FormLastName);
            SelectedUser.Location = SafeText(FormLocation);
            SelectedUser.Email = SafeText(FormEmail);
            SelectedUser.PhoneNumber = SafeText(FormPhoneNumber);
            _logger?.Info("User updated: " + SelectedUser.DisplayLabel);
        }

        /// <summary>
        /// Removes currently selected user and resets form state.
        /// </summary>
        private void DeleteUser()
        {
            if (SelectedUser == null)
            {
                return;
            }

            var deletedLabel = SelectedUser.DisplayLabel;
            Users.Remove(SelectedUser);
            SelectedUser = null;
            ClearForm();
            _logger?.Info("User deleted: " + deletedLabel);
        }

        /// <summary>
        /// Clears buffer values and selection so the form is ready for a new entry.
        /// </summary>
        private void ClearForm()
        {
            SelectedUser = null;
            FormUserIdDisplay = "(new)";
            FormFirstName = string.Empty;
            FormLastName = string.Empty;
            FormLocation = string.Empty;
            FormEmail = string.Empty;
            FormPhoneNumber = string.Empty;
        }

        /// <summary>
        /// Reads selected item values into the edit buffer.
        /// </summary>
        private void LoadSelectedUserIntoForm()
        {
            if (SelectedUser == null)
            {
                FormUserIdDisplay = "(new)";
                return;
            }

            FormUserIdDisplay = SelectedUser.UserId.ToString();
            FormFirstName = SelectedUser.FirstName;
            FormLastName = SelectedUser.LastName;
            FormLocation = SelectedUser.Location;
            FormEmail = SelectedUser.Email;
            FormPhoneNumber = SelectedUser.PhoneNumber;
        }

        /// <summary>
        /// Creates the fixed state code list for the location dropdown.
        /// </summary>
        private static ObservableCollection<string> BuildStates()
        {
            return new ObservableCollection<string>
            {
                string.Empty,
                "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA",
                "HI","ID","IL","IN","IA","KS","KY","LA","ME","MD",
                "MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ",
                "NM","NY","NC","ND","OH","OK","OR","PA","RI","SC",
                "SD","TN","TX","UT","VT","VA","WA","WV","WI","WY"
            };
        }

        /// <summary>
        /// Seeds a few records to demonstrate Update/Delete interactions.
        /// </summary>
        private void SeedUsers()
        {
            Users.Add(new User
            {
                UserId = _nextUserId++,
                FirstName = "Avery",
                LastName = "Jordan",
                Location = "TX",
                Email = "avery.jordan@example.com",
                PhoneNumber = "512-555-0101"
            });

            Users.Add(new User
            {
                UserId = _nextUserId++,
                FirstName = "Mia",
                LastName = "Patel",
                Location = "WA",
                Email = "mia.patel@example.com",
                PhoneNumber = "206-555-0142"
            });
        }

        /// <summary>
        /// Normalizes null user input to safe empty strings.
        /// </summary>
        private static string SafeText(string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}
