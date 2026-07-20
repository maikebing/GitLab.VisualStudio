using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GitLab.TeamFoundation.Views
{
    public class GitLabSettingsView : UserControl
    {
        public GitLabSettingsView()
        {
            Content = CreateContent();
        }

        private UIElement CreateContent()
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(8)
            };

            panel.Children.Add(CreateTextBox("Default branch", "DefaultBranch"));
            panel.Children.Add(CreateTextBox("Issues project", "IssuesProjectPath"));
            panel.Children.Add(CreateTextBox("Merge requests project", "MergeRequestsProjectPath"));

            var saveButton = new Button
            {
                Content = "Save",
                HorizontalAlignment = HorizontalAlignment.Left,
                MinWidth = 72,
                Margin = new Thickness(0, 4, 0, 8),
                Padding = new Thickness(8, 2, 8, 2)
            };
            saveButton.SetBinding(Button.CommandProperty, new Binding("SaveCommand"));
            panel.Children.Add(saveButton);

            var status = new TextBlock();
            status.SetBinding(TextBlock.TextProperty, new Binding("StatusMessage"));
            panel.Children.Add(status);

            return panel;
        }

        private static FrameworkElement CreateTextBox(string label, string path)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 8)
            };

            panel.Children.Add(new TextBlock
            {
                Text = label,
                Margin = new Thickness(0, 0, 0, 2)
            });

            var textBox = new TextBox
            {
                MinWidth = 220
            };
            textBox.SetBinding(TextBox.TextProperty, new Binding(path)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            panel.Children.Add(textBox);

            return panel;
        }
    }
}
