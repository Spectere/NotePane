﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NotePane {
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class Note {
        private bool _expanded = true;

        /// <summary>
        /// Gets or sets whether or not the note is expanded or not.
        /// </summary>
        public bool Expanded {
            get => _expanded;
            set => ExpansionHandler(value);
        }

        public event EventHandler<Note> DeleteNote;
        public event EventHandler<Note> Modified;
        public event EventHandler<Note> MoveDown;
        public event EventHandler<Note> MoveUp;
        public event EventHandler<Note> MoveToTab;

        public Note() {
            InitializeComponent();
            TitleSeparator.Margin = new Thickness(0, TitleRow.Height.Value - 1, 0, 0);
            NoteText.Document.LineHeight = 1;
        }

        private void Delete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var result = MessageBox.Show("Are you sure you wish to delete this note? This action cannot be undone.", null, MessageBoxButton.YesNo);
            if(result != MessageBoxResult.Yes) return;

            Modified?.Invoke(this, this);
            DeleteNote?.Invoke(this, this);
        }

        private void ExpansionHandler(bool expand) {
            Height = expand ? double.NaN : TitleRow.Height.Value;
            ExpandButton.Content = expand ? "-" : "+";
            TitleSeparator.Visibility = expand ? Visibility.Hidden : Visibility.Visible;
            _expanded = expand;
            Modified?.Invoke(this, this);
        }

        private void Expand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ExpansionHandler(!_expanded);
        }

        private void MoveDown_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MoveDown?.Invoke(this, this);
        }

        private void MoveUp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MoveUp?.Invoke(this, this);
        }

        private void NoteContentsChanged(object sender, TextChangedEventArgs e) {
            Modified?.Invoke(this, this);
        }

        private void MoveToTab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MoveToTab?.Invoke(this, this);
        }
    }
}
