using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PerfectCamera.Resources;

namespace PerfectCamera
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        private void SelfieCameraButton_Click(object sender, RoutedEventArgs e)
        {
            PerfectCamera.DataContext.Instance.CameraType = PerfectCameraType.Selfie;
            NavigationService.Navigate(new Uri("/CameraPage.xaml", UriKind.Relative));
        }

        private void EasyCamButton_Click(object sender, RoutedEventArgs e)
        {
            PerfectCamera.DataContext.Instance.CameraType = PerfectCameraType.EasyCam;
            NavigationService.Navigate(new Uri("/CameraPage.xaml", UriKind.Relative));
        }

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AlbumPage.xaml", UriKind.Relative));
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}