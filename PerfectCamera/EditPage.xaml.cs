using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices.WindowsRuntime;

using PerfectCamera.Filters;
using PerfectCamera.Filters.Artistic;
using PerfectCamera.Filters.FilterControls;
using System.IO.IsolatedStorage;
using Windows.Storage.Streams;
using System.Threading.Tasks;

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Custom;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using PerfectCamera.Filters.Lomo;
using PerfectCamera.Filters.Funny;
using PerfectCamera.Filters.Retro;
using PerfectCamera.Filters.MagicSkin;

namespace PerfectCamera
{
    public class EffectGroup<T>: List<T>
    {
        public string Name { get; set; }
        public Uri ThumbnailUri { get; set; }
    }

    public partial class EditPage : PhoneApplicationPage
    {
        private EffectIndex _selectedEffect = new EffectIndex(0, 0);
        private Picture _editingPicture = null;
        private Semaphore _editSemaphore = new Semaphore(1,1);

        // Constants
        private const String DebugTag = "PreviewPage: ";
        private const double DefaultOutputResolutionWidth = 480;
        private const double DefaultOutputResolutionHeight = 640;
        private const String FileNamePrefix = "FilterEffects_";
        private const String TombstoneImageDir = "TempData";
        private const String TombstoneImageFile = "TempData\\TempImage.jpg";
        private const String EffectGroupIndexKey = "GroupIndex";
        private const String FilterIndexKey = "FilterIndex";
        private const int HideControlsDelay = 2; // Seconds
        private const String PivotItemNamePrefix = "PivotItem_";
        private const String FilterPropertyControlNamePrefix = "FilterPropertyControl_";

        // Members
        private List<EffectGroup<AbstractFilter>> _effects = null;
        private ProgressIndicator _progressIndicator = new ProgressIndicator();
        private DispatcherTimer _timer = null;
        private FilterPropertiesControl _controlToHide = null;
        private bool _isNewPageInstance = false;

        private MemoryStream _editingStream = new MemoryStream();

        private AbstractFilter _lastSelectedFilter = null;

        public EditPage()
        {
            InitializeComponent();

            if (PhoneApplicationService.Current.State.ContainsKey("EditPicture"))
            {
                _editingPicture = PhoneApplicationService.Current.State["EditPicture"] as Picture;
                if (_editingPicture != null)
                {
                    var stream = _editingPicture.GetImage();
                    stream.CopyTo(_editingStream);
                    _editingStream.Position = 0;

                    BitmapImage img = new BitmapImage();
                    img.SetSource(_editingStream);
                    CurrentEditImage.Source = img;

                    OriginalImageHolder.Source = img;
                }
            }

            _isNewPageInstance = true;
            _progressIndicator.IsIndeterminate = true;

            CreateComponents();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If _isNewPageInstance is true, the state may need to be restored.
            if (_isNewPageInstance)
            {
                RestoreState();
            }

            // If the user navigates back to this page and it has remained in 
            // memory, this value will continue to be false.
            _isNewPageInstance = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // On back navigation there is no need to save state.
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                StoreState();
            }
            else
            {
                // Navigating back
                // Dispose the filters
                foreach (var group in _effects)
                {
                    foreach (AbstractFilter filter in group)
                    {
                        filter.Dispose();
                    }
                }

                _editingStream.Dispose();
            }

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Store the page state in case application gets tombstoned.
        /// </summary>
        private void StoreState()
        {
            // Save the currently filtered image into isolated app storage.
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();
            myStore.CreateDirectory(TombstoneImageDir);

            try
            {
                using (var isoFileStream = new IsolatedStorageFileStream(
                    TombstoneImageFile,
                    FileMode.OpenOrCreate,
                    myStore))
                {
                    _editingStream.Position = 0;
                    _editingStream.CopyTo(isoFileStream);
                    isoFileStream.Flush();
                }
            }
            catch
            {
                MessageBox.Show("Error while trying to store temporary image.");
            }

            // Save also the current preview index 
            State[EffectGroupIndexKey] = _selectedEffect.EffectIdx;
            State[FilterIndexKey] = _selectedEffect.FilterIdx;
        }

        /// <summary>
        /// Restores the page state if application was tombstoned.
        /// </summary>
        private async void RestoreState()
        {
            // Load also the preview index which was last used
            if (State.ContainsKey(EffectGroupIndexKey) && State.ContainsKey(FilterIndexKey))
            {
                _selectedEffect.EffectIdx = (int)State[EffectGroupIndexKey];
                _selectedEffect.FilterIdx = (int)State[FilterIndexKey];
            }

            // Load the image which was filtered from isolated app storage.
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                if (myStore.FileExists(TombstoneImageFile))
                {
                    using (var isoFileStream = new IsolatedStorageFileStream(
                        TombstoneImageFile,
                        FileMode.Open,
                        myStore))
                    {
                        // Load image asynchronously at application launch
                        await isoFileStream.CopyToAsync(_editingStream);

                        Dispatcher.BeginInvoke(() => {
                            EffectGroup<AbstractFilter> group = _effects[_selectedEffect.EffectIdx];
                            AbstractFilter filter = group[_selectedEffect.FilterIdx];

                            filter.Buffer = _editingStream.GetWindowsRuntimeBuffer();
                            filter.Apply();

                            CurrentEditImage.Source = filter.PreviewImageSource;
                        });

                    }
                }
            }
            catch
            {
                MessageBox.Show("Error while trying to restore temporary image.");
            }

            // Remove temporary file from isolated storage
            try
            {
                if (myStore.FileExists(TombstoneImageFile))
                {
                    myStore.DeleteFile(TombstoneImageFile);
                }
            }
            catch (IsolatedStorageException /*ex*/)
            {
                MessageBox.Show("Error while trying to delete temporary image.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EffectButton_Click(object sender, RoutedEventArgs e)
        {
            /*HideEffectLayoutButton.Visibility = System.Windows.Visibility.Visible;
            CancelEffectButton.Visibility = System.Windows.Visibility.Collapsed;
            EffectTitleTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            ApplyEffectButton.Visibility = System.Windows.Visibility.Collapsed;*/

            PreviewImageGrid.Tap += PreviewImageGrid_Tap;

            EffectButton.IsHitTestVisible = false;
            EffectLayoutSlideIn.Begin();
        }

        private void HideEffectLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            HideEffectLayoutButton.IsHitTestVisible = false;
            EffectLayoutSlideOut.Begin();
        }

        private void EffectLayoutSlideOut_Completed(object sender, EventArgs e)
        {
            PreviewImageGrid.Tap -= PreviewImageGrid_Tap;
            HintText.Visibility = System.Windows.Visibility.Collapsed;

            EffectButton.IsHitTestVisible = true;
            CancelEffectButton.IsHitTestVisible = true;
            ApplyEffectButton.IsHitTestVisible = true;
        }

        private void EffectLayoutSlideIn_Completed(object sender, EventArgs e)
        {
            HideEffectLayoutButton.IsHitTestVisible = true;
        }

        private void InitEffectPanel()
        {
            if (_effects != null)
            {
                EffectStackPanel.Children.Clear();
                for (int i = 0; i < _effects.Count; i++)
                {
                    EffectGroup<AbstractFilter> group = _effects[i];
                    Button btn = new Button()
                    {
                        Style = (Style)Application.Current.Resources["ButtonStyleNoBorder"],
                        Margin = new Thickness(10, 0, 0, 0),
                        Height = 130,
                        Width = 97,
                        Background = new ImageBrush()
                        {
                            ImageSource = new BitmapImage(group.ThumbnailUri),
                            Stretch = Stretch.Uniform
                        },
                        Tag = i,
                        ContentTemplate = (DataTemplate)Application.Current.Resources["ButtonContentWrap"],
                        Content = group.Name,
                        FontSize = 20,
                        FontWeight = FontWeights.Light
                    };
                    EffectStackPanel.Children.Add(btn);

                    btn.Click += EffectGroup_Click;
                }

                EffectStackPanel.Width = _effects.Count * 97 + (_effects.Count + 1) * 10;
            }
        }

        void EffectGroup_Click(object sender, RoutedEventArgs e)
        {
            HideEffectLayoutButton.Visibility = System.Windows.Visibility.Collapsed;
            CancelEffectButton.Visibility = System.Windows.Visibility.Visible;
            EffectTitleTextBlock.Visibility = System.Windows.Visibility.Visible;
            ApplyEffectButton.Visibility = System.Windows.Visibility.Visible;

            FilterStackPanel.Children.Clear();
            if (_effects != null)
            {
                Button clickedbtn = sender as Button;
                int idx = (int)clickedbtn.Tag;

                Button backBtn = new Button()
                {
                    Style = (Style)Application.Current.Resources["ButtonStyleNoBorder"],
                    Margin = new Thickness(10, 0, 0, 0),
                    Height = 130,
                    Width = 60,
                    Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri("/Assets/BackToEffectButton.png", UriKind.Relative)),
                        Stretch = Stretch.Uniform
                    }
                };
                FilterStackPanel.Children.Add(backBtn);
                backBtn.Click += BackToEffect_Click;

                if (idx >= 0 && idx < _effects.Count)
                {
                    EffectGroup<AbstractFilter> group = _effects[idx];

                    for (int i = 0; i < group.Count; i++)
                    {
                        AbstractFilter filter = group[i];
                        Button btn = new Button()
                        {
                            Style = (Style)Application.Current.Resources["ButtonStyleNoBorder"],
                            Margin = new Thickness(10, 0, 0, 0),
                            Height = 130,
                            Width = 97,
                            Background = new ImageBrush()
                            {
                                ImageSource = new BitmapImage(filter.ThumbnailUri),
                                Stretch = Stretch.Uniform
                            },
                            Tag = i,
                            ContentTemplate = (DataTemplate)Application.Current.Resources["ButtonContentWrap"],
                            Content = filter.Name,
                            FontSize = 20,
                            FontWeight = FontWeights.Light
                        };
                        FilterStackPanel.Children.Add(btn);

                        btn.Click += FilterButton_Click;
                    }

                    FilterStackPanel.Width = group.Count * 97 + (group.Count + 1) * 10 + 70;
                }

                _selectedEffect.EffectIdx = idx;

                FilterPanelSlideIn.Begin();
            }
        }

        void BackToEffect_Click(object sender, RoutedEventArgs e)
        {
            ApplyEditIndicator.Visibility = System.Windows.Visibility.Visible;

            PreviewImageGrid.Tap -= PreviewImageGrid_Tap;
            HintText.Visibility = System.Windows.Visibility.Collapsed;

            Dispatcher.BeginInvoke(() =>
            {
                CancelEffectButton.IsHitTestVisible = false;
                
                BitmapImage img = new BitmapImage();
                img.SetSource(_editingStream);
                CurrentEditImage.Source = img;

                ApplyEditIndicator.Visibility = System.Windows.Visibility.Collapsed;
            });

            FilterPanelSlideOut.Begin();

            HideEffectLayoutButton.Visibility = System.Windows.Visibility.Visible;
            CancelEffectButton.Visibility = System.Windows.Visibility.Collapsed;
            EffectTitleTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            ApplyEffectButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ShowHint()
        {
            Debug.WriteLine(DebugTag + "FilterPreviewPivot_SelectionChanged()");

            EffectGroup<AbstractFilter> group = _effects[_selectedEffect.EffectIdx];
            AbstractFilter filter = group[_selectedEffect.FilterIdx];

            if (filter.Control != null)
            {
                HintText.Visibility = Visibility.Visible;
            }
            else if (HintText.Visibility == Visibility.Visible)
            {
                HintText.Visibility = Visibility.Collapsed;
            }

            ShowControlsAnimationStoryBoard.Completed -= ShowControlsAnimationStoryBoard_Completed;
            HideControlsAnimation.Completed -= HideControlsAnimation_Completed;
            ShowControlsAnimationStoryBoard.Stop();
            HideControlsAnimationStoryBoard.Stop();

            if (_controlToHide != null)
            {
                _controlToHide.Visibility = Visibility.Collapsed;
                _controlToHide.Opacity = 0;
                _controlToHide = null;
            }
        }

        void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_editSemaphore.WaitOne(500))
            {
                var clickedBtn = sender as Button;
                _selectedEffect.FilterIdx = (int)clickedBtn.Tag;

                ShowHint();

                if (_editingPicture != null)
                {
                    if (_lastSelectedFilter != null)
                    {
                        _lastSelectedFilter.ReleaseBitmapMemory();
                    }

                    EffectGroup<AbstractFilter> group = _effects[_selectedEffect.EffectIdx];
                    AbstractFilter filter = group[_selectedEffect.FilterIdx];

                    _lastSelectedFilter = filter;

                    filter.CreateBimapMemory();

                    _editingStream.Position = 0;

                    filter.Buffer = _editingStream.GetWindowsRuntimeBuffer();
                    filter.Apply();

                    CurrentEditImage.Source = filter.PreviewImageSource;
                }

                _editSemaphore.Release();
            }
        }

        /// <summary>
        /// Constructs the filters and the pivot items.
        /// </summary>
        private void CreateComponents()
        {
            CreateFilters();

            // Create a pivot item with an image for each filter. The image
            // content is added later. In addition, create the preview bitmaps
            // and associate them with the images.
            foreach (var group in _effects)
            {
                foreach (var filter in group)
                {
                    FilterPropertiesControl control = new FilterPropertiesControl();

                    String name = FilterPropertyControlNamePrefix + filter.Name;
                    control.Name = name;

                    if (filter.AttachControl(control))
                    {
                        control.VerticalAlignment = VerticalAlignment.Bottom;
                        control.Opacity = 0;
                        control.Visibility = Visibility.Collapsed;
                        control.ControlBackground.Fill = AppUtils.ThemeBackgroundBrush;
                        PreviewImageGrid.Children.Add(control);

                        control.Manipulated += OnControlManipulated;
                    }

                    Windows.Foundation.Size previewSize = new Windows.Foundation.Size();
                    previewSize.Width = DefaultOutputResolutionWidth;
                    previewSize.Height = _editingPicture.Height * previewSize.Width / _editingPicture.Width;

                    //filter.PreviewResolution = new Windows.Foundation.Size(DefaultOutputResolutionWidth, DefaultOutputResolutionHeight);
                    filter.PreviewResolution = previewSize;
                }
            }

            HintTextBackground.Fill = AppUtils.ThemeBackgroundBrush;

            InitEffectPanel();
        }

        /// <summary>
        /// Constructs the filters.
        /// </summary>
        private void CreateFilters()
        {
            _effects = new List<EffectGroup<AbstractFilter>>();
            var group = new EffectGroup<AbstractFilter>
            {
                new OriginalImageFilter(),
                new SixthGearFilter(),
                new SadHipsterFilter(),
                new EightiesPopSongFilter(),
                new MarvelFilter(),
                new SurroundedFilter()
            };
            group.Name = "Filters";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);

            CreateMagicSkinGroup();
            CreateArtisticGroup();
            CreateEnhanceGroup();
            CreateHDRGroup();
            CreateLightColorGroup();
            CreateLOFTGroup();
            CreateSketchGroup();
            CreateLOMOGroup();
            CreateRetroGroup();
            CreateFunnyGroup();
        }

        /// <summary>
        /// Shows the filter property controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewImageGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (sender is Grid)
            {
                Grid grid = (Grid)sender;
                EffectGroup<AbstractFilter> group = _effects[_selectedEffect.EffectIdx];
                AbstractFilter filter = group[_selectedEffect.FilterIdx];
                foreach (UIElement element in grid.Children)
                {
                    if (element is FilterPropertiesControl)
                    {
                        if ((element.Visibility == Visibility.Collapsed || element.Opacity < 1) && element == filter.Control)
                        {
                            Debug.WriteLine(DebugTag + "ShowPropertiesControls()");

                            if (HintText.Visibility == Visibility.Visible)
                            {
                                HintText.Visibility = Visibility.Collapsed;
                            }

                            HideControlsAnimation.Completed -= HideControlsAnimation_Completed;
                            HideControlsAnimationStoryBoard.Stop();

                            if (_timer != null)
                            {
                                _timer.Tick -= HidePropertiesControls;
                                _timer.Stop();
                                _timer = null;
                            }

                            _controlToHide = (FilterPropertiesControl)element;
                            _controlToHide.Visibility = Visibility.Visible;

                            try
                            {
                                Storyboard.SetTargetName(ShowControlsAnimation, _controlToHide.Name);
                                ShowControlsAnimation.From = _controlToHide.Opacity;
                                ShowControlsAnimationStoryBoard.Completed += ShowControlsAnimationStoryBoard_Completed;
                                ShowControlsAnimationStoryBoard.Begin();
                            }
                            catch (InvalidOperationException ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }

                            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, HideControlsDelay) };
                            _timer.Tick += HidePropertiesControls;
                            _timer.Start();

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Makes sure that the controls stay visible after the animation is
        /// completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShowControlsAnimationStoryBoard_Completed(object sender, EventArgs e)
        {
            _controlToHide.Opacity = 1;
            ShowControlsAnimationStoryBoard.Completed -= ShowControlsAnimationStoryBoard_Completed;
        }

        /// <summary>
        /// Hides the filter property controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HidePropertiesControls(object sender, EventArgs e)
        {
            ShowControlsAnimationStoryBoard.Stop();

            if (_controlToHide != null)
            {
                Debug.WriteLine(DebugTag + "HidePropertiesControls()");
                Storyboard.SetTargetName(HideControlsAnimation, _controlToHide.Name);
                HideControlsAnimation.From = _controlToHide.Opacity;
                HideControlsAnimationStoryBoard.Begin();
                HideControlsAnimation.Completed += HideControlsAnimation_Completed;
            }

            if (_timer != null)
            {
                _timer.Tick -= HidePropertiesControls;
                _timer.Stop();
                _timer = null;
            }
        }

        /// <summary>
        /// Completes the actions when HideControlsAnimation has finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HideControlsAnimation_Completed(object sender, EventArgs e)
        {
            HideControlsAnimation.Completed -= HideControlsAnimation_Completed;
            _controlToHide.Visibility = Visibility.Collapsed;
            _controlToHide.Opacity = 0;
            _controlToHide = null;
        }

        /// <summary>
        /// Restarts the timer responsible for hiding the filter property
        /// controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnControlManipulated(object sender, EventArgs e)
        {
            Debug.WriteLine(DebugTag + "OnControlManipulated(): " + sender);

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void CancelEffectButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyEditIndicator.Visibility = System.Windows.Visibility.Visible;

            Dispatcher.BeginInvoke(() => {
                CancelEffectButton.IsHitTestVisible = false;
                EffectLayoutSlideOut.Begin();
                BitmapImage img = new BitmapImage();
                img.SetSource(_editingStream);
                CurrentEditImage.Source = img;

                ApplyEditIndicator.Visibility = System.Windows.Visibility.Collapsed;
            });
            
        }

        private async void ApplyEffectButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyEditIndicator.Visibility = System.Windows.Visibility.Visible;

            EffectGroup<AbstractFilter> group = _effects[_selectedEffect.EffectIdx];
            AbstractFilter filter = group[_selectedEffect.FilterIdx];

            _editingStream.Position = 0;
            IBuffer buffer = await filter.RenderJpegAsync(_editingStream.GetWindowsRuntimeBuffer());

            _editingStream.Position = 0;
            Stream stream = buffer.AsStream();
            stream.CopyTo(_editingStream);

            ApplyEditIndicator.Visibility = System.Windows.Visibility.Collapsed;

            ApplyEffectButton.IsHitTestVisible = false;
            EffectLayoutSlideOut.Begin();
        }

        private void CreateMagicSkinGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>()
            {
                new MagicSkinNaturalFilter()
            };
            group.Name = "Magic Skin";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateArtisticGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>
            {
                new AntiqueWrapperFilter(),
                new CartoonWrapperFilter(),
                new ColorSwapWrapperFilter(),
                new EmbossWrapperFilter(),
                new FogWrapperFilter(),
                new FoundationWrapperFilter(),
                new GrayscaleNegativeWrapperFilter(),
                new MagicPenWrapperFilter(),
                new MilkyWrapperFilter()
            };
            group.Name = "Artistic";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateLightColorGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>();
            group.Name = "Light Color";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateEnhanceGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>();
            group.Name = "Enhance";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateHDRGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>();
            group.Name = "HDR";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateLOFTGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>();
            group.Name = "LOFT";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateSketchGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>();
            group.Name = "Sketch";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateLOMOGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>()
            {
                new LomoNeutralFilter(),
                new LomoRedFilter(),
                new LomoGreenFilter(),
                new LomoBlueFilter(),
                new LomoYellowFilter()
            };
            group.Name = "LOMO";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateRetroGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>()
            {
                new RetroPurpleFilter(),
                new VintageFilter(),
                new RetroBloomFilter(),
                new RetroGoldenFilter(),
                new RetroLightFilter(),
                //new RetroCleanFilter(),
                new RetroFadedFilter(),
                new RetroEmeraldFilter(),
                new RetroDiffuseFilter()
            };
            group.Name = "Retro";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void CreateFunnyGroup()
        {
            EffectGroup<AbstractFilter> group = new EffectGroup<AbstractFilter>() 
            {
                new MirrorAboveFilter(),
                new MirrorBelowFilter(),
                new MirrorLeftFilter(),
                new MirrorRightFilter(),
                new FishEyeFilter(),
                new OrangeWrapperFilter(),
                new RoseWrapperFilter(),
                new GreenWrapperFilter(),
                new BlueWrapperFilter()
            };
            group.Name = "Funny";
            group.ThumbnailUri = new Uri("/Assets/EffectThumbnails/NoEffect.png", UriKind.Relative);
            _effects.Add(group);
        }

        private void OriginalImageHolder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CurrentEditImage.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OriginalImageHolder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CurrentEditImage.Visibility = System.Windows.Visibility.Visible;
        }

        private void PreviewImageGrid_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CurrentEditImage.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PreviewImageGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CurrentEditImage.Visibility = System.Windows.Visibility.Visible;
        }

        private void PreviewImageGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CurrentEditImage.Visibility = System.Windows.Visibility.Visible;
        }
    }
}