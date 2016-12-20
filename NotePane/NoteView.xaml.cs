using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            newTab.MouseDoubleClick += Tab_MouseDoubleClick;

            NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, newTab);
            NoteTabContainer.SelectedItem = newTab;
        }

        private void Tab_MouseDoubleClick(object sender, RoutedEventArgs e) {
            var tab = (TabItem)sender;
            if(tab.Header.GetType() != typeof(string)) return;

            var renameBox = new TextBox {
                Focusable = true,
                Tag = tab,
                Text = tab.Header.ToString()
            };

            renameBox.LostFocus += (obj, args) => {
                                       var txtBox = (TextBox)obj;
                                       var parentTab = (TabItem)txtBox.Tag;
                                       RenameTab(parentTab, txtBox.Text);
                                   };

            renameBox.KeyDown += (obj, args) => {
                                     if(args.Key != Key.Enter || args.Key != Key.Return) return;
                                     var txtBox = (TextBox)obj;
                                     var parentTab = (TabItem)txtBox.Tag;
                                     RenameTab(parentTab, txtBox.Text);
                                 };

            // Used to set focus when the control becomes visible.
            renameBox.IsVisibleChanged += (obj, args) => { ((TextBox)obj).Focus(); };

            renameBox.SelectAll();
            tab.Header = renameBox;
        }

        private static void RenameTab(TabItem tab, string name) {
            tab.Header = name;
        }
    }
}
