using System.Linq;
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

            XDocument document = XDocument.Parse(
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

            BindXDocumentToDataGrid(Grid, document);

        }

        /// <summary>
        /// Binds an XDocument to the DataGrid.
        /// </summary>
        /// <param name="grid">The DataGrid to which the XDocument is bound.</param>
        /// <param name="document">The XDocument to bind to the DataGrid.</param>
        private static void BindXDocumentToDataGrid(DataGrid grid, [NotNull] XDocument document)
        {
            if (document.Root == null)
            {
                document.Add(new XElement("root"));
            }
            if (!document.Root?.HasElements ?? false)
            {
                document.Root?.Add(new XElement("record"));
            }
            if (!document.Root?.Elements().First()?.HasElements ?? false)
            {
                document.Root?.Elements().First()?.Add(new XElement("errorMessage", "No records found. For further information, contact Austin.Drenski@usitc.gov."));
            }
            DataGridTextColumn name = new DataGridTextColumn
            {
                Header = "Market",
                Binding = new Binding("Name")
            };
            grid.Columns.Add(name);
            foreach (XAttribute property in document.Root?.Elements().FirstOrDefault()?.Attributes() ?? Enumerable.Empty<XAttribute>())
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = property.Name,
                    Binding =  new Binding($"Attribute[{property.Name}].Value")
                };
                grid.Columns.Add(column);
            }
            foreach (XElement element in document.Root?.Elements() ?? Enumerable.Empty<XElement>())
            {
                grid.Items.Add(element);
            }
            grid.RowDetailsVisibilityChanged += LoadRowDetails;
        }


        private static void LoadRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            DataGrid subGrid = e.DetailsElement.FindName("SubGrid") as DataGrid;
            if (subGrid == null)
            {
                return;
            }
            DataGridTextColumn name = new DataGridTextColumn
            {
                Header = "Market",
                Binding = new Binding("Name")
            };
            subGrid.Columns.Add(name);
            foreach (XAttribute property in (e.Row.Item as XElement).Elements().First().Attributes())
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = property.Name,
                    Binding = new Binding($"Attribute[{property.Name}].Value")
                };
                subGrid.Columns.Add(column);
            }
            foreach (XElement element in ((XElement)e.Row.Item).Elements())
            {
                subGrid.Items.Add(element);
            }
        }
    }
}
