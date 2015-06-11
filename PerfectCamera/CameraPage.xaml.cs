using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;

using PerfectCamera.Resources;
using System.Windows.Controls.Primitives;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using Microsoft.Phone.Reactive;
using System.Windows.Threading;
using PerfectCamera.Filters;

namespace PerfectCamera
{
    public enum CameraRatio
    {
        Ratio_4x3,
        Ratio_16x9
    }

    public enum PerfectCameraType
    {
        Selfie,
        EasyCam
    }

    public class ApplyEffectButton: Button
    {
        public string EffectName
        {
            get;
            set;
        }

        private bool _isGroupEffect = false;
        public bool IsGroupEffect
        {
            get
            {
                return _isGroupEffect;
            }
            set
            {
                _isGroupEffect = value;
            }
        }
    }


    public partial class CameraPage : PhoneApplicationPage
    {
        // Members
        private PhotoCaptureDevice Camera = null;
        private bool _capturing = false;

        private MediaElement _mediaElement = null;
        private Effects _cameraEffect = null;
        private CameraStreamSource _cameraStreamSource = null;
        private Semaphore _cameraSemaphore = new Semaphore(1, 1);

        private int _selectedEffect = 0;

        private List<Popup> _openPopupList = new List<Popup>();

        private const String FileNamePrefix = "PerfectCamera_";

        private SettingMenu _settingMenu = new SettingMenu();
        private FlashMenu _flashSettingMenu = new FlashMenu();

        private readonly DispatcherTimer _focusDisplayTimer = new DispatcherTimer();
        private Semaphore _focusSemaphore = new Semaphore(1,1);
        private Windows.Foundation.Size _focusRegionSize = new Windows.Foundation.Size(80, 80);
        private bool _manuallyFocused = false;

        public CameraPage()
        {
            InitializeComponent();

            if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
            {
                PerfectCamera.DataContext.Instance.SensorLocation = CameraSensorLocation.Front;
            }
            else if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.EasyCam)
            {
                PerfectCamera.DataContext.Instance.SensorLocation = CameraSensorLocation.Back;

                EffectButton.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!Microsoft.Devices.PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
            {
                SwitchCameraButton.IsHitTestVisible = false;
                /*ImageBrush brush = new ImageBrush()
                {
                    Stretch = Stretch.Uniform,
                    ImageSource = new BitmapImage(new Uri("/Assets/SwitchCameraButtonDisabled.png", UriKind.Relative))
                };
                SwitchCameraButton.Background = brush;*/
                SwitchCameraButton.Opacity = 0.5;

                //force using back camera
                PerfectCamera.DataContext.Instance.SensorLocation = CameraSensorLocation.Back;
            }

            UpdateFlashIcon();

            _settingMenu.RatioChanged = (ratio) =>
            {
                if (ratio != PerfectCamera.DataContext.Instance.CameraRatio)
                {
                    PerfectCamera.DataContext.Instance.CameraRatio = ratio;
                    var task = ResetCamera();
                }
            };

            _flashSettingMenu.FlashStateChanged = (state) =>
            {
                if (state != PerfectCamera.DataContext.Instance.FlashState)
                {
                    PerfectCamera.DataContext.Instance.FlashState = state;

                    if (Camera != null)
                    {
                        Camera.SetProperty(KnownCameraPhotoProperties.FlashMode, PerfectCamera.DataContext.Instance.FlashState);
                    }

                    UpdateFlashIcon();
                }
            };

            _focusDisplayTimer.Interval = TimeSpan.FromSeconds(1);
            _focusDisplayTimer.Tick += _focusDisplayTimer_Tick;
        }

        private void UpdateFlashIcon()
        {
            ImageBrush brush = new ImageBrush()
            {
                Stretch = Stretch.Uniform
            };
            switch(PerfectCamera.DataContext.Instance.FlashState)
            {
                case FlashState.Auto:
                    {
                        brush.ImageSource = new BitmapImage(new Uri("/Assets/FlashAutoOff.png", UriKind.Relative));
                        break;
                    }
                case FlashState.On:
                    {
                        brush.ImageSource = new BitmapImage(new Uri("/Assets/FlashOnOff.png", UriKind.Relative));
                        break;
                    }
                case FlashState.Off:
                    {
                        brush.ImageSource = new BitmapImage(new Uri("/Assets/FlashOffOff.png", UriKind.Relative));
                        break;
                    }
            }
            FlashButton.Background = brush;
        }

        private async Task ResetCamera()
        {
            Uninitialize();

            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            ShowProgress(AppResources.InitializingCameraText);
            await InitializeCamera(PerfectCamera.DataContext.Instance.SensorLocation);
            HideProgress();

            InitEffectPanel();

            if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
            {
                _mediaElement = new MediaElement { Stretch = Stretch.UniformToFill, BufferingTime = new TimeSpan(0) };
                _mediaElement.SetSource(_cameraStreamSource);

                BackgroundVideoBrush.SetSource(_mediaElement);

                EffectNameTextBlock.Text = _cameraEffect.EffectName;
                EffectNameFadeIn.Begin();
            }
            else
            {
                BackgroundVideoBrush.SetSource(Camera);
            }

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);

            SetOrientation(this.Orientation);
        }

        private void Uninitialize()
        {
            if (_mediaElement != null)
            {
                _mediaElement.Source = null;
                _mediaElement = null;
            }

            if (_cameraStreamSource != null)
            {
                _cameraStreamSource = null;
            }

            _cameraEffect = null;
        }

        /// <summary>
        /// If camera has not been initialized when navigating to this page, initialization
        /// will be started asynchronously in this method. Once initialization has been
        /// completed the camera will be set as a source to the VideoBrush element
        /// declared in XAML. On-screen controls are enabled when camera has been initialized.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }

            ShowProgress(AppResources.InitializingCameraText);
            await InitializeCamera(PerfectCamera.DataContext.Instance.SensorLocation);
            HideProgress();

            InitEffectPanel();

            if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
            {
                _mediaElement = new MediaElement { Stretch = Stretch.UniformToFill, BufferingTime = new TimeSpan(0) };
                _mediaElement.SetSource(_cameraStreamSource);

                BackgroundVideoBrush.SetSource(_mediaElement);

                EffectNameTextBlock.Text = _cameraEffect.EffectName;
                EffectNameFadeIn.Begin();
            }
            else
            {
                BackgroundVideoBrush.SetSource(Camera);
            }

            SetScreenButtonsEnabled(true);
            SetCameraButtonsEnabled(true);
            Storyboard sb = (Storyboard)Resources["CaptureAnimation"];
            sb.Stop();

            SetOrientation(this.Orientation);

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// On-screen controls are disabled when navigating away from the
        /// viewfinder. This is because we want the controls to default to
        /// disabled when arriving to the page again.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Uninitialize();

            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Adjusts UI according to device orientation.
        /// </summary>
        /// <param name="e">Orientation event arguments.</param>
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            SetOrientation(e.Orientation);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            bool cancel = false;
            for (int i = _openPopupList.Count - 1; i >= 0; i-- )
            {
                Popup p = _openPopupList[i];
                if (p.IsOpen)
                {
                    cancel = true;
                    p.IsOpen = false;
                    _openPopupList.Remove(p);
                    break;
                }
                else
                {
                    _openPopupList.Remove(p);
                }
            }

            if (cancel)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        /// <summary>
        /// Makes adjustments to UI depending on device orientation. Ensures 
        /// that the viewfinder stays fully visible in the middle of the 
        /// screen. This requires dynamic changes to title and video canvas.
        /// </summary>
        /// <param name="orientation">Device orientation.</param>
        private void SetOrientation(PageOrientation orientation)
        {
            double ratio = PerfectCamera.DataContext.Instance.CameraRatio == CameraRatio.Ratio_4x3 ? (4.0 / 3.0) : (16.0 / 9.0);
            // Default values in landscape left orientation.
            int videoBrushTransformRotation = 0;
            int videoCanvasWidth = (int)(Application.Current.Host.Content.ActualWidth * ratio);
            int videoCanvasHeight = (int)Application.Current.Host.Content.ActualWidth;
            Thickness videoCanvasMargin = new Thickness(-60, 0, 0, 0);

            // Orientation.specific changes to default values
            if (orientation == PageOrientation.PortraitUp)
            {
                if (PerfectCamera.DataContext.Instance.SensorLocation == CameraSensorLocation.Back)
                {
                    videoBrushTransformRotation = 90;
                }
                else
                {
                    videoBrushTransformRotation = -90;
                }
                videoCanvasWidth = (int)Application.Current.Host.Content.ActualWidth;
                videoCanvasHeight = (int)(Application.Current.Host.Content.ActualWidth * ratio);
                videoCanvasMargin = new Thickness(0, -20, 0, 0);
            }
            else if (orientation == PageOrientation.LandscapeRight)
            {
                videoBrushTransformRotation = 180;
                videoCanvasMargin = new Thickness(60, 0, 0, 0);
            }

            // Set correct values
            VideoBrushTransform.Rotation = videoBrushTransformRotation;
            VideoCanvas.Width = videoCanvasWidth;
            VideoCanvas.Height = videoCanvasHeight;
            VideoCanvas.Margin = videoCanvasMargin;

            if (Camera != null)
            {
                try
                {
                    Camera.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, VideoBrushTransform.Rotation);
                    if (Camera.SensorLocation == CameraSensorLocation.Back)
                    {
                        Camera.SetProperty(KnownCameraPhotoProperties.FlashMode, PerfectCamera.DataContext.Instance.FlashState);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Enables or disabled on-screen controls.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable controls.</param>
        private void SetScreenButtonsEnabled(bool enabled)
        {
            ShutterButton.IsHitTestVisible = enabled;
            EffectButton.IsHitTestVisible = enabled;
            FlashButton.IsHitTestVisible = enabled;
            MenuButton.IsHitTestVisible = enabled;
        }

        /// <summary>
        /// Enables or disables listening to hardware shutter release key events.
        /// </summary>
        /// <param name="enabled">True to enable listening, false to disable listening.</param>
        private void SetCameraButtonsEnabled(bool enabled)
        {
            if (enabled)
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed += ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed += ShutterKeyPressed;
            }
            else
            {
                Microsoft.Devices.CameraButtons.ShutterKeyHalfPressed -= ShutterKeyHalfPressed;
                Microsoft.Devices.CameraButtons.ShutterKeyPressed -= ShutterKeyPressed;
            }
        }

        /// <summary>
        /// Displays the progress indicator with the given message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        private void ShowProgress(String message)
        {
            ProgressIndicator.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Hides the progress indicator.
        /// </summary>
        private void HideProgress()
        {
            ProgressIndicator.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Initializes camera.
        /// </summary>
        /// <param name="sensorLocation">Camera sensor to initialize</param>
        private async Task InitializeCamera(CameraSensorLocation sensorLocation)
        {
            IReadOnlyList<Windows.Foundation.Size> availablePreviewResolutions = PhotoCaptureDevice.GetAvailablePreviewResolutions(sensorLocation);

            Windows.Foundation.Size previewResolution = new Windows.Foundation.Size(int.MaxValue, int.MaxValue);
            for (int i = 0; i < availablePreviewResolutions.Count; i++)
            {
                double ratio = availablePreviewResolutions[i].Width / availablePreviewResolutions[i].Height;
                if (ratio > 1.32 && ratio < 1.34 && PerfectCamera.DataContext.Instance.CameraRatio == CameraRatio.Ratio_4x3)
                {
                    if (previewResolution.Width > availablePreviewResolutions[i].Width)
                    {
                        previewResolution = availablePreviewResolutions[i];
                    }
                }
                else if (ratio > 1.7 && ratio < 1.8 && PerfectCamera.DataContext.Instance.CameraRatio == CameraRatio.Ratio_16x9)
                {
                    if (previewResolution.Width > availablePreviewResolutions[i].Width)
                    {
                        previewResolution = availablePreviewResolutions[i];
                    }
                }
            }

            PerfectCamera.DataContext.Instance.PreviewResolution = previewResolution;

            IReadOnlyList<Windows.Foundation.Size> availableResolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(sensorLocation);

            //find 4:3 (2048 x 1536) or 16:9 (1280x720)
            Windows.Foundation.Size captureResolution = new Windows.Foundation.Size(0, 0);
            for (int i = 0; i < availableResolutions.Count; i++)
            {
                double ratio = availableResolutions[i].Width / availableResolutions[i].Height;
                if (ratio > 1.32 && ratio < 1.34 && PerfectCamera.DataContext.Instance.CameraRatio == CameraRatio.Ratio_4x3)
                {
                    if (captureResolution.Width < availableResolutions[i].Width)
                    {
                        captureResolution = availableResolutions[i];
                    }
                }
                else if (ratio > 1.7 && ratio < 1.8 && PerfectCamera.DataContext.Instance.CameraRatio == CameraRatio.Ratio_16x9)
                {
                    if (captureResolution.Width < availableResolutions[i].Width)
                    {
                        captureResolution = availableResolutions[i];
                    }
                }
            }
            //
            

            PhotoCaptureDevice device = await PhotoCaptureDevice.OpenAsync(sensorLocation, captureResolution);

            await device.SetPreviewResolutionAsync(previewResolution);
            await device.SetCaptureResolutionAsync(captureResolution);

            Camera = device;

            if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
            {
                _cameraEffect = new Effects()
                {
                    PhotoCaptureDevice = Camera
                };

                _cameraStreamSource = new CameraStreamSource(_cameraEffect, previewResolution);
            }

            if (Camera != null)
            {
                if (Camera.SensorLocation == CameraSensorLocation.Front)
                {
                    FlashButton.IsHitTestVisible = false;
                    FlashButton.Opacity = 0.5;
                }
                else
                {
                    FlashButton.IsHitTestVisible = true;
                    FlashButton.Opacity = 1.0;
                }
            }

            SetOrientation(this.Orientation);
        }

        /// <summary>
        /// Starts autofocusing, if supported. Capturing buttons are disabled
        /// while focusing.
        /// </summary>
        private async Task AutoFocus()
        {
            if (!_capturing && PhotoCaptureDevice.IsFocusSupported(Camera.SensorLocation))
            {
                SetScreenButtonsEnabled(false);
                SetCameraButtonsEnabled(false);

                await Camera.FocusAsync();

                SetScreenButtonsEnabled(true);
                SetCameraButtonsEnabled(true);

                _capturing = false;
            }
        }

        /// <summary>
        /// Captures a photo. Photo data is stored to ImageStream, and
        /// application is navigated to the preview page after capturing.
        /// </summary>
        private async Task Capture()
        {
            //pause camera
            if (_mediaElement != null)
            {
                _mediaElement.Pause();
            }
            
            if (!_capturing)
            {
                _capturing = true;

                DataContext dataContext = PerfectCamera.DataContext.Instance;

                // Reset the streams
                dataContext.ResetStreams();

                CameraCaptureSequence sequence = Camera.CreateCaptureSequence(1);
                sequence.Frames[0].CaptureStream = dataContext.FullResolutionStream.AsOutputStream();
                sequence.Frames[0].ThumbnailStream = dataContext.PreviewResolutionStream.AsOutputStream();

                await Camera.PrepareCaptureSequenceAsync(sequence);
                await sequence.StartCaptureAsync();

                _capturing = false;

                // Get the storyboard from application resources
                Storyboard sb = (Storyboard)Resources["CaptureAnimation"];
                sb.Begin();

                //Save
                await SaveCapturedPhotoToLibrary(dataContext.FullResolutionStream, dataContext.PreviewResolutionStream);

                sb.Stop();
            }

            _manuallyFocused = false;

            Camera.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);

            //resume camera
            if (_mediaElement != null)
            {
                _mediaElement.Play();
            }
        }

        private async Task SaveCapturedPhotoToLibrary(MemoryStream fullStream, MemoryStream previewStream)
        {
            try
            {
                Stream thumbnailStream = previewStream;
                if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
                {
                    thumbnailStream = await _cameraEffect.ApplyEffect(previewStream);
                }
                if (thumbnailStream != null)
                {
                    thumbnailStream.Position = 0;
                    ImageBrush brush = new ImageBrush();
                    brush.Stretch = Stretch.UniformToFill;
                    var bmp = new BitmapImage();
                    bmp.SetSource(thumbnailStream);
                    brush.ImageSource = bmp;
                    PreviewThumbnailButton.Background = brush;
                }

                Stream capturedStream = fullStream;
                if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
                {
                    capturedStream = await _cameraEffect.ApplyEffect(fullStream);
                }
                if (capturedStream != null)
                {
                    capturedStream.Position = 0;
                    using (MediaLibrary library = new MediaLibrary())
                    {
                        library.SavePictureToCameraRoll(FileNamePrefix
                                + DateTime.Now.ToString() + ".jpg", capturedStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("save ex: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Half-pressing the shutter key initiates autofocus unless tapped to focus.
        /// </summary>
        private async void ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            if (_manuallyFocused)
            {
                _manuallyFocused = false;
            }

            await AutoFocus();
        }

        /// <summary>
        /// Completely pressing the shutter key initiates capturing a photo.
        /// </summary>
        private async void ShutterKeyPressed(object sender, EventArgs e)
        {
            await Capture();
        }

        private void EffectButton_Click(object sender, RoutedEventArgs e)
        {
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
            EffectButton.IsHitTestVisible = true;
        }

        private void EffectLayoutSlideIn_Completed(object sender, EventArgs e)
        {
            HideEffectLayoutButton.IsHitTestVisible = true;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void FlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (Camera != null && Camera.SensorLocation == CameraSensorLocation.Back)
            {
                var p = new Popup()
                {
                    Child = _flashSettingMenu
                };

                _flashSettingMenu.SetCurrentFlashMode(PerfectCamera.DataContext.Instance.FlashState);

                _openPopupList.Add(p);

                p.IsOpen = true;
            }
        }

        private void SwitchCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (Microsoft.Devices.PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing)
                && Microsoft.Devices.PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
            {
                if (PerfectCamera.DataContext.Instance.SensorLocation == CameraSensorLocation.Back)
                {
                    PerfectCamera.DataContext.Instance.SensorLocation = CameraSensorLocation.Front;
                }
                else
                {
                    PerfectCamera.DataContext.Instance.SensorLocation = CameraSensorLocation.Back;
                }

                var task = ResetCamera();
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var p = new Popup()
            {
                Child = _settingMenu
            };

            _settingMenu.SetRatio(PerfectCamera.DataContext.Instance.CameraRatio);

            _openPopupList.Add(p);

            p.IsOpen = true;
        }

        private async void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_manuallyFocused)
            {
                await AutoFocus();
            }
            await Capture();
        }

        private void PreviewThumbnailButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PreviewPage.xaml", UriKind.Relative));
        }

        private void EffectNameFadeIn_Completed(object sender, EventArgs e)
        {
            EffectNameFadeOut.Begin();
        }

        //mouse event handler
        private bool _touchBegan = false;
        private Point _beginTouchLocaltion = new Point(0.0,0.0);

        private void MouseInput_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _touchBegan = true;

            _beginTouchLocaltion = e.GetPosition(this);
        }

        private void MouseInput_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_touchBegan)
            {
                _touchBegan = false;
                Point pt = e.GetPosition(this);
                if (PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
                {
                    if (pt.X < _beginTouchLocaltion.X - 20.0)
                    {
                        //next effect
                        _cameraEffect.NextEffect();
                        EffectNameTextBlock.Text = _cameraEffect.EffectName;
                        EffectNameFadeIn.Begin();
                    }
                    else if (pt.X > _beginTouchLocaltion.X + 20.0)
                    {
                        //previous effect
                        _cameraEffect.PreviousEffect();
                        EffectNameTextBlock.Text = _cameraEffect.EffectName;
                        EffectNameFadeIn.Begin();
                    }
                }

                if (pt.X > _beginTouchLocaltion.X - 20 && pt.X < _beginTouchLocaltion.X + 20 &&
                    pt.Y > _beginTouchLocaltion.Y - 20 && pt.Y < _beginTouchLocaltion.Y + 20)
                {
                    //focus
                    var task = FocusAtPoint(pt);
                }
            }
        }

        private void MouseInput_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        private void InitEffectPanel()
        {
            if (_cameraEffect != null && PerfectCamera.DataContext.Instance.CameraType == PerfectCameraType.Selfie)
            {
                EffectStackPanel.Children.Clear();
                for (int i = 0; i < _cameraEffect.EffectGroup.Count; i++)
                {
                    AbstractFilter filter = _cameraEffect.EffectGroup[i];
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
                    EffectStackPanel.Children.Add(btn);

                    btn.Click += FilterButton_Click;
                }

                EffectStackPanel.Width = _cameraEffect.EffectGroup.Count * 97 + (_cameraEffect.EffectGroup.Count + 1) * 10;
            }
        }

        void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedBtn = sender as Button;
            _selectedEffect = (int)clickedBtn.Tag;

            _cameraEffect.SetSelectedIndex(_selectedEffect);

            EffectNameTextBlock.Text = (string)clickedBtn.Content;
            EffectNameFadeIn.Begin();
        }

        private void RotateFocusImage_Completed(object sender, EventArgs e)
        {
            _focusDisplayTimer.Start();
        }

        void _focusDisplayTimer_Tick(object sender, EventArgs e)
        {
            FocusAnimation.Stop();
            FocusImage.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async Task FocusAtPoint(Point location)
        {
            if (PhotoCaptureDevice.IsFocusRegionSupported(PerfectCamera.DataContext.Instance.SensorLocation) && _focusSemaphore.WaitOne(0))
            {
                try
                {
                    _focusDisplayTimer.Stop();
                    FocusAnimation.Stop();

                    Thickness margin = new Thickness(location.X - 45, location.Y - 45, 0, 0);
                    FocusImage.Margin = margin;

                    FocusImage.Visibility = System.Windows.Visibility.Visible;
                    FocusAnimation.Begin();

                    // Get tap coordinates as a foundation point
                    Windows.Foundation.Point tapPoint = new Windows.Foundation.Point(location.X, location.Y);

                    double xRatio = VideoCanvas.ActualHeight / PerfectCamera.DataContext.Instance.PreviewResolution.Width;
                    double yRatio = VideoCanvas.ActualWidth / PerfectCamera.DataContext.Instance.PreviewResolution.Height;

                    // adjust to center focus on the tap point
                    Windows.Foundation.Point displayOrigin = new Windows.Foundation.Point(
                                tapPoint.Y - _focusRegionSize.Width / 2,
                                (VideoCanvas.ActualWidth - tapPoint.X) - _focusRegionSize.Height / 2);

                    // adjust for resolution difference between preview image and the canvas
                    Windows.Foundation.Point viewFinderOrigin = new Windows.Foundation.Point(displayOrigin.X / xRatio, displayOrigin.Y / yRatio);
                    Windows.Foundation.Rect focusrect = new Windows.Foundation.Rect(viewFinderOrigin, _focusRegionSize);

                    // clip to preview resolution
                    Windows.Foundation.Rect viewPortRect = new Windows.Foundation.Rect(0, 0, PerfectCamera.DataContext.Instance.PreviewResolution.Width, PerfectCamera.DataContext.Instance.PreviewResolution.Height);
                    focusrect.Intersect(viewPortRect);

                    Camera.FocusRegion = focusrect;

                    CameraFocusStatus status = await Camera.FocusAsync();

                    if (status == CameraFocusStatus.Locked)
                    {
                        _manuallyFocused = true;
                        Camera.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters,
                            AutoFocusParameters.Exposure & AutoFocusParameters.Focus & AutoFocusParameters.WhiteBalance);
                    }
                    else
                    {
                        _manuallyFocused = false;
                        Camera.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ex: {0}", ex.Message);
                }

                _focusSemaphore.Release();
            }
        }
    }
}