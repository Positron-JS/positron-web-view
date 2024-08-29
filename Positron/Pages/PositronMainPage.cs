using Positron.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Positron.Pages
{
    public class PositronMainPage: ContentPage
    {

        public readonly PositronWebView WebView;

        public string? Url { get; set; }

        public PositronMainPage()
        {
            WebView = new PositronWebView() {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var grid = new Grid();
            grid.Children.Add(WebView);
            this.Content = grid;
            // this.Content = WebView;
            Dispatcher.DispatchTaskDelayed(TimeSpan.FromMilliseconds(1), this.Ask);
        }

        private async Task Ask()
        {

            this.Url ??= Microsoft.Maui.Storage.Preferences.Default.Get<string>("url", null!);

            if (this.Url != null)
            {
                this.WebView.Source = new UrlWebViewSource { Url = Url };
                return;
            }

            while (true)
            {

                var url = await this.DisplayPromptAsync("Url", "Enter URL");
                if (url == null)
                {
                    continue;
                }

                this.Url = url;
                Microsoft.Maui.Storage.Preferences.Default.Set("url", url);

                this.WebView.Source = new UrlWebViewSource {  Url = url };
                break;
            }
        }
    }
}
