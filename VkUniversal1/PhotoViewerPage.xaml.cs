using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VkUniversal1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoViewerPage : Page
    {
        public PhotoViewerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string parameter)
            {
                var photoUri = new Uri(parameter);
                Image.Source = await ImageUtils.ConvertToImageSource(await ImageUtils.UriToByte(photoUri));
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var currentView = ApplicationView.GetForCurrentView();
            var newViewMode = currentView.ViewMode == ApplicationViewMode.Default
                ? ApplicationViewMode.CompactOverlay
                : ApplicationViewMode.Default;
            if (!currentView.IsViewModeSupported(newViewMode)) return;
            if (currentView.ViewMode != newViewMode)
            {
                var viewEnteredCompactOverlay =
                    await currentView.TryEnterViewModeAsync(newViewMode);
            }
        }
    }
}