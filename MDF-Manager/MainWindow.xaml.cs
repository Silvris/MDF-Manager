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
                    BinaryReader readFile = new BinaryReader(new FileStream(defs.lastOpenFiles[i], FileMode.Open), Encoding.Unicode);
                    MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(defs.lastOpenFiles[i]).Replace(".", ""));
                    MDFs.Add(new MDFFile(defs.lastOpenFiles[i], readFile, type));
                    readFile.Close();
                }
            }
            InitializeComponent();
            MaterialView.DataContext = this;
            LibraryView.DataContext = this;
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
                BinaryReader readFile = new BinaryReader(new FileStream(importFile.FileName, FileMode.Open), Encoding.Unicode);
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(importFile.FileName).Replace(".", ""));
                MDFs.Add(new MDFFile(importFile.FileName,readFile,type));
                readFile.Close();
            }
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (!CheckMaterialNames(MaterialView.SelectedIndex))
            {
                MessageBox.Show("Material names cannot be identical!");
            }
            BinaryWriter bw = new BinaryWriter(new FileStream(MDFs[MaterialView.SelectedIndex].Header, FileMode.OpenOrCreate), Encoding.Unicode);
            MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[MaterialView.SelectedIndex].Header).Replace(".", ""));
            MDFs[MaterialView.SelectedIndex].Export(bw, type);
            bw.Close();
        }
        private void SaveAs(object sender, RoutedEventArgs e)
        {
            if (!CheckMaterialNames(MaterialView.SelectedIndex))
            {
                MessageBox.Show("Material names cannot be identical!");
            }
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "All readable files|*.6;*.10;*.13;*.19|RE7 Material file (*.6)|*.6|RE2/DMC5 Material file (*.10)|*.10|RE3 Material file (*.13)|*.13|RE8/MHRise Material file (*.19)|*.19";
            saveFile.FileName = System.IO.Path.GetFileName(MDFs[MaterialView.SelectedIndex].Header);
            if (saveFile.ShowDialog() == true)
            {
                BinaryWriter bw = new BinaryWriter(new FileStream(saveFile.FileName, FileMode.OpenOrCreate), Encoding.Unicode);
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(saveFile.FileName).Replace(".", ""));
                MDFs[MaterialView.SelectedIndex].Export(bw, type);
                bw.Close();
                MDFs[MaterialView.SelectedIndex].Header = saveFile.FileName;
            }
        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < MDFs.Count; i++)
            {
                if (!CheckMaterialNames(i))
                {
                    MessageBox.Show("Material names cannot be identical!");
                }
                BinaryWriter bw = new BinaryWriter(new FileStream(MDFs[i].Header, FileMode.OpenOrCreate), Encoding.Unicode);
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(MDFs[i].Header).Replace(".", ""));
                MDFs[i].Export(bw, type);
                bw.Close();
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
                LibraryEntry entry = (LibraryEntry)e.Data.GetData(typeof(LibraryEntry));
                BinaryReader readFile = new BinaryReader(new FileStream(entry.MDFPath, FileMode.Open), Encoding.Unicode);
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
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for(int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(".6") || files[i].Contains(".10") || files[i].Contains(".13") || files[i].Contains(".19"))
                    {
                        BinaryReader readFile = new BinaryReader(new FileStream(files[i], FileMode.Open), Encoding.Unicode);
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
                BinaryReader readFile = new BinaryReader(new FileStream(importFile.FileName, FileMode.Open), Encoding.Unicode);
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
            lib.entries.Remove((LibraryEntryHeader)LibraryView.SelectedItem);
            LibraryChanged = true;
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
            string jsontxt = JsonSerializer.Serialize(defs);
            File.WriteAllText("defs.json",jsontxt);
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
