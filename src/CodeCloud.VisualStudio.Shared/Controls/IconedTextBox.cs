using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace CodeCloud.VisualStudio.Shared.Controls
{
    [TemplatePart(Name = "PART_Icon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_WatermarkTextBlock", Type = typeof(TextBlock))]
    public class IconedTextBox : TextBox
    {
        static IconedTextBox()
        {
            // Fix System.Windows.Interactivity not found issue
            System.Console.WriteLine(typeof(Interaction));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconedTextBox), new FrameworkPropertyMetadata(typeof(IconedTextBox)));
        }

        private TextBlock _watermarkTextBlock;
        private Image _iconImage;

        /// <summary>
        /// Gets or sets a value indicating whether the busy indicator should show.
        /// </summary>
        [Description("The icon of the textbox"), Category("Appearance")]
        public ImageSource Icon
        {
            get { return GetValue(IconProperty) as ImageSource; }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Identifies the IsBusy dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(ImageSource),
            typeof(IconedTextBox),
            new PropertyMetadata((d, e) =>
            {
                var me = (IconedTextBox)d;
                if (me._iconImage != null)
                {
                    me._iconImage.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
                }
            }));

        [Description("Watermark of the textbox"), Category("Appearance")]
        public string Wartermark
        {
            get { return GetValue(WartermarkProperty) as string; }
            set { SetValue(WartermarkProperty, value); }
        }

        /// <summary>
        /// Identifies the IsBusy dependency property.
        /// </summary>
        public static readonly DependencyProperty WartermarkProperty = DependencyProperty.Register(
            nameof(Wartermark),
            typeof(string),
            typeof(IconedTextBox));

        [Description("If used as passwordbox"), Category("Appearance")]
        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        /// <summary>
        /// Identifies the IsBusy dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPasswordProperty = DependencyProperty.Register(
            nameof(IsPassword),
            typeof(bool),
            typeof(IconedTextBox));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _watermarkTextBlock = GetTemplateChild("PART_WatermarkTextBlock") as TextBlock;
            _iconImage = GetTemplateChild("PART_Icon") as Image;

            _iconImage.Visibility = Icon == null ? Visibility.Collapsed : Visibility.Visible;
        }

        // Fake char to display in Visual Tree
        const char PasswordChar = '●';

        // flag used to bypass OnTextChanged
        bool _dirtyBaseText;

        /// <summary>
        ///   Only copy of real password
        /// </summary>
        /// <remarks>
        ///   For more security use System.Security.SecureString type instead
        /// </remarks>
        string _text = string.Empty;

        /// <summary>
        ///   Provide access to base.Text without call OnTextChanged
        /// </summary>
        protected string BaseText
        {
            get { return base.Text; }
            set
            {
                _dirtyBaseText = true;
                base.Text = value;
                _dirtyBaseText = false;
            }
        }

        private void SyncBaseText()
        {
            BaseText = IsPassword ? new string(PasswordChar, _text.Length) : _text;
        }

        /// <summary>
        ///   Clean Password
        /// </summary>
        public new string Text
        {
            get { return _text; }
            set
            {
                _text = value ?? string.Empty;
                SyncBaseText();
            }
        }

        /// <summary>
        ///   TextChanged event handler for secure storing of password into Visual Tree,
        ///   text is replaced with pwdChar chars, clean text is kept in
        ///   Text property (CLR property not snoopable without mod)
        /// </summary>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            try
            {
                if (_dirtyBaseText)
                    return;

                string currentText = BaseText;

                int selStart = SelectionStart;
                if (_text != null && currentText.Length < _text.Length)
                {
                    // Remove deleted chars
                    _text = _text.Remove(selStart, _text.Length - currentText.Length);
                }
                if (!string.IsNullOrEmpty(currentText))
                {
                    for (int i = 0; i < currentText.Length; i++)
                    {
                        if (currentText[i] != PasswordChar)
                        {
                            Debug.Assert(_text != null, "Password can't be null here");
                            // Replace or insert char
                            string currentCharacter = currentText[i].ToString(CultureInfo.InvariantCulture);
                            _text = BaseText.Length == _text.Length ? _text.Remove(i, 1).Insert(i, currentCharacter) : _text.Insert(i, currentCharacter);
                        }
                    }
                    Debug.Assert(_text != null, "Password can't be null here");
                    SyncBaseText();
                    SelectionStart = selStart;
                }
                base.OnTextChanged(e);
            }
            finally
            {
                if (_watermarkTextBlock != null)
                {
                    _watermarkTextBlock.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}
