using System;
using System.IO;

namespace Client.Models.Networking
{
    public class BoardChangedEventArgs : EventArgs
    {
        public MemoryStream Strokes { get; set; }

        public BoardChangedEventArgs(MemoryStream strokes)
        {
            Strokes = strokes;
        }
    }

}
