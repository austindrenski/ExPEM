using System.Windows;
using System.Xml.Linq;
using AD.PartialEquilibriumApi;

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

            XElement model = Example.Example1();

            BuildGrid.Build(RootDataGrid, model);

            XElement html = ChartFactory.CreateOrganizationalChart(model);

            html.Element("head")?.AddFirst(
                new XElement("meta", 
                    new XAttribute("http-equiv", "X-UA-Compatible"), 
                    new XAttribute("content", "IE=edge")));

            html.Element("body")?.SetAttributeValue("onContextMenu", "return false");

            Tree.SetValue(BrowserBehavior.HtmlProperty, html.ToString());
        }

        public void OpenModelFile(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
