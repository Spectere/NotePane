using Microsoft.Win32;
using NotePane.Schema;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;

namespace NotePane {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NoteView {
        private string _filename;
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
            var activeNoteContainer = (StackPanel)activeTab.Content;
            activeNoteContainer.Children.Add(CreateNote());
        }

        private Note CreateNote() {
            var newNote = new Note();
            newNote.DeleteNote += Note_DeleteNote;
            return newNote;
        }

        private void CreateTab() {
            var newTab = new TabItem {
                Header = $"Tab {++_tabCount}",
                Content = new StackPanel()
            };

            newTab.MouseDoubleClick += Tab_MouseDoubleClick;

            NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, newTab);
            NoteTabContainer.SelectedItem = newTab;
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e) {
            NoteExpansion(false);
        }

        private Notebook DeserializeFile(string filename) {
            var serializer = new XmlSerializer(typeof(Notebook));
            Notebook notebook;

            using(var notebookFile = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                notebook = (Notebook)serializer.Deserialize(notebookFile);

            return notebook;
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e) {
            NoteExpansion(true);
        }

        private void Load(Notebook notebook) {
            NewNotebook(false);

            foreach(var tab in notebook.Tabs) {
                var newTab = new TabItem { Header = tab.Title };
                var noteContainer = new StackPanel();

                if(tab.Note != null) {
                    foreach(var note in tab.Note) {
                        var newNote = CreateNote();
                        newNote.Expanded = !note.ExpandedSpecified || note.Expanded;
                        newNote.Title.Text = note.Title;

                        var documentContents = System.Convert.FromBase64String(note.Data);
                        using(var memoryStream = new MemoryStream(documentContents)) {
                            var range = new TextRange(newNote.NoteText.Document.ContentStart,
                                                      newNote.NoteText.Document.ContentEnd);
                            range.Load(memoryStream, DataFormats.XamlPackage);
                        }
                        noteContainer.Children.Add(newNote);
                    }
                }

                newTab.MouseDoubleClick += Tab_MouseDoubleClick;
                newTab.Content = noteContainer;

                NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, newTab);
            }

            if(notebook.SelectedTabSpecified)
                NoteTabContainer.SelectedIndex = notebook.SelectedTab;
        }

        private void New_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var result = MessageBox.Show(
                "Are you sure you wish to create a new Notebook? Unsaved changes will be lost.", null,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.No) return;

            NewNotebook();
        }

        private void NewNotebook(bool createNewTab = true) {
            _filename = null;
            _tabCount = 0;
            NoteTabContainer.Items.Clear();

            var newTab = new TabItem { Header = "+" };
            newTab.GotFocus += AddTab_GotFocus;
            NoteTabContainer.Items.Add(newTab);

            if(createNewTab)
                CreateTab();
        }

        private void Note_DeleteNote(object sender, Note e) {
            var noteContainer = (StackPanel)NoteTabContainer.SelectedContent;
            noteContainer.Children.Remove(e);
        }

        private void NoteExpansion(bool expand) {
            var noteContainer = (StackPanel)NoteTabContainer.SelectedContent;

            foreach(Note note in noteContainer.Children) {
                note.Expanded = expand;
            }
        }

        private void Open_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var openDlg = new OpenFileDialog { Filter = "Notebook|*.note|All Files|*.*" };

            var result = openDlg.ShowDialog();
            if(!result.HasValue || !result.Value) return;

            var filename = openDlg.FileName;
            Load(DeserializeFile(filename));
            _filename = filename;
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
            if(_filename == null) SaveAs();
            else SaveFile(_filename);
        }

        private void SaveAs_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            SaveAs();
        }

        private void SaveAs() {
            var saveDlg = new SaveFileDialog { Filter = "Notebook|*.note|All Files|*.*" };

            var result = saveDlg.ShowDialog();
            if(!result.HasValue || !result.Value) return;

            _filename = saveDlg.FileName;
            SaveFile(_filename);
        }

        private void SaveFile(string filename) {
            var serializer = new XmlSerializer(typeof(Notebook));

            using(var outputStream = new MemoryStream()) {
                serializer.Serialize(outputStream, SerializeData());

                outputStream.Position = 0;
                using(var fileStream = File.Open(filename, FileMode.Create))
                    outputStream.CopyTo(fileStream);
            }
        }

        private Notebook SerializeData() {
            var notebook = new Notebook();
            var outTabList = new List<TabType>();

            // Focus the form to fire off the LostFocus event for any stray rename boxes.
            Focus();

            foreach(var tabObject in NoteTabContainer.Items) {
                var tab = (TabItem)tabObject;
                if(tab.Content == null || tab.Content.GetType() != typeof(StackPanel)) continue;

                var outTab = new TabType();
                var noteContainer = (StackPanel)tab.Content;
                var tabNotes = new List<NoteType>();
                foreach(var containerChild in noteContainer.Children) {
                    if(containerChild.GetType() != typeof(Note)) continue;

                    var note = (Note)containerChild;
                    var outNote = new NoteType {
                        Expanded = note.Expanded,
                        ExpandedSpecified = true,
                        Title = note.Title.Text
                    };

                    using(var memoryStream = new MemoryStream()) {
                        var range = new TextRange(note.NoteText.Document.ContentStart, note.NoteText.Document.ContentEnd);
                        range.Save(memoryStream, DataFormats.XamlPackage);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        var rtfData = System.Convert.ToBase64String(memoryStream.ToArray());
                        outNote.Data = rtfData;
                    }

                    tabNotes.Add(outNote);
                }
                outTab.Title = (string)tab.Header;
                outTab.Note = tabNotes.ToArray();
                outTabList.Add(outTab);
            }
            notebook.Tabs = outTabList.ToArray();
            notebook.SelectedTab = NoteTabContainer.SelectedIndex;
            notebook.SelectedTabSpecified = true;

            return notebook;
        }

        private class TabData {
            public TabItem TabObject { get; set; }
            public string OldName { get; set; }
        }
    }
}
