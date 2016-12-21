using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NotePane {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NoteView {
        private int _tabCount;

        public NoteView() {
            InitializeComponent();
            NewNotebook();
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

        private void CollapseAll_Click(object sender, RoutedEventArgs e) {
            NoteExpansion(false);
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e) {
            NoteExpansion(true);
        }

        private void New_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            NewNotebook();
        }

        private void NewNotebook() {
            _tabCount = 0;
            NoteTabContainer.Items.Clear();

            var newTab = new TabItem { Header = "+" };
            newTab.GotFocus += AddTab_GotFocus;
            NoteTabContainer.Items.Add(newTab);

            CreateTab();
        }

        private void NoteExpansion(bool expand) {
            var noteContainer = (NoteContainer)NoteTabContainer.SelectedContent;

            foreach(Note note in noteContainer.NoteStack.Children) {
                if(expand) note.Expand();
                else note.Collapse();
            }
        }

        private void Open_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void Tab_MouseDoubleClick(object sender, RoutedEventArgs e) {
            // The only thing that uses TextBlock controls (as of right now) are tab headers.
            // TODO: Make this a bit more of a surefire thing.
            if(e.OriginalSource.GetType() != typeof(TextBlock)) return;

            var tab = (TabItem)sender;
            if(tab.Header.GetType() != typeof(string)) return;

            var renameBox = new TextBox {
                Focusable = true,
                Tag = new TabData {
                    TabObject = tab,
                    OldName = tab.Header.ToString()
                },
                Text = tab.Header.ToString()
            };

            renameBox.LostFocus += (obj, args) => {
                                       var txtBox = (TextBox)obj;
                                       var tabData = (TabData)txtBox.Tag;
                                       var parentTab = tabData.TabObject;

                                       // Only update the textbox if it's still in the header.
                                       // This allows cancelling input to work as expected.
                                       if(parentTab.Header.Equals(txtBox))
                                           RenameTab(parentTab, txtBox.Text);
                                   };

            renameBox.KeyDown += (obj, args) => {
                                     if(args.Key != Key.Enter && args.Key != Key.Return && args.Key != Key.Escape) return;
                                     var txtBox = (TextBox)obj;
                                     var tabData = (TabData)txtBox.Tag;
                                     var parentTab = tabData.TabObject;
                                     RenameTab(parentTab, args.Key == Key.Escape ? tabData.OldName : txtBox.Text);
                                     args.Handled = true;
                                 };

            // Used to set focus when the control becomes visible.
            renameBox.IsVisibleChanged += (obj, args) => {
                                              if((bool)args.NewValue) ((TextBox)obj).Focus();
                                          };

            renameBox.SelectAll();
            tab.Header = renameBox;
        }

        private static void RenameTab(HeaderedContentControl tab, string name) {
            tab.Header = name;
        }

        private void Save_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void SaveAs_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private class TabData {
            public TabItem TabObject { get; set; }
            public string OldName { get; set; }
        }
    }
}
