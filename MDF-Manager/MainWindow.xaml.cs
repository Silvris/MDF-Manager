using MDF_Manager.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MDF_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string MDFFilter = "All readable files (*.mdf2)|*.mdf2.6*;*.mdf2.10*;*.mdf2.13*;*.mdf2.19*;*.mdf2.21*;*.mdf2.23*|" +
            "RE7 Material file (*.mdf2.6)|*.mdf2.6*|" +
            "RE2/DMC5 Material file (*.mdf2.10)|*.mdf2.10*|" +
            "RE3 Material file (*.mdf2.13)|*.mdf2.13*|" +
            "RE8/MHRiseRE8 Material file (*.mdf2.19)|*.mdf2.19*|" +
            "RE2/3/7 RT-Update Material file (*.mdf2.21)|*.mdf2.21*|" +
            "MH Rise Sunbreak Material file (*.mdf2.23)|*.mdf2.23*";
        public ObservableCollection<MDFFile> MDFs { get; set; }
        public Defs defs { get; set; }
        public Library lib { get; set; }
        public Compendium compendium { get; set; }
        public bool LibraryChanged = false;
        public bool CompendiumChanged = false;
        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
        public SolidColorBrush WindowsColor { get; set; }
        public SolidColorBrush ButtonsColor { get; set; }
        public SolidColorBrush TextColor { get; set; }

        bool IsDragging;

        public MainWindow()
        {
            lib = new Library();
            compendium = new Compendium();
            if (File.Exists("defs.json"))
            {
                string jsontxt = File.ReadAllText("defs.json");
                defs = JsonSerializer.Deserialize<Defs>(jsontxt);
            }
            else
            {
                defs = new Defs();
            }
            if(defs.lastOpenLib != "")
            {
                string jsontxt = File.ReadAllText(defs.lastOpenLib);
                Library nlib = JsonSerializer.Deserialize<Library>(jsontxt);
                lib.SetEntries(nlib.entries);
            }
            if(defs.lastOpenComp != "")
            {
                string jsontxt = File.ReadAllText(defs.lastOpenComp);
                Compendium comp = JsonSerializer.Deserialize<Compendium>(jsontxt);
                compendium.SetEntries(comp.entries);
            }
            MDFs = new ObservableCollection<MDFFile>();
            if(defs.lastOpenFiles.Count > 0)
            {
                for(int i = 0; i < defs.lastOpenFiles.Count; i++)
                {
                    if (File.Exists(defs.lastOpenFiles[i]))
                    {
                        BinaryReader readFile = HelperFunctions.OpenFileR(defs.lastOpenFiles[i], Encoding.Unicode);
                        if(readFile != null)
                        {
                            try
                            {
                                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(defs.lastOpenFiles[i]).Replace(".", ""));
                                MDFs.Add(new MDFFile(defs.lastOpenFiles[i], readFile, type));
                                readFile.Close();
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show($"Unable to open file {defs.lastOpenFiles[i]} due to an exception: {ex}");
                                defs.lastOpenFiles[i] = "";
                            }
                        }
                    }

                }
            }
            InitializeBrushes(defs);
            InitializeComponent();
            MaterialView.DataContext = this;
            LibraryView.DataContext = this;
            CompendiumView.DataContext = this;
            UpdateWindowBrushes();
            if (MDFs.Count > 0)
            {
                MaterialView.SelectedItem = MDFs[0];
            }
        }

        void InitializeBrushes(Defs def)
        {
            BackgroundColor = new SolidColorBrush(defs.background);
            ForegroundColor = new SolidColorBrush(defs.foreground);
            ButtonsColor = new SolidColorBrush(defs.buttons);
            WindowsColor = new SolidColorBrush(defs.windows);
            TextColor = new SolidColorBrush(defs.text);
        }
        void UpdateMaterials()
        {
            for (int i = 0; i < MDFs[MaterialView.SelectedIndex].Materials.Count; i++)
            {
                MDFs[MaterialView.SelectedIndex].Materials[i].UpdateMaterialIndex(i);
            }
        }

        bool CheckMaterialNames(int mdfIndex)
        {
            List<string> currentNames = new List<string>();
            for(int i = 0; i < MDFs[mdfIndex].Materials.Count; i++)
            {
                if (currentNames.Contains(MDFs[mdfIndex].Materials[i].Name))
                {
                    return false;
                }
                else
                {
                    currentNames.Add(MDFs[mdfIndex].Materials[i].Name);
                }
            }
            return true;
        }

        private void OpenMDFFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog importFile = new OpenFileDialog();
            importFile.Multiselect = false;
            importFile.Filter = MDFFilter;
            if (importFile.ShowDialog() == true)
            {
                BinaryReader readFile = HelperFunctions.OpenFileR(importFile.FileName, Encoding.Unicode);
                if(readFile != null)
                {
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(importFile.FileName).Replace(".", ""));
                    MDFs.Add(new MDFFile(importFile.FileName, readFile, type));
                    readFile.Close();
                }

            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if(MDFs.Count > 0)
            {
                if (!CheckMaterialNames(MaterialView.SelectedIndex))
                {
                    MessageBox.Show("Material names cannot be identical!");
                }
                else
                {
                    BinaryWriter bw = HelperFunctions.OpenFileW(MDFs[MaterialView.SelectedIndex].Header, Encoding.Unicode);
                    if(bw != null)
                    {
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[MaterialView.SelectedIndex].Header).Replace(".", ""));
                        MDFs[MaterialView.SelectedIndex].Export(bw, type);
                        bw.Close();
                    }
                }
            }


        }
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            if(MDFs.Count > 0)
            {
                if (!CheckMaterialNames(MaterialView.SelectedIndex))
                {
                    MessageBox.Show("Material names cannot be identical!");
                }
                else
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Filter = MDFFilter;
                    saveFile.FileName = System.IO.Path.GetFileName(MDFs[MaterialView.SelectedIndex].Header);
                    if (saveFile.ShowDialog() == true)
                    {
                        BinaryWriter bw = HelperFunctions.OpenFileW(saveFile.FileName, Encoding.Unicode);
                        if(bw != null)
                        {
                            MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(saveFile.FileName).Replace(".", ""));
                            MDFs[MaterialView.SelectedIndex].Export(bw, type);
                            bw.Close();
                            MDFs[MaterialView.SelectedIndex].Header = saveFile.FileName;
                        }
                    }
                }
            }


        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {
            //this one doesn't actually need a check, since the for loop will just immediately end if MDFs.Count = 0
            for(int i = 0; i < MDFs.Count; i++)
            {
                if (!CheckMaterialNames(i))
                {
                    MessageBox.Show("Material names cannot be identical!");
                }
                else
                {
                    BinaryWriter bw = HelperFunctions.OpenFileW(MDFs[i].Header, Encoding.Unicode);
                    if(bw != null)
                    {
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[i].Header).Replace(".", ""));
                        MDFs[i].Export(bw, type);
                        bw.Close();
                    }

                }

            }
        }
        void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                if(LibraryView.SelectedItem != null && LibraryView.SelectedItem is LibraryEntry && MDFs.Count > 0)
                {
                    IsDragging = true;
                    LibraryEntry entry = (LibraryEntry)LibraryView.SelectedItem;
                    DragDrop.DoDragDrop(LibraryView, LibraryView.SelectedValue,
                        DragDropEffects.Copy);
                    IsDragging = false;
                }
            }
        }
        void treeView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(LibraryEntry)) && !e.Data.GetDataPresent(typeof(CompendiumEntry)) && !e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void TabControl_Drop(object sender, DragEventArgs e)
        {
            //library dropping
            if (e.Data.GetDataPresent(typeof(LibraryEntry)))
            {
                if(MDFs.Count > 0)
                {
                    LibraryEntry entry = (LibraryEntry)e.Data.GetData(typeof(LibraryEntry));
                    BinaryReader readFile = HelperFunctions.OpenFileR(entry.MDFPath,Encoding.Unicode);
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(entry.MDFPath).Replace(".", ""));
                    if(readFile != null)
                    {
                        MDFFile donor = new MDFFile(entry.MDFPath, readFile, type);
                        readFile.Close();
                        Material newMat = null;
                        for (int i = 0; i < donor.Materials.Count; i++)
                        {
                            //the only flaw of ObvservableCollection is lack of Find(), which is fine since we can get around it
                            if (donor.Materials[i].Name == entry.MaterialName)
                            {
                                newMat = donor.Materials[i];
                            }
                        }
                        if (newMat != null)
                        {
                            MDFs[MaterialView.SelectedIndex].Materials.Add(newMat);
                            UpdateMaterials();
                        }
                    }

                }

            }
            else if (e.Data.GetDataPresent(typeof(CompendiumEntry)))
            {
                CompendiumEntry entry = (CompendiumEntry)e.Data.GetData(typeof(CompendiumEntry));
                BinaryReader readFile = HelperFunctions.OpenFileR(entry.MDFPath, Encoding.Unicode);
                if (readFile != null)
                {
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(entry.MDFPath).Replace(".", ""));
                    MDFs.Add(new MDFFile(entry.MDFPath, readFile, type));
                    readFile.Close();
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for(int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(".6") || files[i].Contains(".10") || files[i].Contains(".13") || files[i].Contains(".19") || files[i].Contains(".21") || files[i].Contains(".23"))
                    {
                        BinaryReader readFile = HelperFunctions.OpenFileR(files[i], Encoding.Unicode);
                        if(readFile != null)
                        {
                            MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(files[i]).Replace(".", ""));
                            MDFs.Add(new MDFFile(files[i], readFile, type));
                            readFile.Close();
                        }
                    }
                }

            }

        }
        
        private void ChangeColor(object sender, MouseButtonEventArgs e)
        {
            Rectangle send = (Rectangle)sender;
            Float4Property prop = (Float4Property)send.DataContext;
            Float4 propVal = (Float4)prop.value;
            ColorCanvas cCanvas = new ColorCanvas();
            cCanvas.colorCanvas.SelectedColor = propVal.mColor;
            if(cCanvas.ShowDialog() == true)
            {
                Color newColor = (Color)cCanvas.colorCanvas.SelectedColor;
                Float4 nFloat4 = new Float4(newColor.ScR, newColor.ScG, newColor.ScB, newColor.ScA);
                MDFs[MaterialView.SelectedIndex].Materials[prop.indexes[0]].Properties[prop.indexes[1]].value = nFloat4;
            }
        }

        private void DeleteMaterial(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            ContextMenu cm = (ContextMenu)mi.Parent;
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Material mat = (Material)lbi.DataContext; //what a chain to get this going
            MDFs[MaterialView.SelectedIndex].Materials.RemoveAt(mat.materialIndex);
            UpdateMaterials();
            
        }

        private void AddToLibrary(object sender, RoutedEventArgs e)
        {
            OpenFileDialog importFile = new OpenFileDialog();
            importFile.Multiselect = false;
            importFile.Filter = MDFFilter;
            if (importFile.ShowDialog() == true)
            {
                BinaryReader readFile = HelperFunctions.OpenFileR(importFile.FileName, Encoding.Unicode);
                if(readFile != null)
                {
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(importFile.FileName).Replace(".", ""));
                    MDFFile ibmdf = new MDFFile(importFile.FileName, readFile, type);
                    LibraryEntryHeader libHead = new LibraryEntryHeader(importFile.FileName);
                    for (int i = 0; i < ibmdf.Materials.Count; i++)
                    {
                        LibraryEntry le = new LibraryEntry(ibmdf.Materials[i].Name, importFile.FileName);
                        //le.Foreground = HelperFunctions.GetBrushFromHex("#000000");
                        libHead.Items.Add(le);
                    }
                    lib.entries.Add(libHead);
                    readFile.Close();
                    LibraryChanged = true;
                }

            }
        }
        private void RemoveFromLibrary(object sender, RoutedEventArgs e)
        {
            if(LibraryView.SelectedItem is LibraryEntryHeader)
            {
                lib.entries.Remove((LibraryEntryHeader)LibraryView.SelectedItem);
                LibraryChanged = true;
            }

        }

        private void NewLibrary(object sender, RoutedEventArgs e)
        {
            lib = new Library();
            defs.lastOpenLib = "";
            LibraryChanged = false;
        }
        private void OpenLibrary(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "MDF Material Library (*.mdflib)|*.mdflib";
            if(openFile.ShowDialog() == true)
            {
                string jsontxt = File.ReadAllText(openFile.FileName);
                Library nlib = JsonSerializer.Deserialize<Library>(jsontxt);
                lib.SetEntries(nlib.entries);
                defs.lastOpenLib = openFile.FileName;
                LibraryChanged = false;
            }
        }
        private void SaveLibrary(object sender, RoutedEventArgs e)
        {
            string jsontxt = JsonSerializer.Serialize(lib);
            File.WriteAllText(defs.lastOpenLib, jsontxt);
            LibraryChanged = false;
        }
        private void SaveLibraryAs(object sender, RoutedEventArgs e)
        {
            string jsontxt = JsonSerializer.Serialize(lib);
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "MDF Material Library (*.mdflib)|*.mdflib";
            if(saveFile.ShowDialog() == true)
            {
                File.WriteAllText(saveFile.FileName, jsontxt);
                defs.lastOpenLib = saveFile.FileName;
                LibraryChanged = false;
            }

        }
        private void DefsSetColor()
        {
            defs.background = BackgroundColor.Color;
            defs.foreground = ForegroundColor.Color;
            defs.buttons = ButtonsColor.Color;
            defs.windows = WindowsColor.Color;
            defs.text = TextColor.Color;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DefsSetColor();
            if (LibraryChanged)
            {
                if(MessageBox.Show("Save changes to the library?","",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if(defs.lastOpenLib == "")
                    {
                        SaveLibraryAs(sender, new RoutedEventArgs());
                    }
                    else
                    {
                        SaveLibrary(sender, new RoutedEventArgs());
                    }
                }
            }
            if (CompendiumChanged)
            {
                if (MessageBox.Show("Save changes to the compendium?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SaveCompendium(sender, new RoutedEventArgs());
                }
            }
            defs.lastOpenFiles.Clear();
            for(int i = 0; i < MDFs.Count; i++)
            {
                defs.lastOpenFiles.Add(MDFs[i].FileName);
            }
            
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string jsontxt = JsonSerializer.Serialize(defs,options);
            File.WriteAllText("defs.json",jsontxt);
        }

        private void MoveMatUp(object sender, RoutedEventArgs e)
        {
            Button send = sender as Button;
            ListBox senderBox = send.DataContext as ListBox;
            //this approach is weird but I'm surprised it works
            if (senderBox != null)
            {
                Material senderMat = (Material)senderBox.SelectedItem;
                if(senderMat != null)
                {
                    int oldIndex = senderMat.materialIndex;
                    if (oldIndex > 0)
                    {
                        //obviously cannot go past first index
                        HelperFunctions.Swap(MDFs[MaterialView.SelectedIndex].Materials, oldIndex - 1, oldIndex);
                        senderBox.SelectedIndex = oldIndex - 1;
                        send.DataContext = senderBox;
                    }

                }

            }
            UpdateMaterials();
        }
        private void MoveMatDown(object sender, RoutedEventArgs e)
        {
            Button send = sender as Button;
            ListBox senderBox = send.DataContext as ListBox;
            //this approach is weird but I'm surprised it works
            if(senderBox != null)
            {
                Material senderMat = (Material)senderBox.SelectedItem;
                if(senderMat != null)
                {
                    int oldIndex = senderMat.materialIndex;
                    if (oldIndex < MDFs[MaterialView.SelectedIndex].Materials.Count - 1)
                    {
                        //obviously cannot go past last index
                        HelperFunctions.Swap(MDFs[MaterialView.SelectedIndex].Materials, oldIndex + 1, oldIndex);
                        senderBox.SelectedIndex = oldIndex + 1;
                        send.DataContext = senderBox;
                    }

                }

            }
            UpdateMaterials();

        }
        public void UpdateWindowBrushes()
        {
            Resources["BackgroundColor"] = BackgroundColor;
            Resources["ForegroundColor"] = ForegroundColor;
            Resources["WindowsColor"] = WindowsColor;
            Resources["ButtonColor"] = ButtonsColor;
            Resources["TextColor"] = TextColor;
        }

        private void ThemeOpen(object sender, RoutedEventArgs e)
        {
            ThemeManager themeManager = new ThemeManager();
            themeManager.BackgroundColor = BackgroundColor;
            themeManager.ForegroundColor = ForegroundColor;
            themeManager.ButtonColor = ButtonsColor;
            themeManager.WindowsColor = WindowsColor;
            themeManager.TextColor = TextColor;
            if(themeManager.ShowDialog() == true)
            {
                BackgroundColor = themeManager.BackgroundColor;
                ForegroundColor = themeManager.ForegroundColor;
                WindowsColor = themeManager.WindowsColor;
                ButtonsColor = themeManager.ButtonColor;
                TextColor = themeManager.TextColor;
                UpdateWindowBrushes();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();

            var item = MaterialView.Items.Cast<MDFFile>().Where(i => i.Header.Equals(tabName)).SingleOrDefault();

            MDFFile tab = item as MDFFile;

            if (tab != null)
            {
                // get selected tab
                MDFFile selectedTab = MaterialView.SelectedItem as MDFFile;

                if (MessageBox.Show(string.Format("Would you like to save before closing?", tab.Header.ToString()),
                    "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    BinaryWriter bw = HelperFunctions.OpenFileW(tab.Header, Encoding.Unicode);
                    if(bw != null)
                    {
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(tab.Header).Replace(".", ""));
                        tab.Export(bw, type);
                        bw.Close();
                    }

                }


                MDFs.Remove(tab);

                // select previously selected tab. if that is removed then select first tab
                if ((selectedTab == null || selectedTab.Equals(tab)) && (MDFs.Count > 0))
                {
                    selectedTab = MDFs[0];
                }
                MaterialView.SelectedItem = selectedTab;
            }
        }

        private void ExtendCompendium(OpenFileDialog dialog)
        {
            //set some error info up here for reference if crash
            string lastMDF = " ";
            MDFTypes lastType = MDFTypes.RE2DMC5;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string fullPath = dialog.FileName;
                    string searchPath = System.IO.Path.GetDirectoryName(fullPath);
                    EnumerationOptions enops = new EnumerationOptions();
                    enops.RecurseSubdirectories = true;
                    string[] mdfs = System.IO.Directory.GetFiles(searchPath, "*.mdf2.*", enops);//brilliant
                    foreach (string mdf in mdfs)
                    {
                        //gonna abuse the fact everything is by reference
                        string sType = System.IO.Path.GetExtension(mdf.ReplaceInsensitive(".stm", "").ReplaceInsensitive(".x64", "")).Replace(".","");
                        MDFTypes type = (MDFTypes)Convert.ToInt32(sType);
                        lastMDF = mdf;
                        lastType = type;
                        CompendiumTopLevel parent = null;
                        switch (type)
                        {
                            case MDFTypes.RE7:
                                parent = compendium.RE7;
                                break;
                            case MDFTypes.RE2DMC5:
                                parent = compendium.RE2DMC5;
                                break;
                            case MDFTypes.RE3:
                                parent = compendium.RE3;
                                break;
                            case MDFTypes.MHRiseRE8:
                                parent = compendium.MHRiseRE8;
                                break;
                            case MDFTypes.RERT:
                                parent = compendium.RERT;
                                break;
                            case MDFTypes.Sunbreak:
                                parent = compendium.Sunbreak;
                                break;
                            default:
                                break;
                        }
                        if (parent != null)
                        {
                            BinaryReader br = HelperFunctions.OpenFileR(mdf, Encoding.Unicode);
                            if (br != null)
                            {
                                MDFFile file = new MDFFile(mdf, br, type);
                                foreach (Material mat in file.Materials)
                                {
                                    CompendiumEntryHeader mParent = null;
                                    mParent = parent.FindItem(x => x.MMTRName == System.IO.Path.GetFileNameWithoutExtension(mat.MasterMaterial));
                                    if (mParent == null)
                                    {
                                        mParent = new CompendiumEntryHeader(System.IO.Path.GetFileNameWithoutExtension(mat.MasterMaterial));
                                        parent.AddChild(mParent);
                                    }
                                    int check = mParent.FindEntry(mdf);
                                    if (check == -1)
                                    {
                                        CompendiumEntry ce = new CompendiumEntry(mdf);
                                        mParent.Items.Add(ce);
                                    }
                                }
                            }
                        } 
                    }
                    CompendiumChanged = true;
                    compendium.Sort();
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"An error occurred during reading compendium entries:{ex}\n\n" +
                        $"Diagnostic Information: MDF:{lastMDF} Version:{lastType}");
                }
            }
        }
        private void RebaseCompendium(object sender, RoutedEventArgs e)
        {
            compendium.ClearList();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.FileName = "Rebase Compendium here";
            ExtendCompendium(dialog);
        }
        private void ExpandCompendium(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.FileName = "Expand Compendium here";
            ExtendCompendium(dialog);
        }
        private void OpenCompendium(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "MDF Material Compendium (*.mdfcomp)|*.mdfcomp";
            if (openFile.ShowDialog() == true)
            {
                string jsontxt = File.ReadAllText(openFile.FileName);
                Compendium nlib = JsonSerializer.Deserialize<Compendium>(jsontxt);
                compendium.SetEntries(nlib.entries);
                defs.lastOpenComp = openFile.FileName;
                CompendiumChanged = false;
            }
        }
        private void SaveCompendium(object sender, RoutedEventArgs e)
        {
            if (defs.lastOpenComp == "")
            {
                SaveCompendiumAs(sender, e);
            }
            string jsontxt = JsonSerializer.Serialize(compendium);
            File.WriteAllText(defs.lastOpenComp, jsontxt);
            CompendiumChanged = false;
        }
        private void SaveCompendiumAs(object sender, RoutedEventArgs e)
        {
            string jsontxt = JsonSerializer.Serialize(compendium);
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "MDF Material Compendium (*.mdfcomp)|*.mdfcomp";
            if (saveFile.ShowDialog() == true)
            {
                File.WriteAllText(saveFile.FileName, jsontxt);
                defs.lastOpenComp = saveFile.FileName;
                CompendiumChanged = false;
            }

        }

        private void CompendiumView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                if (CompendiumView.SelectedItem != null && CompendiumView.SelectedItem is CompendiumEntry)
                {
                    IsDragging = true;
                    CompendiumEntry entry = (CompendiumEntry)CompendiumView.SelectedItem;
                    DragDrop.DoDragDrop(CompendiumView, CompendiumView.SelectedValue,
                        DragDropEffects.Copy);
                    IsDragging = false;
                }
            }
        }

        private void LibraryView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CompendiumEntry)))
            {
                CompendiumEntry entry = (CompendiumEntry)e.Data.GetData(typeof(CompendiumEntry));
                BinaryReader readFile = HelperFunctions.OpenFileR(entry.MDFPath, Encoding.Unicode);
                if (readFile != null)
                {
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(entry.MDFPath).Replace(".", ""));
                    MDFFile ibmdf = new MDFFile(entry.MDFPath, readFile, type);
                    LibraryEntryHeader libHead = new LibraryEntryHeader(entry.MDFPath);
                    for (int i = 0; i < ibmdf.Materials.Count; i++)
                    {
                        LibraryEntry le = new LibraryEntry(ibmdf.Materials[i].Name, entry.MDFPath);
                        //le.Foreground = HelperFunctions.GetBrushFromHex("#000000");
                        libHead.Items.Add(le);
                    }
                    lib.entries.Add(libHead);
                    readFile.Close();
                    LibraryChanged = true;
                }
            }
        }

        private void LibraryView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(CompendiumEntry)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MaterialView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BatchConvert(object sender, RoutedEventArgs e)
        {
            BatchConverter batch = new BatchConverter();
            batch.Show();
        }
    }
    public class PropertySelect : DataTemplateSelector
    {
        public DataTemplate FloatTemplate { get; set; }
        public DataTemplate Float4Template { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is FloatProperty)
                {
                    return FloatTemplate;
                }
                else if (item is Float4Property)
                {
                    return Float4Template;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
    public class LibSelect : DataTemplateSelector
    {
        public DataTemplate LibEntry { get; set; }
        public DataTemplate LibEntryHead { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is LibraryEntry)
                {
                    return LibEntry;
                }
                else if (item is LibraryEntryHeader)
                {
                    return LibEntryHead;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
    public class CompSelect : DataTemplateSelector
    {
        public DataTemplate CompEntry { get; set; }
        public DataTemplate CompEntryHead { get; set; }
        public DataTemplate CompTopLevel { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is CompendiumEntry)
                {
                    return CompEntry;
                }
                else if (item is CompendiumEntryHeader)
                {
                    return CompEntryHead;
                }
                else if(item is CompendiumTopLevel)
                {
                    return CompTopLevel;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

}
