using GitLab.TeamFoundation.ViewModels;
using GitLab.VisualStudio.Shared;
using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;

namespace GitLab.TeamFoundation.Views
{
    /// <summary>
    /// Interaction logic for PublishSectionView.xaml
    /// </summary>
    public partial class PublishSectionView : UserControl
    {
        static PublishSectionView()
        {
            // Fix System.Windows.Interactivity not found issue
            System.Console.WriteLine(typeof(Interaction));
        }

        public PublishSectionView()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly().GetName().Name;
            var bi = new BitmapImage(new Uri($"pack://application:,,,/{assembly};component/Resources/logo.png", UriKind.Absolute)); ;

            octokit.Source = bi;
        }
    }
}
