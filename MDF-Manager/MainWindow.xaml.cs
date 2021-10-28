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
        public ObservableCollection<MDFFile> MDFs { get; set; }
        public Defs defs { get; set; }
        public Library lib { get; set; }
        public bool LibraryChanged = false;
        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
        public SolidColorBrush WindowsColor { get; set; }
        public SolidColorBrush ButtonsColor { get; set; }
        public SolidColorBrush TextColor { get; set; }

        bool IsDragging;

        public MainWindow()
        {
            lib = new Library();
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
            MDFs = new ObservableCollection<MDFFile>();
            if(defs.lastOpenFiles.Count > 0)
            {
                for(int i = 0; i < defs.lastOpenFiles.Count; i++)
                {
                    if (File.Exists(defs.lastOpenFiles[i]))
                    {
                        BinaryReader readFile = HelperFunctions.OpenFileR(defs.lastOpenFiles[i], Encoding.Unicode);
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(defs.lastOpenFiles[i]).Replace(".", ""));
                        MDFs.Add(new MDFFile(defs.lastOpenFiles[i], readFile, type));
                        readFile.Close();
                    }

                }
            }
            InitializeComponent();
            MaterialView.DataContext = this;
            LibraryView.DataContext = this;
            UpdateWindowBrushes();
        }

        void InitializeBrushes(Defs def)
        {

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
            importFile.Filter = "All readable files|*.6;*.10;*.13;*.19|RE7 Material file (*.6)|*.6|RE2/DMC5 Material file (*.10)|*.10|RE3 Material file (*.13)|*.13|RE8/MHRise Material file (*.19)|*.19";
            if (importFile.ShowDialog() == true)
            {
                BinaryReader readFile = HelperFunctions.OpenFileR(importFile.FileName, Encoding.Unicode);
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(importFile.FileName).Replace(".", ""));
                MDFs.Add(new MDFFile(importFile.FileName,readFile,type));
                readFile.Close();
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
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[MaterialView.SelectedIndex].Header).Replace(".", ""));
                    MDFs[MaterialView.SelectedIndex].Export(bw, type);
                    bw.Close();
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
                    saveFile.Filter = "All readable files|*.6;*.10;*.13;*.19|RE7 Material file (*.6)|*.6|RE2/DMC5 Material file (*.10)|*.10|RE3 Material file (*.13)|*.13|RE8/MHRise Material file (*.19)|*.19";
                    saveFile.FileName = System.IO.Path.GetFileName(MDFs[MaterialView.SelectedIndex].Header);
                    if (saveFile.ShowDialog() == true)
                    {
                        BinaryWriter bw = HelperFunctions.OpenFileW(saveFile.FileName, Encoding.Unicode);
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(saveFile.FileName).Replace(".", ""));
                        MDFs[MaterialView.SelectedIndex].Export(bw, type);
                        bw.Close();
                        MDFs[MaterialView.SelectedIndex].Header = saveFile.FileName;
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
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[i].Header).Replace(".", ""));
                    MDFs[i].Export(bw, type);
                    bw.Close();
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
            if (!e.Data.GetDataPresent(typeof(LibraryEntry)))
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void TabablzControl_Drop(object sender, DragEventArgs e)
        {
            //library dropping
            if (e.Data.GetDataPresent(typeof(LibraryEntry)))
            {
                if(MDFs.Count > 0)
                {
                    LibraryEntry entry = (LibraryEntry)e.Data.GetData(typeof(LibraryEntry));
                    BinaryReader readFile = HelperFunctions.OpenFileR(entry.MDFPath,Encoding.Unicode);
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(entry.MDFPath).Replace(".", ""));
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
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for(int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(".6") || files[i].Contains(".10") || files[i].Contains(".13") || files[i].Contains(".19"))
                    {
                        BinaryReader readFile = HelperFunctions.OpenFileR(files[i], Encoding.Unicode);
                        MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(files[i]).Replace(".", ""));
                        MDFs.Add(new MDFFile(files[i], readFile, type));
                        readFile.Close();
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
            importFile.Filter = "All readable files|*.6;*.10;*.13;*.19|RE7 Material file (*.6)|*.6|RE2/DMC5 Material file (*.10)|*.10|RE3 Material file (*.13)|*.13|RE8/MHRise Material file (*.19)|*.19";
            if (importFile.ShowDialog() == true)
            {
                BinaryReader readFile = HelperFunctions.OpenFileR(importFile.FileName, Encoding.Unicode);
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(importFile.FileName).Replace(".", ""));
                MDFFile ibmdf = new MDFFile(importFile.FileName, readFile, type);
                LibraryEntryHeader libHead = new LibraryEntryHeader(importFile.FileName);
                for(int i = 0; i < ibmdf.Materials.Count; i++)
                {
                    LibraryEntry le = new LibraryEntry(ibmdf.Materials[i].Name,importFile.FileName);
                    //le.Foreground = HelperFunctions.GetBrushFromHex("#000000");
                    libHead.Items.Add(le);
                }
                lib.entries.Add(libHead);
                readFile.Close();
                LibraryChanged = true;
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
            }

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (LibraryChanged)
            {
                SaveLibraryNotice sln = new SaveLibraryNotice();
                if(sln.ShowDialog() == true)
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

}
