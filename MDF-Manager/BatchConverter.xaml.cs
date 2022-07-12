using MDF_Manager.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MDF_Manager
{
    /// <summary>
    /// Interaction logic for BatchConverter.xaml
    /// </summary>
    public partial class BatchConverter : Window
    {
        private List<MDFTypes> MDFTypeList = new List<MDFTypes>();
        public List<MDFTypes> TypeList { get => MDFTypeList; }
        public MDFTypes ExportType { get; set; }
        public BatchConverter()
        {
            InitializeComponent();
            foreach(MDFTypes type in Enum.GetValues(typeof(MDFTypes)))
            {
                MDFTypeList.Add(type);
            }
            ExportType = MDFTypes.Sunbreak;
            DataContext = this;
        }

        private void AddToList(object sender, RoutedEventArgs e)
        {
            OpenFileDialog importFile = new OpenFileDialog();
            importFile.Multiselect = true;
            importFile.Filter = MainWindow.MDFFilter;
            if (importFile.ShowDialog() == true)
            {
                foreach(string file in importFile.FileNames)
                {
                    MDFView.Items.Add(new ListBoxItem() { Content = file });
                }
            }
        }

        private void RemoveSelected(object sender, RoutedEventArgs e)
        {
            MDFView.Items.Remove(MDFView.SelectedItem);
            MDFView.SelectedIndex = 0;
        }

        public void ConvertSingle(string path, string output)
        {
            BinaryReader readFile = HelperFunctions.OpenFileR(path, Encoding.Unicode);
            if (readFile != null)
            {
                MDFTypes type = (MDFTypes)Convert.ToInt32(System.IO.Path.GetExtension(path).Replace(".", ""));
                MDFFile file = new MDFFile(path, readFile, type);
                //now export with proper format/path
                string outPath = Path.ChangeExtension(Path.Combine(output, Path.GetFileName(path)), ((int)ExportType).ToString());
                BinaryWriter bw = HelperFunctions.OpenFileW(outPath, Encoding.Unicode);
                if(bw != null)
                {
                    file.Export(bw, ExportType);
                }
                bw.Close();
            }
            readFile.Close();
        }

        public void ConvertAll(object sender, RoutedEventArgs e)
        {
            OpenFileDialog exportFile = new OpenFileDialog();
            exportFile.CheckFileExists = false;
            exportFile.FileName = "Save Here";
            if(exportFile.ShowDialog() == true)
            {
                foreach (ListBoxItem item in MDFView.Items)
                {
                    string path = item.Content as string;
                    ConvertSingle(path, System.IO.Path.GetDirectoryName(exportFile.FileName));
                }
                MessageBox.Show("Batch conversion completed successfully!");
                this.Close();
            }
        }
    }
}
