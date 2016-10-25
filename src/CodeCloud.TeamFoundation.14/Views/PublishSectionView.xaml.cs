using CodeCloud.TeamFoundation.ViewModels;
using CodeCloud.VisualStudio.Shared;
using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CodeCloud.TeamFoundation.Views
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
        }
    }
}
