using System;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RocketManiaClone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public enum Neighbour
    {
        Left,
        Top,
        Right,
        Bottom,
    }

    public class Pair<K, V>
    {
        public K First { get; set; }
        public V Second { get; set; }

        public Pair(K first, V second)
        {
            First = first;
            Second = second;
        }
    }

    public class RocketConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int v);
            double l = v - 566.0;
            double t = App.tg25 * l;
            return new Thickness(l, t, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public partial class App : Application
    {
        public const double tg25 = 0.46630765815;

        public static readonly Random randomGenerator = new Random();

        public static readonly RocketConverter rocketConverter = new RocketConverter();

        public static readonly Pen blackPen = new Pen(Brushes.Black, 1);

        public static T[] Ar<T>(params T[] ar)
        {
            return ar;
        }
    }
}
