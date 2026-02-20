using AutomationShell48.Core.MVVM;

namespace AutomationShell48.UI.Features.Users.Models
{
    /// <summary>
    /// Represents a single user record displayed and edited on the Users page.
    /// Inherits ObservableObject so list rows update immediately after edits.
    /// </summary>
    public class User : ObservableObject
    {
        private int _userId;
        private string _firstName;
        private string _lastName;
        private string _location;
        private string _email;
        private string _phoneNumber;

        /// <summary>
        /// Primary identifier used by this demo CRUD feature.
        /// </summary>
        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        /// <summary>
        /// User first name.
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    OnPropertyChanged(nameof(DisplayLabel));
                }
            }
        }

        /// <summary>
        /// User last name.
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    OnPropertyChanged(nameof(DisplayLabel));
                }
            }
        }

        /// <summary>
        /// Two-letter state code for the user location.
        /// </summary>
        public string Location
        {
            get => _location;
            set
            {
                if (SetProperty(ref _location, value))
                {
                    OnPropertyChanged(nameof(DisplayLabel));
                }
            }
        }

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        /// <summary>
        /// User phone number.
        /// </summary>
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        /// <summary>
        /// Friendly display format used by the list panel.
        /// </summary>
        public string DisplayLabel => LastName + ", " + FirstName + " (" + Location + ")";
    }
}
