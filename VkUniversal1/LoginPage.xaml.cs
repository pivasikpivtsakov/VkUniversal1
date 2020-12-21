using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.EntityFrameworkCore;
using VkUniversal1.DbContext;

namespace VkUniversal1
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(CustomTitleBar);
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }

        private void AuthWithKnownToken(SessionInfo sessionInfo)
        {
            VkObjects.InitializeApi();
            Frame.Navigate(typeof(MainPage));
        }

        private void LoginWebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var authUri = args.Uri.ToString();
            if (authUri.Contains("access_token="))
            {
                var accessToken = authUri.Split("access_token=")[1].Split("&")[0];
                Console.WriteLine($"Token is: {accessToken}");
                using (var db = new CacheDbContext())
                {
                    db.SessionInfo.Add(new SessionInfo
                    {
                        AccessToken = accessToken
                    });
                    db.SaveChanges();
                }

                VkObjects.InitializeApi();
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void LoginWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            Console.WriteLine("No connection");
            Frame.Navigate(typeof(MainPage));
        }

        private void LoginWebView_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (var db = new CacheDbContext())
            {
                if (db.SessionInfo.Any())
                {
                    var sessionInfo = db.SessionInfo.First();
                    if (!string.IsNullOrWhiteSpace(sessionInfo.AccessToken))
                    {
                        AuthWithKnownToken(sessionInfo);
                        return;
                    }
                }
            }

            LoginWebView.Navigate(new Uri(
                $"https://oauth.vk.com/authorize?client_id={VkConstants.AppId}&display=page&scope={VkConstants.AppScope}&redirect_uri=https://oauth.vk.com/blank.html&response_type=token&v=5.103&state=success"
            ));
        }
    }
}