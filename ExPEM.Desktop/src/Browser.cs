using System.Windows;
using System.Windows.Controls;

namespace ExPEM.Desktop
{
    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = 
            DependencyProperty.RegisterAttached(
                "Html", 
                typeof(string), 
                typeof(BrowserBehavior), 
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser browser)
        {
            return (string)browser.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser browser, string value)
        {
            browser.SetValue(HtmlProperty, value);
        }

        private static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = dependencyObject as WebBrowser;
            browser?.NavigateToString((string)e.NewValue);
        }
    }
}
