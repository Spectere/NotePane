using System;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NotePane {
    /// <summary>
    /// Interaction logic for NoteTab.xaml
    /// </summary>
    public partial class NoteTabHeader {
        public event EventHandler<TabItem> DeleteTab;

        public TabItem ParentTab { get; set; }

        public string TabTitle {
            get { return Title.Content.ToString(); }
            set { Title.Content = value; }
        }

        public NoteTabHeader() {
            InitializeComponent();
        }

        private void Delete() {
            var result = MessageBox.Show("Are you sure you wish to delete this tab? This action cannot be undone.", null, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if(result != MessageBoxResult.Yes) return;
            DeleteTab?.Invoke(this, ParentTab);
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e) {
            Delete();
        }

        private void NoteTabHeader_OnMouseDown(object sender, MouseButtonEventArgs e) {
            if(e.ChangedButton != MouseButton.Middle) return;
            Delete();
        }

        private void Rename(string newName) {
            Title.Content = newName;
        }

        private void Title_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(Title.Content.GetType() != typeof(string)) return;
            var header = (string)Title.Content;

            var renameBox = new TextBox() {
                Focusable = true,
                Tag = header,
                Text = header
            };

            renameBox.LostFocus += (obj, args) => {
                                       var textBox = (TextBox)obj;

                                       // Only update the textbox if it's still visible.
                                       // This allows input cancellation to work as expected.
                                       if(Title.Content.Equals(textBox))
                                           Rename(textBox.Text);
                                   };

            renameBox.KeyDown += (obj, args) => {
                                     if(args.Key != Key.Enter && args.Key != Key.Return && args.Key != Key.Escape)
                                         return;
                                     var textBox = (TextBox)obj;
                                     Rename(args.Key == Key.Escape ? (string)textBox.Tag : textBox.Text);
                                     args.Handled = true;
                                 };

            // Used to set focus when the control becomes visible.
            renameBox.IsVisibleChanged += (obj, args) => {
                                              if((bool)args.NewValue) ((TextBox)obj).Focus();
                                          };

            renameBox.SelectAll();
            Title.Content = renameBox;
        }
    }
}
