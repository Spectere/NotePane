using System;
using System.Windows.Media;

namespace NotePane {
    /// <summary>
    /// Interaction logic for Note.xaml
    /// </summary>
    public partial class Note {
        public Note() {
            InitializeComponent();

            var rng = new Random();
            var col = new byte[3];
            rng.NextBytes(col);
            Background.Background = new SolidColorBrush(Color.FromArgb(byte.MaxValue, col[0], col[1], col[2]));
            Height = 100;
        }
    }
}
