using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace CryptoTA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            VisualStudio2019Palette.Palette.AccentColor = (Color)ColorConverter.ConvertFromString("#FFB10000");
            VisualStudio2019Palette.Palette.AccentMainColor = (Color)ColorConverter.ConvertFromString("#6B720404");
            VisualStudio2019Palette.Palette.AccentDarkColor = (Color)ColorConverter.ConvertFromString("#FFB90000");
            VisualStudio2019Palette.Palette.AccentSecondaryDarkColor = (Color)ColorConverter.ConvertFromString("#87BC0000");
            VisualStudio2019Palette.Palette.AccentMouseOverColor = (Color)ColorConverter.ConvertFromString("#70FF4040");
            VisualStudio2019Palette.Palette.AccentFocusedColor = (Color)ColorConverter.ConvertFromString("#FFFF0000");
            VisualStudio2019Palette.Palette.AccentForegroundColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2019Palette.Palette.ValidationColor = (Color)ColorConverter.ConvertFromString("#FFFFB300");
            VisualStudio2019Palette.Palette.BasicColor = (Color)ColorConverter.ConvertFromString("#FF555555");
            VisualStudio2019Palette.Palette.SemiBasicColor = (Color)ColorConverter.ConvertFromString("#FF656565");
            VisualStudio2019Palette.Palette.PrimaryColor = (Color)ColorConverter.ConvertFromString("#FF262523");
            VisualStudio2019Palette.Palette.SecondaryColor = (Color)ColorConverter.ConvertFromString("#FF3F3F46");
            VisualStudio2019Palette.Palette.MarkerColor = (Color)ColorConverter.ConvertFromString("#FFF1F1F1");
            VisualStudio2019Palette.Palette.MarkerInvertedColor = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
            VisualStudio2019Palette.Palette.IconColor = (Color)ColorConverter.ConvertFromString("#FFF1F1F1");
            VisualStudio2019Palette.Palette.AlternativeColor = (Color)ColorConverter.ConvertFromString("#FF161616");
            VisualStudio2019Palette.Palette.MouseOverColor = (Color)ColorConverter.ConvertFromString("#FF403E3E");
            VisualStudio2019Palette.Palette.ComplementaryColor = (Color)ColorConverter.ConvertFromString("#FF2B2B2B");
            VisualStudio2019Palette.Palette.MainColor = (Color)ColorConverter.ConvertFromString("#FF212121");
            VisualStudio2019Palette.Palette.HeaderColor = (Color)ColorConverter.ConvertFromString("#93FF0000");
            VisualStudio2019Palette.Palette.DockingBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF171717");
            VisualStudio2019Palette.Palette.ReadOnlyBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF241717");
            VisualStudio2019Palette.Palette.ReadOnlyBorderColor = (Color)ColorConverter.ConvertFromString("#FF555555");
            VisualStudio2019Palette.Palette.DisabledOpacity = 0.3;
            VisualStudio2019Palette.Palette.ReadOnlyOpacity = 0.6;

            StyleManager.ApplicationTheme = new VisualStudio2019Theme();
            InitializeComponent();
        }
    }
}
