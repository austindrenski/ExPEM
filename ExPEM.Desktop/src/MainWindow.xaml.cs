using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace ExPEM.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            XElement model = XElement.Parse(
                @"<Root>
                      <A a=""test1"" b=""test2"">
                          <B a=""test1"" b=""test2""/>
                          <C a=""test1"" b=""test2""/>
                      </A>
                      <D a=""test1"" b=""test2"">
                          <B a=""test1"" b=""test2""/>
                          <C a=""test1"" b=""test2""/>
                      </D>
                  </Root>"
                );

            model = Example.Example1();

            BindXDocumentToDataGrid(model);
        }

        /// <summary>
        /// Binds an XDocument to the DataGrid.
        /// </summary>
        /// <param name="grid">The DataGrid to which the model is bound.</param>
        /// <param name="model">The model element to bind to the DataGrid.</param>
        public void BindXDocumentToDataGrid([NotNull] XElement model)
        {
            DataGrid grid = new DataGrid() {RowDetailsTemplate = new DataTemplate("{StaticResource ExpandableGrid}")};
            MainGrid.Children.Add(grid);

            DataGridTextColumn name = new DataGridTextColumn
            {
                Header = "Market",
                Binding = new Binding("Name")
            };
            grid.Columns.Add(name);
            foreach (XName property in model.Attributes().Select(x => x.Name).Distinct())
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = property,
                    Binding =  new Binding($"Attribute[{property}].Value")
                };
                grid.Columns.Add(column);
            }
            grid.Items.Add(model);
            grid.RowDetailsVisibilityChanged += LoadRowDetails;
        }

        private static void BuildGrid(DataGrid grid, XElement element)
        {
            DataGridTextColumn name = new DataGridTextColumn
            {
                Header = "Market",
                Binding = new Binding("Name")
            };
            grid.Columns.Add(name);
            foreach (XName property in element.Elements().Attributes().Select(x => x.Name).Distinct())
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = property,
                    Binding = new Binding($"Attribute[{property}].Value")
                };
                grid.Columns.Add(column);
            }
            foreach (XElement item in element.Elements())
            {
                grid.Items.Add(item);
            }
            grid.RowDetailsVisibilityChanged += LoadRowDetails;
        }

        private static void LoadRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            XElement element = (XElement) e.Row.Item;
            if (!element.HasElements)
            {
                return;
            }
            DataGrid grid = e.DetailsElement as DataGrid;
            if (grid == null)
            {
                return;
            }
            BuildGrid(grid, element);
        }
    }
}
