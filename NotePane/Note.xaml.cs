using System.Windows;
using System.Windows.Input;

namespace NotePane {
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class Note {
        private bool _expanded = true;

        public Note() {
            InitializeComponent();
            TitleSeparator.Margin = new Thickness(0, TitleRow.Height.Value - 1, 0, 0);
            NoteText.Document.LineHeight = 1;
        }

        public void Collapse() {
            ExpansionHandler(false);
        }

        public void Expand() {
            ExpansionHandler(true);
        }

        private void ExpansionHandler(bool expand) {
            Height = expand ? double.NaN : TitleRow.Height.Value;
            ExpandButton.Content = expand ? "-" : "+";
            TitleSeparator.Visibility = expand ? Visibility.Hidden : Visibility.Visible;
            _expanded = expand;
        }

        private void Expand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ExpansionHandler(!_expanded);
        }
    }
}
