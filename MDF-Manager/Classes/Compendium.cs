﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MDF_Manager.Classes
{
    public class CompendiumEntry
    {
        //library entry only needs 2 things, material name/index (name will let it be future-proof) and filepath
        private string _MDFPath;
        public string Header { get; set; }
        public ObservableCollection<CompendiumEntry> Items { get; set; }
        public string MDFPath { get => _MDFPath; set { _MDFPath = value; Header = value;} }
        public CompendiumEntry()
        {
            Items = new ObservableCollection<CompendiumEntry>();
            MDFPath = "";

        }
        public CompendiumEntry(string path)
        {
            Items = new ObservableCollection<CompendiumEntry>(); //how did this not crash
            MDFPath = path;
        }
    }
    public class CompendiumEntryHeader : IComparable<CompendiumEntryHeader>
    {
        private string _MMTRName;
        public string Header { get; set; }
        public ObservableCollection<CompendiumEntry> Items { get; set; }
        public string MMTRName { get => _MMTRName; set { _MMTRName = value; Header = value; } }
        public CompendiumEntryHeader()
        {
            Items = new ObservableCollection<CompendiumEntry>();
            MMTRName = "";
        }
        public CompendiumEntryHeader(string mmtr)
        {
            Items = new ObservableCollection<CompendiumEntry>();
            MMTRName = mmtr;
        }
        public int FindEntry(string path)
        {
            for(int i = 0; i < Items.Count; i++)
            {
                if(Items[i].MDFPath == path)
                {
                    return i;
                }
            }
            return -1;//the wanted approach here
        }
        public int CompareTo(CompendiumEntryHeader compare)
        {
            if (compare == null)
            {
                return 1;
            }
            else return this.MMTRName.CompareTo(compare.MMTRName);
        }
    }
    public class CompendiumTopLevel : INotifyPropertyChanged
    {
        //entryHeader contains children as 
        private string _Game;
        public string Header { get; set; }
        private List<CompendiumEntryHeader> _Items = new List<CompendiumEntryHeader>();
        public ObservableCollection<CompendiumEntryHeader> Items { get => new ObservableCollection<CompendiumEntryHeader>(_Items); set { _Items = new List<CompendiumEntryHeader>(value); OnPropertyChanged("Items"); } }
        public string Game { get => _Game; set { _Game = value; Header = value; } }
        public CompendiumTopLevel()
        {
            Game = "";
        }
        public CompendiumTopLevel(string game)
        {
            Game = game;
        }
        public void ClearChildren()
        {
            _Items.Clear();
        }
        public void Sort()
        {
            _Items.Sort();
            OnPropertyChanged("Items");
        }
        public void AddChild(CompendiumEntryHeader ceh)
        {
            _Items.Add(ceh);
        }
        public CompendiumEntryHeader FindItem(Predicate<CompendiumEntryHeader> predicate)
        {
            return _Items.Find(predicate);
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Compendium : INotifyPropertyChanged
    {
        public ObservableCollection<CompendiumTopLevel> entries { get; set; }
        //here, set references to each one, so we can attach to them
        public CompendiumTopLevel RE7 { get; set; }
        public CompendiumTopLevel RE2DMC5 { get; set; }
        public CompendiumTopLevel RE3 { get; set; }
        public CompendiumTopLevel MHRiseRE8 { get; set; }
        public CompendiumTopLevel REV { get; set; }
        public CompendiumTopLevel RERT { get; set; }
        public CompendiumTopLevel Sunbreak { get; set; }
        public CompendiumTopLevel SF6 { get; set; }
        public CompendiumTopLevel RE4 { get; set; }
        public CompendiumTopLevel MHWS { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public Compendium()
        {
            RE7 = new CompendiumTopLevel("Version 6 (RE7)");
            RE2DMC5 = new CompendiumTopLevel("Version 10 (RE2/DMC5)");
            RE3 = new CompendiumTopLevel("Version 13 (RE3)");
            MHRiseRE8 = new CompendiumTopLevel("Version 19 (MHRise/RE8)");
            REV = new CompendiumTopLevel("Version 20 (REVerse)");
            RERT = new CompendiumTopLevel("Version 21 (RE Raytracing)");
            Sunbreak = new CompendiumTopLevel("Version 23 (MHRS)");
            SF6 = new CompendiumTopLevel("Version 31 (SF6)");
            RE4 = new CompendiumTopLevel("Version 32 (RE4)");
            MHWS = new CompendiumTopLevel("Version 45 (MHWS)");
            entries = new ObservableCollection<CompendiumTopLevel> { RE7, RE2DMC5, RE3, MHRiseRE8, REV, RERT, Sunbreak, SF6, RE4, MHWS };

        }
        public void SetEntries(ObservableCollection<CompendiumTopLevel> newEntries)
        {
            foreach (CompendiumTopLevel entry in newEntries)
            {
                switch (entry.Game)
                {
                    case "RE7":
                    case "Version 6 (RE7)":
                        RE7.Items = entry.Items;
                        break;
                    case "RE2/DMC5":
                    case "Version 10 (RE2/DMC5)":
                        RE2DMC5.Items = entry.Items;
                        break;
                    case "RE3":
                    case "Version 13 (RE3)":
                        RE3.Items = entry.Items;
                        break;
                    case "MHRise/RE8":
                    case "Version 19 (MHRise/RE8)":
                        MHRiseRE8.Items = entry.Items;
                        break;
                    case "Version 20 (REVerse)":
                        REV.Items = entry.Items;
                        break;
                    case "Version 21 (RE Raytracing)":
                        RERT.Items = entry.Items;
                        break;
                    case "Version 23 (MHRS)":
                        Sunbreak.Items = entry.Items;
                        break;
                    case "Version 31 (SF6)":
                        SF6.Items = entry.Items;
                        break;
                    case "Version 32 (RE4)":
                        RE4.Items = entry.Items;
                        break;
                    case "Version 45 (MHWS)":
                        MHWS.Items = entry.Items;
                        break;
                }
            }
            OnPropertyChanged("entries");
            Sort();
        }

        public void ClearList()
        {
            foreach(CompendiumTopLevel ctl in entries)
            {
                ctl.ClearChildren();
            }
        }
        
        public void Sort()
        {
            RE7.Sort();
            RE2DMC5.Sort();
            RE3.Sort();
            MHRiseRE8.Sort();
            REV.Sort();
            RERT.Sort();
            Sunbreak.Sort();
            SF6.Sort();
            RE4.Sort();
            MHWS.Sort();
            OnPropertyChanged("entries");
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
