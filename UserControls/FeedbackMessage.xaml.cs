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
                default:
                    Header = "Unknown Error";
                    Text = "Unknown error occured.";
                    break;
            }
        }

        public FeedbackMessage(string krakenApiErrorMessage)
        {
            InitializeComponent();
            DataContext = this;

            switch (krakenApiErrorMessage)
            {
                case "EGeneral:Invalid arguments":
                    Header = "Kraken API General Error";
                    Text = "The request payload is malformed, incorrect or ambiguous.";
                    break;
                case "EGeneral:Invalid arguments:Index unavailable":
                    Header = "Kraken API General Error";
                    Text = "Index pricing is unavailable for stop/profit orders on this pair.";
                    break;
                case "EService:Unavailable":
                    Header = "Kraken API General Error";
                    Text = "The matching engine or API is offline";
                    break;
                case "EService:Market in cancel_only mode":
                    Header = "Kraken API Service Error";
                    Text = "Request can't be made at this time.";
                    break;
                case "EService:Market in post_only mode":
                    Header = "Kraken API Service Error";
                    Text = "Request can't be made at this time.";
                    break;
                case "EService:Deadline elapsed":
                    Header = "Kraken API Service Error";
                    Text = "The request timed out according to the default or specified deadline.";
                    break;
                case "EAPI:Invalid key":
                    Header = "Kraken API Authentication Error";
                    Text = "An invalid API-Key header was supplied.\nYou can change it in account settings under the name 'Public key'.";
                    break;
                case "EAPI:Invalid signature":
                    Header = "Kraken API Authentication Error";
                    Text = "An invalid API-Sign header was supplied.\nPrivate key may be invalid, you can change it in account settings.";
                    break;
                case "EAPI:Invalid nonce":
                    Header = "Kraken API Authentication Error";
                    Text = "An invalid nonce was supplied.\nMake sure nonce window for your API is big enough to fit UNIX timestamp.";
                    break;
                case "EGeneral:Permission denied":
                    Header = "Kraken API General Error";
                    Text = "API key doesn't have permission to make this request.\nCheck permissions in your Kraken account.";
                    break;
                case "EOrder:Cannot open position":
                    Header = "Kraken API Order Error";
                    Text = "User/tier is ineligible for margin trading.";
                    break;
                case "EOrder:Margin allowance exceeded":
                    Header = "Kraken API Order Error";
                    Text = "User has exceeded their margin allowance.";
                    break;
                case "EOrder:Margin level too low":
                    Header = "Kraken API Order Error";
                    Text = "Client has insufficient equity or collateral.";
                    break;
                case "EOrder:Margin position size exceeded":
                    Header = "Kraken API Order Error";
                    Text = "Client would exceed the maximum position size for this pair.";
                    break;
                case "EOrder:Insufficient margin":
                    Header = "Kraken API Order Error";
                    Text = "Exchange does not have available funds for this margin trade.";
                    break;
                case "EOrder:Insufficient funds":
                    Header = "Kraken API Order Error";
                    Text = "Client does not have the necessary funds.";
                    break;
                case "EOrder:Order minimum not met":
                    Header = "Kraken API Order Error";
                    Text = "Order size does not meet ordermin.";
                    break;
                case "EOrder:Orders limit exceeded":
                    Header = "Kraken API Order Error";
                    Text = "Number of open orders in a given pair is exceeded.";
                    break;
                case "EOrder:Rate limit exceeded":
                    Header = "Kraken API Order Error";
                    Text = "User's max ratecount is exceeded for a given pair.";
                    break;
                case "EOrder:Positions limit exceeded":
                    Header = "Kraken API Order Error";
                    Text = "User's max ratecount is exceeded for a given pair.";
                    break;
                case "EOrder:Unknown position":
                    Header = "Kraken API Order Error";
                    Text = "Requested position is unknown.";
                    break;
                case "EAPI:Rate limit exceeded":
                    Header = "Kraken API Authentication Error";
                    Text = "API calls rate limit is exceeded.";
                    break;
                default:
                    Header = "Kraken API General Error";
                    Text = "Unknown error occured.";
                    break;
            }
        }
    }
}
