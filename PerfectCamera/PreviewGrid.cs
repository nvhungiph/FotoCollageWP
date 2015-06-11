using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;

namespace PerfectCamera
{
    class PreviewGrid: Grid
    {
        const double MaxScale = 10;

        double _scale = 1.0;
        double _minScale;
        double _coercedScale;
        double _originalScale;

        Size _viewportSize;
        bool _pinching;
        Point _screenMidpoint;
        Point _relativeMidpoint;

        private ViewportControl _viewPort = null;
        private Image _imageView = null;
        private BitmapImage _bitmap = null;
        private Canvas _imageHolderCanvas = null;
        private ScaleTransform _scaleTransform = null;

        private Picture _displayPicture = null;

        public Picture DisplayPicture
        {
            get
            {
                return _displayPicture;
            }
        }

        public PreviewGrid(Picture picture): base()
        {
            _displayPicture = picture;
            _viewPort = new ViewportControl();
            this.Children.Add(_viewPort);

            _viewPort.ManipulationStarted += OnViewportManipulationStarted;
            _viewPort.ManipulationDelta += OnViewportManipulationDelta;
            _viewPort.ManipulationCompleted += OnViewportManipulationCompleted;
            _viewPort.ViewportChanged += OnViewportChanged;

            ImageLoaded = false;

            _imageView = new Image();
            _bitmap = new BitmapImage();
            //_bitmap.SetSource(picture.GetImage());
            _imageView.Source = _bitmap;
            _imageView.Stretch = Stretch.Uniform;
            _imageView.RenderTransformOrigin = new Point(0, 0);

            _scaleTransform = new ScaleTransform();
            _imageView.RenderTransform = _scaleTransform;

            _imageHolderCanvas = new Canvas();
            _imageHolderCanvas.Children.Add(_imageView);

            _viewPort.Content = _imageHolderCanvas;

            //LoadImage();
        }

        public bool ImageLoaded { get; set; }

        public void LoadImage()
        {
            if (!ImageLoaded)
            {
                ImageLoaded = true;
                _bitmap.SetSource(_displayPicture.GetImage());

                // Set scale to the minimum, and then save it. 
                _scale = 0;
                CoerceScale(true);
                _scale = _coercedScale;

                ResizeImage(true);
            }
        }

        public BitmapImage Bitmap
        {
            get
            {
                return _bitmap;
            }
        }

        void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            Size newSize = new Size(_viewPort.Viewport.Width, _viewPort.Viewport.Height);
            if (newSize != _viewportSize)
            {
                _viewportSize = newSize;
                CoerceScale(true);
                ResizeImage(false);
            }
        }

        void OnViewportManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            _pinching = false;
            _scale = _coercedScale;
        }

        void OnViewportManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                e.Handled = true;

                if (!_pinching)
                {
                    _pinching = true;
                    Point center = e.PinchManipulation.Original.Center;
                    _relativeMidpoint = new Point(center.X / _imageView.ActualWidth, center.Y / _imageView.ActualHeight);

                    var xform = _imageView.TransformToVisual(_viewPort);
                    _screenMidpoint = xform.Transform(center);
                }

                _scale = _originalScale * e.PinchManipulation.CumulativeScale;

                CoerceScale(false);
                ResizeImage(false);
            }
            else if (_pinching)
            {
                _pinching = false;
                _originalScale = _scale = _coercedScale;
            }
        }

        void OnViewportManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            _pinching = false;
            _originalScale = _scale;
        }

        /// <summary> 
        /// Adjust the size of the image according to the coerced scale factor. Optionally 
        /// center the image, otherwise, try to keep the original midpoint of the pinch 
        /// in the same spot on the screen regardless of the scale. 
        /// </summary> 
        /// <param name="center"></param> 
        void ResizeImage(bool center)
        {
            if (_coercedScale != 0 && _bitmap != null)
            {
                double newWidth = _imageHolderCanvas.Width = Math.Round(_bitmap.PixelWidth * _coercedScale);
                double newHeight = _imageHolderCanvas.Height = Math.Round(_bitmap.PixelHeight * _coercedScale);

                _scaleTransform.ScaleX = _scaleTransform.ScaleY = _coercedScale;

                _viewPort.Bounds = new Rect(0, 0, newWidth, newHeight);

                if (center)
                {
                    _viewPort.SetViewportOrigin(
                        new Point(
                            Math.Round((newWidth - _viewPort.ActualWidth) / 2),
                            Math.Round((newHeight - _viewPort.ActualHeight) / 2)
                            ));
                }
                else
                {
                    Point newImgMid = new Point(newWidth * _relativeMidpoint.X, newHeight * _relativeMidpoint.Y);
                    Point origin = new Point(newImgMid.X - _screenMidpoint.X, newImgMid.Y - _screenMidpoint.Y);
                    _viewPort.SetViewportOrigin(origin);
                }
            }
        }

        /// <summary> 
        /// Coerce the scale into being within the proper range. Optionally compute the constraints  
        /// on the scale so that it will always fill the entire screen and will never get too big  
        /// to be contained in a hardware surface. 
        /// </summary> 
        /// <param name="recompute">Will recompute the min max scale if true.</param> 
        void CoerceScale(bool recompute)
        {
            if (recompute && _bitmap != null && _viewPort != null)
            {
                // Calculate the minimum scale to fit the viewport 
                double minX = _viewPort.ActualWidth / _bitmap.PixelWidth;
                double minY = _viewPort.ActualHeight / _bitmap.PixelHeight;

                _minScale = Math.Min(minX, minY);
            }

            _coercedScale = Math.Min(MaxScale, Math.Max(_scale, _minScale));

        }
    }
}
