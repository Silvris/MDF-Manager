using System.Windows;
using System.Drawing;

namespace MDF_Manager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ColorCanvas : Window
    {

        public ColorCanvas()
        {
            InitializeComponent();

        }

        public void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
