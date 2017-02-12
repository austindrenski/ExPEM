using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace ExPEM.Desktop
{
    internal static class BuildGrid
    {
        /// <summary>
        /// Binds an XElement to the DataGrid.
        /// </summary>
        /// <param name="grid">The DataGrid to which the element is bound.</param>
        /// <param name="element">The element to bind to the DataGrid.</param>
        internal static void Build(DataGrid grid, XElement element)
        {
            grid.Columns.Add(new DataGridTextColumn { Header = "Market", Binding = new Binding("Name") });

            IEnumerable<XAttribute> attributes = element.HasElements ? element.Elements().Attributes() : element.Attributes();
            foreach (XName property in attributes.Select(x => x.Name).Distinct())
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = property,
                    Binding = new Binding($"Attribute[{property}].Value")
                };
                grid.Columns.Add(column);
            }

            IEnumerable<XElement> elements = grid.Name == "RootDataGrid" ? new XElement[] { element } : element.Elements();
            foreach (XElement item in elements)
            {
                grid.Items.Add(item);
            }

            grid.RowDetailsVisibilityChanged += Build;
        }

        private static void Build(object sender, DataGridRowDetailsEventArgs e)
        {
            XElement element = (XElement)e.Row.Item;
            if (!element.HasElements)
            {
                e.DetailsElement.Visibility = Visibility.Collapsed;
                return;
            }
            DataGrid grid = e.DetailsElement as DataGrid;
            if (grid == null)
            {
                return;
            }
            Build(grid, element);
        }
    }
}
