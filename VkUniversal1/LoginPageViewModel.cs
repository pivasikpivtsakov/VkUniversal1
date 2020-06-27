﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VkUniversal1
{
    public class LoginPageViewModel
    {
        public void ParseTokenFromUrl(string url)
        {
            if (url.Contains("access_token="))
            {
                var accessToken = url.Split("access_token=")[1].Split("&")[0];
                using (var db = new CacheDbContext())
                {
                    db.SessionInfo.Add(new SessionInfo
                    {
                        AccessToken = accessToken
                    });
                    db.SaveChanges();
                }

                VKObjects.InitializeApi();
                (Window.Current.Content as Frame)?.Navigate(typeof(MainPage));
            }
        }
    }
}