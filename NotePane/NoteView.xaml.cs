using System.Windows;
using System.Windows.Controls;

namespace NotePane {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NoteView {
        private int _tabCount = 0;

        public NoteView() {
            InitializeComponent();

            CreateTab();
        }

        private void AddTab_GotFocus(object sender, RoutedEventArgs e) {
            CreateTab();
        }

        private void AddNote_Click(object sender, RoutedEventArgs e) {
            var activeTab = (TabItem)NoteTabContainer.SelectedItem;
            var activeNoteContainer = (NoteContainer)activeTab.Content;
            activeNoteContainer.NoteStack.Children.Add(new Note());
        }

        private void CreateTab() {
            var newTab = new TabItem {
                Header = $"Tab {++_tabCount}",
                Content = new NoteContainer()
            };

            NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, newTab);
            NoteTabContainer.SelectedItem = newTab;
        }
    }
}
