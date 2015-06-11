using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PerfectCamera
{
    public partial class AlbumPreviewPage : PhoneApplicationPage
    {
        public AlbumPreviewPage()
        {
            InitializeComponent();

            Album p = PhoneApplicationService.Current.State["SelectedAlbum"] as Album;
            PhotoHubLLS.ItemsSource = LibraryDataService.Instance.GetGroupedPhotosFromAlbum(p);
            TitleTextBlock.Text = p.AlbumName;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AlbumPreviewPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}