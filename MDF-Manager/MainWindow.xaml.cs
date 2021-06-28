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

namespace MDF_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MDFFile> MDFs { get; set; }
        public MainWindow()
        {
            MDFs = new ObservableCollection<MDFFile>();
            InitializeComponent();
            MaterialView.DataContext = this;
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

        private void TabablzControl_Drop(object sender, DragEventArgs e)
        {

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
}
