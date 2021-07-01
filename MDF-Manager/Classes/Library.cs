using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MDF_Manager.Classes
{
    public class LibraryEntry
    {
        //library entry only needs 2 things, material name/index (name will let it be future-proof) and filepath
        private string _MatName;
        public string Header { get; set; }
        public ObservableCollection<LibraryEntry> Items { get; set; }
        public string MaterialName { get => _MatName; set { _MatName = value; Header = value; } }
        public string MDFPath { get; set; }
        public LibraryEntry()
        {
            Items = new ObservableCollection<LibraryEntry>();
            MaterialName = "";
            MDFPath = "";

        }
        public LibraryEntry(string name, string path)
        {
            MaterialName = name;
            MDFPath = path;
        }
    }
    public class LibraryEntryHeader
    {
        //entryHeader contains children as 
        private string _Path;
        public string Header { get; set; }
        public ObservableCollection<LibraryEntry> Items { get; set; }
        public string MDFPath { get => _Path; set { _Path = value; Header = value; } }
        public LibraryEntryHeader()
        {
            Items = new ObservableCollection<LibraryEntry>();
            MDFPath = "";
        }
        public LibraryEntryHeader(string path)
        {
            Items = new ObservableCollection<LibraryEntry>();
            MDFPath = path;
        }
    }
    public class Library : INotifyPropertyChanged
    {
        public ObservableCollection<LibraryEntryHeader> entries { get; set; }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Library()
        {
            entries = new ObservableCollection<LibraryEntryHeader>();
        }
        public void SetEntries(ObservableCollection<LibraryEntryHeader> newEntries)
        {
            entries = newEntries;
            OnPropertyChanged("entries");
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
