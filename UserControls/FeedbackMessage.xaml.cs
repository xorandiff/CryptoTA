using System.Windows;
using System.Windows.Controls;

namespace CryptoTA.UserControls
{
    public enum MessageType
    {
        CredentialsMissing,
        CredentialsInvalid
    }

    public partial class FeedbackMessage : UserControl
    {
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            "Type", typeof(MessageType),
            typeof(UserControl)
        );

        public MessageType Type
        {
            get => (MessageType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public string Header { get; set; }
        public string Text { get; set; }

        public FeedbackMessage(MessageType messageType)
        {
            InitializeComponent();
            DataContext = this;

            Type = messageType;
            switch (Type)
            {
                case MessageType.CredentialsMissing: 
                    Header = "Credentials Missing";
                    Text = "You have to provide credentials for this API.\nYou can add them in account settings.";
                    break;
                case MessageType.CredentialsInvalid:
                    Header = "Credentials Invaild";
                    Text = "Provided credentials are invalid for this API.\n You can edit them in account settings.";
                    break;
                default:
                    Header = "Unknown Error";
                    Text = "Unknown error occured.";
                    break;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            new AccountsWindow().Show();
        }
    }
}
