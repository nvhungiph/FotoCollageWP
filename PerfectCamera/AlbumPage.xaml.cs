using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;
using PerfectCamera.Helpers;
using System.Collections.ObjectModel;

namespace PerfectCamera
{
    public class Photo
    {
        private Picture _picture = null;
        public Photo(Picture picture)
        {
            _picture = picture;
        }

        public Picture Picture
        {
            get
            {
                return _picture;
            }
        }

        public DateTime Date
        {
            get
            {
                return _picture.Date;
            }
        }

        private BitmapImage _imageSource = null;
        public BitmapImage ImageSource
        {
            get
            {
                if (_imageSource == null)
                {
                    _imageSource = new BitmapImage();
                    _imageSource.SetSource(_picture.GetThumbnail());
                }
                return _imageSource;
            }
        }
    }

    public class Album
    {
        private PictureAlbum _album = null;
        public Album(PictureAlbum album)
        {
            _album = album;
        }

        public PictureAlbum PictureAlbum
        {
            get
            {
                return _album;
            }
        }

        private BitmapImage _thumbnailSource = null;
        public BitmapImage ThumbnailSource
        {
            get
            {
                if (_thumbnailSource == null)
                {
                    _thumbnailSource = new BitmapImage();
                    if (_album.Pictures != null && _album.Pictures.Count > 0)
                    {
                        Picture p = _album.Pictures[_album.Pictures.Count - 1];
                        _thumbnailSource.SetSource(p.GetThumbnail());
                    }
                }

                return _thumbnailSource;
            }
        }

        public string AlbumName
        {
            get
            {
                return _album.Name;
            }
        }
    }

    public class LibraryDataService
    {
        private static LibraryDataService _instance;
        public static LibraryDataService Instance
        {
            get
            {
                return _instance ?? (_instance = new LibraryDataService());
            }
        }

        public ObservableCollection<KeyedList<string, Photo>> GetGroupedPhotos()
        {
            List<Photo> photos = new List<Photo>();
            using (var library = new MediaLibrary())
            {
                PictureCollection lst = library.Pictures;
                for (int i = 0; i < lst.Count; i++)
                {
                    Picture picture = lst[i];

                    photos.Add(new Photo(picture));
                }
            }

            var groupedPhotos =
                from photo in photos
                orderby photo.Date
                group photo by photo.Date.ToString("y") into photosByMonth
                select new KeyedList<string, Photo>(photosByMonth);

            return new ObservableCollection<KeyedList<string, Photo>>(groupedPhotos);
        }

        public ObservableCollection<KeyedList<string, Photo>> GetGroupedPhotosFromAlbum(Album album)
        {
            List<Photo> photos = new List<Photo>();
            if (album != null && album.PictureAlbum != null && album.PictureAlbum.Pictures != null)
            {
                PictureCollection lst = album.PictureAlbum.Pictures;
                for (int i = 0; i < lst.Count; i++)
                {
                    Picture picture = lst[i];

                    photos.Add(new Photo(picture));
                }
            }

            var groupedPhotos =
                from photo in photos
                orderby photo.Date
                group photo by photo.Date.ToString("y") into photosByMonth
                select new KeyedList<string, Photo>(photosByMonth);

            return new ObservableCollection<KeyedList<string, Photo>>(groupedPhotos);
        }

        public ObservableCollection<Album> GetAllAlbums()
        {
            List<Album> ret = new List<Album>();

            using (var library = new MediaLibrary())
            {
                var rootAlbum = library.RootPictureAlbum;
                if (rootAlbum != null && rootAlbum.Albums != null && rootAlbum.Albums.Count > 0)
                {
                    foreach (PictureAlbum album in rootAlbum.Albums)
                    {
                        var r = GetAlbums(album);
                        if (r != null)
                        {
                            ret.AddRange(r);
                        }
                    }
                }
            }

            return new ObservableCollection<Album>(ret);
        }

        private static List<Album> GetAlbums(PictureAlbum album)
        {
            List<Album> ret = new List<Album>();

            if (album != null && album.Albums != null && album.Albums.Count > 0)
            {
                foreach (PictureAlbum p in album.Albums)
                {
                    List<Album> r = GetAlbums(p);
                    if (r != null && r.Count > 0)
                    {
                        ret.AddRange(r);
                    }
                }
            }

            if (album.Pictures != null)
            {
                ret.Add(new Album(album));
            }

            return ret;
        }
    }

    public partial class AlbumPage : PhoneApplicationPage
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AllPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailPreviewPivot.SelectedIndex = 0;
        }

        private void AllAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailPreviewPivot.SelectedIndex = 1;
        }

        private void AlbumPage_Loaded(object sender, RoutedEventArgs e)
        {
            PhotoHubLLS.ItemsSource = LibraryDataService.Instance.GetGroupedPhotos();

            PhotoHubLLS.ScrollTo(PhotoHubLLS.ItemsSource[PhotoHubLLS.ItemsSource.Count - 1]);

            AlbumHubLLS.ItemsSource = LibraryDataService.Instance.GetAllAlbums();
        }

        private void ThumbnailPreviewPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = ThumbnailPreviewPivot.SelectedIndex;
            if (selectedIndex == 0)
            {
                AllImageIcon.Source = new BitmapImage(new Uri("/Assets/AllImageIconSelected.png", UriKind.Relative));
                AllImageTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 224, 73));

                AllAlbumIcon.Source = new BitmapImage(new Uri("/Assets/AllAlbumIcon.png", UriKind.Relative));
                AllAlbumTextBlock.Foreground = new SolidColorBrush(Colors.White);

                TitleTextBlock.Text = "All";
            }
            else if (selectedIndex == 1)
            {
                AllImageIcon.Source = new BitmapImage(new Uri("/Assets/AllImageIcon.png", UriKind.Relative));
                AllImageTextBlock.Foreground = new SolidColorBrush(Colors.White);

                AllAlbumIcon.Source = new BitmapImage(new Uri("/Assets/AllAlbumIconSelected.png", UriKind.Relative));
                AllAlbumTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 224, 73));

                TitleTextBlock.Text = "Albums";
            }
        }

        private void AlbumHubLLS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = AlbumHubLLS.SelectedItem as Album;
            if (selectedItem != null)
            {
                PhoneApplicationService.Current.State["SelectedAlbum"] = selectedItem;
                NavigationService.Navigate(new Uri("/AlbumPreviewPage.xaml", UriKind.Relative));
                AlbumHubLLS.SelectedItem = null;
            }
        }

        private void PhotoHubLLS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var photo = PhotoHubLLS.SelectedItem as Photo;
            if (photo != null)
            {
                PhoneApplicationService.Current.State["SelectedPhoto"] = photo;
                NavigationService.Navigate(new Uri("/PreviewPage.xaml?album_enabled=false", UriKind.Relative));

                PhotoHubLLS.SelectedItem = null;
            }
        }
    }
}