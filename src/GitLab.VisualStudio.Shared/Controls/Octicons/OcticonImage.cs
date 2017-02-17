using System.Windows;
using System.Windows.Controls;

namespace GitLab.VisualStudio.Shared.Controls
{
    public class OcticonImage : Control
    {
        public Octicon Icon
        {
            get { return (Octicon)GetValue(OcticonPath.IconProperty); }
            set { SetValue(OcticonPath.IconProperty, value); }
        }

        public static DependencyProperty IconProperty =
            OcticonPath.IconProperty.AddOwner(typeof(OcticonImage));

        static OcticonImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OcticonImage), new FrameworkPropertyMetadata(typeof(OcticonImage)));
        }
    }
}
