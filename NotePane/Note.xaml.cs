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
            NoteText.AppendText("Hello world!");
        }

        private void Expand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _expanded = !_expanded;
            Height = _expanded ? double.NaN : TitleRow.Height.Value;
            Expand.Content = _expanded ? "-" : "+";
            TitleSeparator.Visibility = _expanded ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
