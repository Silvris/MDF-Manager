using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MDF_Manager.Classes
{
    public class MDFTab : TabItem
    {
        public MDFFile mdf;
        public MDFTab(MDFFile mdfFile)
        {
            mdf = mdfFile;
            Header = mdf.Header;
        }
    }
}
