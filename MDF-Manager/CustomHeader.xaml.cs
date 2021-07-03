using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CustomHeader : UserControl
    {
        public CustomHeader()
        {
            InitializeComponent();
        }

    }
    public class RelationalValueConverter : IMultiValueConverter
    {
        public enum RelationsEnum
        {
            Gt, Lt, Gte, Lte, Eq, Neq
        }

        public RelationsEnum Relations { get; protected set; }

        public RelationalValueConverter(RelationsEnum relations)
        {
            Relations = relations;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException(@"Must have two parameters", "values");

            var v0 = values[0] as IComparable;
            var v1 = values[1] as IComparable;

            if (v0 == null || v1 == null)
                throw new ArgumentException(@"Must arguments must be IComparible", "values");

            var r = v0.CompareTo(v1);

            switch (Relations)
            {
                case RelationsEnum.Gt:
                    return r > 0;
                case RelationsEnum.Lt:
                    return r < 0;
                case RelationsEnum.Gte:
                    return r >= 0;
                case RelationsEnum.Lte:
                    return r <= 0;
                case RelationsEnum.Eq:
                    return r == 0;
                case RelationsEnum.Neq:
                    return r != 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class GtConverter : RelationalValueConverter
    {
        public GtConverter() : base(RelationsEnum.Gt) { }
    }
    public class GteConverter : RelationalValueConverter
    {
        public GteConverter() : base(RelationsEnum.Gte) { }
    }
    public class LtConverter : RelationalValueConverter
    {
        public LtConverter() : base(RelationsEnum.Lt) { }
    }
    public class LteConverter : RelationalValueConverter
    {
        public LteConverter() : base(RelationsEnum.Lte) { }
    }
    public class EqConverter : RelationalValueConverter
    {
        public EqConverter() : base(RelationsEnum.Eq) { }
    }
    public class NeqConverter : RelationalValueConverter
    {
        public NeqConverter() : base(RelationsEnum.Neq) { }
    }
}
