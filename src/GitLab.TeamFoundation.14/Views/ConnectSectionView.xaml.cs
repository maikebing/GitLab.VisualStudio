using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace GitLab.TeamFoundation.Views
{
    /// <summary>
    /// Interaction logic for ConnectSectionView.xaml
    /// </summary>
    public partial class ConnectSectionView : UserControl
    {
        static ConnectSectionView()
        {
            // Fix System.Windows.Interactivity not found issue
            System.Console.WriteLine(typeof(Interaction));
        }

        public ConnectSectionView()
        {
            InitializeComponent();
            OnThemeChanged();

            VSColorTheme.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(ThemeChangedEventArgs e = null)
        {
            var theme = DetectTheme();

            if (theme == "Blue")
            {
                Resources["SelectedItemBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0xFF));
            }
            else if (theme == "Light")
            {
                Resources["SelectedItemBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0xFF));
            }
            else
            {
                Resources["SelectedItemBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0xFF));
            }
        }

        private static Color AccentMediumDarkTheme = Color.FromRgb(0x2D, 0x2D, 0x30);
        private static Color AccentMediumLightTheme = Color.FromRgb(0xEE, 0xEE, 0xF2);
        private static Color AccentMediumBlueTheme = Color.FromRgb(0xFF, 0xEC, 0xB5);

        public static string DetectTheme()
        {
            try
            {
                var color = VSColorTheme.GetThemedColor(EnvironmentColors.AccentMediumColorKey);
                var cc = color.ToColor();
                if (cc == AccentMediumBlueTheme)
                    return "Blue";
                if (cc == AccentMediumLightTheme)
                    return "Light";
                if (cc == AccentMediumDarkTheme)
                    return "Dark";
                var brightness = color.GetBrightness();
                var dark = brightness > 0.5f;
                return dark ? "Dark" : "Light";
            }
            // this throws in design time and when running outside of VS
            catch (ArgumentNullException)
            {
                return "Dark";
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var type = typeof(VsBrushes);
            var properties = type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            using (var fs = new FileStream(@"d:\vsbrushes.txt", FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                foreach (var p in properties)
                {
                    var key = p.GetValue(null) as string;

                    var color = FindResource(key) as SolidColorBrush;

                    if (color != null)
                    {
                        writer.WriteLine($"{key}: {color}");
                    }
                }
            }
        }
    }
}