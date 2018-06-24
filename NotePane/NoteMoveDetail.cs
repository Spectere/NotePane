namespace NotePane {
    internal class NoteMoveDetail {
        public Note Note { get; set; }
        public int SourceTabIndex { get; set; }
        public int DestinationTabIndex { get; set; }
        public string DestinationTabTitle { get; set; }

        public override string ToString() {
            return DestinationTabTitle;
        }
    }
}
