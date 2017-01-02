using System;
using Microsoft.Win32;
using NotePane.Schema;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private bool _modified;

        private bool Modified {
            get { return _modified; }
            set {
                _modified = value;
                SetTitle();
            }
        }

        public NoteView() {
            InitializeComponent();
            NewNotebook();
        }

        private void NoteView_OnClosing(object sender, CancelEventArgs e) {
            if(!UnsavedChangesWarning("Are you sure you wish to exit?")) e.Cancel = true;
        }

        private void AddTab(TabItem tab) {
            NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, tab);
            NoteTabContainer.SelectedItem = tab;
            Modified = true;
        }

        private void AddTab_GotFocus(object sender, RoutedEventArgs e) {
            AddTab(CreateTab());
        }

        private void AddNote_Click(object sender, RoutedEventArgs e) {
            var activeTab = (TabItem)NoteTabContainer.SelectedItem;
            var activeNoteContainer = ((NoteContainer)activeTab.Content).NoteStack;
            activeNoteContainer.Children.Add(CreateNote());
        }

        private Note CreateNote() {
            var newNote = new Note();
            newNote.DeleteNote += Note_DeleteNote;
            newNote.Modified += Note_Modified;
            newNote.MoveDown += Note_MoveDown;
            newNote.MoveUp += Note_MoveUp;
            return newNote;
        }

        private TabItem CreateTab(string header = null) {
            var headerText = header ?? $"Tab {++_tabCount}";
            var tabHeader = new NoteTabHeader { TabTitle = headerText };
            tabHeader.DeleteTab += TabHeader_OnDeleteTab;
            tabHeader.Modified += (sender, item) => { Modified = true; };

            var newTab = new TabItem {
                Content = new NoteContainer(),
                Style = (Style)Resources["NoteTabStyle"]
            };

            tabHeader.ParentTab = newTab;
            newTab.Header = tabHeader;

            return newTab;
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e) {
            NoteExpansion(false);
        }

        private void DeleteTab(TabItem tabItem) {
            // Kludge to prevent new tab from getting activated accidentally.
            var tabIndex = NoteTabContainer.Items.IndexOf(tabItem);
            var count = NoteTabContainer.Items.Count;
            if(count > 2 && tabIndex == count - 2) {
                // Decrement the selected index.
                NoteTabContainer.SelectedIndex--;
            }

            NoteTabContainer.Items.Remove(tabItem);

            if(NoteTabContainer.Items.Count == 1)
                AddTab(CreateTab());
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

        private StackPanel GetActiveNoteStack() {
            return ((NoteContainer)NoteTabContainer.SelectedContent).NoteStack;
        }

        private void Load(Notebook notebook) {
            NewNotebook(false);

            foreach(var tab in notebook.Tabs) {
                var newTab = CreateTab(tab.Title);
                var noteContainer = new NoteContainer();

                if(tab.Note != null) {
                    foreach(var note in tab.Note) {
                        var newNote = CreateNote();
                        newNote.Expanded = !note.ExpandedSpecified || note.Expanded;
                        newNote.Title.Text = note.Title;

                        var documentContents = Convert.FromBase64String(note.Data);
                        using(var memoryStream = new MemoryStream(documentContents)) {
                            var range = new TextRange(newNote.NoteText.Document.ContentStart,
                                                      newNote.NoteText.Document.ContentEnd);
                            range.Load(memoryStream, DataFormats.XamlPackage);
                        }
                        noteContainer.NoteStack.Children.Add(newNote);
                    }
                }

                newTab.Content = noteContainer;
                NoteTabContainer.Items.Insert(NoteTabContainer.Items.Count - 1, newTab);
            }

            if(notebook.SelectedTabSpecified)
                NoteTabContainer.SelectedIndex = notebook.SelectedTab;
        }

        private void New_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if(!UnsavedChangesWarning("Are you sure you wish to create a new notebook?")) return;
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
                AddTab(CreateTab());

            _modified = false;
            SetTitle();
        }

        private void Note_DeleteNote(object sender, Note e) {
            var noteContainer = GetActiveNoteStack();
            noteContainer.Children.Remove(e);
        }

        private void Note_Modified(object sender, Note e) {
            Modified = true;
        }

        private void Note_MoveDown(object sender, Note e) {
            if(e == null) return;

            var noteContainer = GetActiveNoteStack();
            var destinationIndex = noteContainer.Children.IndexOf(e) + 1;

            if(destinationIndex >= noteContainer.Children.Count) return;
            noteContainer.Children.Remove(e);
            noteContainer.Children.Insert(destinationIndex, e);

            Modified = true;
        }

        private void Note_MoveUp(object sender, Note e) {
            if(e == null) return;

            var noteContainer = GetActiveNoteStack();
            var destinationIndex = noteContainer.Children.IndexOf(e) - 1;

            if(destinationIndex < 0) return;
            noteContainer.Children.Remove(e);
            noteContainer.Children.Insert(destinationIndex, e);

            Modified = true;
        }

        private void NoteExpansion(bool expand) {
            var noteContainer = GetActiveNoteStack();

            foreach(Note note in noteContainer.Children)
                note.Expanded = expand;
        }

        private void NoteTab_Drag(object sender, MouseEventArgs e) {
            if(e.Source.GetType() != typeof(TabItem)) return;
            var noteTab = (TabItem)e.Source;
            if(noteTab == null) return;

            if(e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(noteTab, noteTab, DragDropEffects.Move);
        }

        private void NoteTab_Drop(object sender, DragEventArgs e) {
            if(e.Source.GetType() != typeof(TabItem) && e.Source.GetType() != typeof(NoteTabHeader)) return;
            var tabSource = (TabItem)e.Data.GetData(typeof(TabItem));

            TabItem tabDestination;
            if(e.Source.GetType() == typeof(NoteTabHeader)) {
                var tabHeader = (NoteTabHeader)e.Source;
                if(tabHeader == null) return;
                tabDestination = tabHeader.ParentTab;
            } else {
                tabDestination = (TabItem)e.Source;
            }

            if(tabSource == null || tabDestination == null || tabSource.Equals(tabDestination)) return;

            var destinationIndex = NoteTabContainer.Items.IndexOf(tabDestination);

            NoteTabContainer.SelectedIndex = 0;  // kludge to prevent the new tab function from activating
            NoteTabContainer.Items.Remove(tabSource);
            NoteTabContainer.Items.Insert(destinationIndex, tabSource);
            NoteTabContainer.SelectedIndex = destinationIndex;

            Modified = true;
        }

        private void Open_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var openDlg = new OpenFileDialog { Filter = "Notebook|*.note|All Files|*.*" };

            var result = openDlg.ShowDialog();
            if(!result.HasValue || !result.Value) return;
            if(!UnsavedChangesWarning("Are you sure you wish to open another file?")) return;

            var filename = openDlg.FileName;
            Load(DeserializeFile(filename));
            _filename = filename;
            _modified = false;

            SetTitle();
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

            Modified = false;
        }

        private Notebook SerializeData() {
            var notebook = new Notebook();
            var outTabList = new List<TabType>();

            // Focus the form to fire off the LostFocus event for any stray rename boxes.
            Focus();

            foreach(var tabObject in NoteTabContainer.Items) {
                var tab = (TabItem)tabObject;
                if(tab.Content == null || tab.Content.GetType() != typeof(NoteContainer)) continue;

                var outTab = new TabType();
                var noteContainer = ((NoteContainer)tab.Content).NoteStack;
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
                        var rtfData = Convert.ToBase64String(memoryStream.ToArray());
                        outNote.Data = rtfData;
                    }

                    tabNotes.Add(outNote);
                }
                outTab.Title = ((NoteTabHeader)tab.Header).TabTitle;
                outTab.Note = tabNotes.ToArray();
                outTabList.Add(outTab);
            }
            notebook.Tabs = outTabList.ToArray();
            notebook.SelectedTab = NoteTabContainer.SelectedIndex;
            notebook.SelectedTabSpecified = true;

            return notebook;
        }

        private void SetTitle() {
            var file = _filename != null
                ? Path.GetFileNameWithoutExtension(_filename)
                : "Untitled";

            Title = string.Format("NotePane - {1}[{0}]", file, _modified ? "*" : "");
        }

        private void TabHeader_OnDeleteTab(object sender, TabItem tabItem) {
            DeleteTab(tabItem);
        }

        private bool UnsavedChangesWarning(string message) {
            if(!_modified) return true;
            var result = MessageBox.Show($"{message} Unsaved changes will be lost.", null, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes) return true;
            return false;
        }
    }
}
