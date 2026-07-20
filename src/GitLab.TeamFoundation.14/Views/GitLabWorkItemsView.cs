using GitLab.TeamFoundation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GitLab.TeamFoundation.Views
{
    public class GitLabWorkItemsView : UserControl
    {
        private readonly bool _showTargetBranch;

        public GitLabWorkItemsView(bool showTargetBranch)
        {
            _showTargetBranch = showTargetBranch;
            Content = CreateContent();
        }

        private UIElement CreateContent()
        {
            var root = new DockPanel
            {
                LastChildFill = true,
                Margin = new Thickness(8)
            };

            var filters = new WrapPanel
            {
                Margin = new Thickness(0, 0, 0, 8)
            };
            DockPanel.SetDock(filters, Dock.Top);

            filters.Children.Add(CreateTextBox("Project", "ProjectPath", 180));
            filters.Children.Add(CreateComboBox("State", "State", _showTargetBranch
                ? new[] { "opened", "merged", "closed", "locked", "all" }
                : new[] { "opened", "closed", "all" }, 90));
            filters.Children.Add(CreateComboBox("Scope", "Scope", new[] { "all", "created_by_me", "assigned_to_me" }, 120));
            filters.Children.Add(CreateTextBox("Author", "AuthorUsername", 105));
            filters.Children.Add(CreateTextBox("Assignee", "AssigneeUsername", 105));
            filters.Children.Add(CreateTextBox("Labels", "Labels", 120));
            filters.Children.Add(CreateTextBox("Search", "Search", 120));
            if (_showTargetBranch)
            {
                filters.Children.Add(CreateTextBox("Target", "TargetBranch", 100));
            }

            var refreshButton = new Button
            {
                Content = "Refresh",
                MinWidth = 72,
                Margin = new Thickness(4, 0, 4, 6),
                Padding = new Thickness(8, 2, 8, 2)
            };
            refreshButton.SetBinding(Button.CommandProperty, new Binding("RefreshCommand"));
            filters.Children.Add(refreshButton);

            var openButton = new Button
            {
                Content = "Open",
                MinWidth = 60,
                Margin = new Thickness(4, 0, 4, 6),
                Padding = new Thickness(8, 2, 8, 2)
            };
            openButton.SetBinding(Button.CommandProperty, new Binding("OpenCommand"));
            filters.Children.Add(openButton);

            var status = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 6)
            };
            status.SetBinding(TextBlock.TextProperty, new Binding("StatusMessage"));
            DockPanel.SetDock(status, Dock.Top);

            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
                SelectionMode = DataGridSelectionMode.Single,
                MinHeight = 280
            };
            grid.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Items"));
            grid.SetBinding(DataGrid.SelectedItemProperty, new Binding("SelectedItem") { Mode = BindingMode.TwoWay });
            grid.MouseDoubleClick += OnGridMouseDoubleClick;

            grid.Columns.Add(CreateColumn("ID", "Reference", 58));
            grid.Columns.Add(CreateColumn("Title", "Title", 260));
            grid.Columns.Add(CreateColumn("State", "State", 76));
            grid.Columns.Add(CreateColumn("Author", "Author", 90));
            grid.Columns.Add(CreateColumn("Assignees", "Assignees", 120));
            grid.Columns.Add(CreateColumn("Labels", "Labels", 180));
            if (_showTargetBranch)
            {
                grid.Columns.Add(CreateColumn("Source", "SourceBranch", 120));
                grid.Columns.Add(CreateColumn("Target", "TargetBranch", 120));
            }
            grid.Columns.Add(CreateColumn("Updated", "UpdatedAt", 120, "g"));

            root.Children.Add(filters);
            root.Children.Add(status);
            root.Children.Add(grid);
            return root;
        }

        private static FrameworkElement CreateTextBox(string label, string path, double width)
        {
            var panel = CreateFieldPanel(label);
            var textBox = new TextBox
            {
                Width = width,
                Margin = new Thickness(0, 0, 8, 6)
            };
            textBox.SetBinding(TextBox.TextProperty, new Binding(path)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            panel.Children.Add(textBox);
            return panel;
        }

        private static FrameworkElement CreateComboBox(string label, string path, string[] values, double width)
        {
            var panel = CreateFieldPanel(label);
            var comboBox = new ComboBox
            {
                Width = width,
                IsEditable = true,
                Margin = new Thickness(0, 0, 8, 6)
            };
            foreach (var value in values)
            {
                comboBox.Items.Add(value);
            }
            comboBox.SetBinding(ComboBox.TextProperty, new Binding(path)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            panel.Children.Add(comboBox);
            return panel;
        }

        private static StackPanel CreateFieldPanel(string label)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 4, 0)
            };
            panel.Children.Add(new TextBlock { Text = label });
            return panel;
        }

        private static DataGridTextColumn CreateColumn(string header, string path, double width, string stringFormat = null)
        {
            var binding = new Binding(path);
            if (!string.IsNullOrWhiteSpace(stringFormat))
            {
                binding.StringFormat = stringFormat;
            }

            return new DataGridTextColumn
            {
                Header = header,
                Binding = binding,
                Width = width
            };
        }

        private void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as GitLabWorkItemsViewModel;
            var command = vm != null ? vm.OpenCommand : null;
            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
            }
        }
    }
}
