using System.Windows;

namespace NotePane {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NoteView {
        public NoteView() {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            NoteStack.Children.Add(new Note());
        }
    }
}
