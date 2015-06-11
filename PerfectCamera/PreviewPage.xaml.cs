using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace PerfectCamera
{
    public partial class PreviewPage : PhoneApplicationPage
    {
        private BackgroundWorker _loadBitmapWorkder = null;
        List<PreviewGrid> _previewImageList = new List<PreviewGrid>();
        public PreviewPage()
        {
            InitializeComponent();

            _loadBitmapWorkder = new BackgroundWorker();
            _loadBitmapWorkder.DoWork += _loadBitmapWorkder_DoWork;
            _loadBitmapWorkder.WorkerSupportsCancellation = true;

            InitView();
        }

        void _loadBitmapWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < _previewImageList.Count ; i++)
            {
                PreviewGrid bitmap = _previewImageList[i];
                if (!bitmap.ImageLoaded)
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            bitmap.LoadImage();
                        });
                }
            }
        }

        private void InitView()
        {
            int selectedIndex = -1;
            Photo selectedPhoto = null;
            if (PhoneApplicationService.Current.State.ContainsKey("SelectedPhoto"))
            {
                selectedPhoto = PhoneApplicationService.Current.State["SelectedPhoto"] as Photo;
            }
            using (MediaLibrary library = new MediaLibrary())
            {
                PictureCollection lst = library.Pictures;
                for (int i = 0; i < lst.Count; i++)
                {
                    Picture picture = lst[i];

                    PivotItem pItem = new PivotItem();

                    PreviewGrid grid = new PreviewGrid(picture);
                    _previewImageList.Add(grid);

                    pItem.Content = grid;

                    ImagePreviewPivot.Items.Add(pItem);

                    if (selectedPhoto != null && picture.Equals(selectedPhoto.Picture))
                    {
                        selectedIndex = i;
                    }
                }
            }

            //ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;
            
            if (selectedIndex >= 0)
            {
                ImagePreviewPivot.SelectedIndex = selectedIndex;
            }
        }

        private void ImagePreviewPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ImagePreviewPivot.SelectedIndex;

            TitleTextBlock.Text = string.Format("{0}/{1}", selectedIndex + 1, ImagePreviewPivot.Items.Count);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                _loadBitmapWorkder.RunWorkerAsync();

                try
                {
                    string enabledAlbumButton = "true";

                    if (!NavigationContext.QueryString.TryGetValue("album_enabled", out enabledAlbumButton))
                    {
                        enabledAlbumButton = "true";
                    }

                    if (enabledAlbumButton == "true")
                    {
                        AlbumButton.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        AlbumButton.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                _loadBitmapWorkder.CancelAsync();
            }
            base.OnNavigatedFrom(e);
        }

        private void PreviewPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void AlbumButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AlbumPage.xaml", UriKind.Relative));
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImagePreviewPivot.SelectedIndex >= 0 && ImagePreviewPivot.SelectedIndex < ImagePreviewPivot.Items.Count)
            {
                PreviewGrid grid = _previewImageList[ImagePreviewPivot.SelectedIndex];
                PhoneApplicationService.Current.State["EditPicture"] = grid.DisplayPicture;
            }
            NavigationService.Navigate(new Uri("/EditPage.xaml", UriKind.Relative));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}