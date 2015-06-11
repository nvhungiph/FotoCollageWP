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
using System.Windows.Shapes;

namespace PerfectCamera.Filters.FilterControls
{
    public partial class CropRegionControl : UserControl
    {
        public Action DidUpdateCropRect { get; set; }

        private Rectangle _draggedRect = null;

        public CropRegionControl()
        {
            InitializeComponent();

            var rects = new Rectangle[] { rectTopRight, rectTopLeft, rectBotRight, rectBotLeft };
            Point _dragOrigin = new Point();
            double origLeftPerc = 0, origRightPerc = 0, origTopPerc = 0, origBotPerc = 0;

            var setOrigin = new Action<Point>((p) =>
            {
                _dragOrigin = p;
                origLeftPerc = this._clipLeftPerc;
                origRightPerc = this._clipRightPerc;
                origTopPerc = this._clipTopPerc;
                origBotPerc = this._clipBotPerc;
            });

            foreach (var aRect in rects)
            {
                aRect.MouseLeftButtonDown += (s, e) =>
                {
                    var r = (Rectangle)s;
                    _draggedRect = r;
                    setOrigin(e.GetPosition(this.imgSauce));

                    r.CaptureMouse();
                };

                aRect.MouseLeftButtonUp += (s, e) =>
                {
                    _draggedRect = null;
                };

                aRect.MouseMove += (s, e) =>
                {
                    if (_draggedRect != null)
                    {

                        var pos = e.GetPosition(this.imgSauce);

                        if (s == this.rectTopLeft || s == this.rectTopRight)
                        {
                            // Adjust top
                            _clipTopPerc = origTopPerc + (pos.Y - _dragOrigin.Y) / imgSauce.ActualHeight;
                        }
                        if (s == this.rectTopLeft || s == this.rectBotLeft)
                        {
                            // Adjust Left
                            _clipLeftPerc = origLeftPerc + (pos.X - _dragOrigin.X) / imgSauce.ActualWidth;
                        }
                        if (s == this.rectBotLeft || s == this.rectBotRight)
                        {
                            // Adjust bottom
                            _clipBotPerc = origBotPerc - (pos.Y - _dragOrigin.Y) / imgSauce.ActualHeight;
                        }
                        if (s == this.rectTopRight || s == this.rectBotRight)
                        {
                            // Adjust Right
                            _clipRightPerc = origRightPerc - (pos.X - _dragOrigin.X) / imgSauce.ActualWidth;
                        }

                        this.updateClipAndTransforms();
                    }
                };
            }

            var draggingImg = false;

            imgSauce.MouseLeftButtonDown += (s, e) =>
            {
                setOrigin(e.GetPosition(this.imgSauce));
                imgSauce.CaptureMouse();
                draggingImg = true;
            };

            imgSauce.MouseLeftButtonUp += (s, e) =>
            {
                draggingImg = false;
            };

            imgSauce.MouseMove += (s, e) =>
            {
                if (draggingImg)
                {
                    var pos = e.GetPosition(this.imgSauce);

                    var xAdjust = (pos.X - _dragOrigin.X) / imgSauce.ActualWidth;
                    var yAdjust = (pos.Y - _dragOrigin.Y) / imgSauce.ActualHeight;

                    _clipLeftPerc = origLeftPerc + xAdjust;
                    _clipRightPerc = origRightPerc - xAdjust;
                    _clipTopPerc = origTopPerc + yAdjust;
                    _clipBotPerc = origBotPerc - yAdjust;

                    this.updateClipAndTransforms();
                }
            };

            imgSauce.SizeChanged += (x, y) =>
            {
                this.updateClipAndTransforms();
            };

            this.updateClipAndTransforms();
        }

        private double _clipLeftPerc, _clipRightPerc, _clipTopPerc, _clipBotPerc =  0;
 
        void updateClipAndTransforms()
        {
            // Check bounds
            if (_clipLeftPerc + _clipRightPerc >= 1)
                _clipLeftPerc = (1 - _clipRightPerc) - 0.04;
            if (_clipTopPerc + _clipBotPerc >= 1)
                _clipTopPerc = (1 - _clipBotPerc) - 0.04;
 
            if (_clipLeftPerc < 0)
                _clipLeftPerc = 0;
            if (_clipRightPerc < 0)
            _clipRightPerc = 0;
            if (_clipBotPerc < 0)
                _clipBotPerc = 0;
            if (_clipTopPerc < 0)
                _clipTopPerc = 0;
            if (_clipLeftPerc >= 1)
                _clipLeftPerc = 0.99;
            if (_clipRightPerc >= 1)
                _clipRightPerc = 0.99;
            if (_clipBotPerc >= 1)
                _clipBotPerc = 0.99;
            if (_clipTopPerc >= 1)
                _clipTopPerc = 0.99;
            
 
            // Image Clip
            var leftX = _clipLeftPerc * this.imgSauce.ActualWidth;
            var topY = _clipTopPerc * this.imgSauce.ActualHeight;
            
            clipRect.Rect = new Rect(leftX, topY, (1 -_clipRightPerc) * this.imgSauce.ActualWidth - leftX, (1 - _clipBotPerc) *  this.imgSauce.ActualHeight - topY);
 
            // Rectangle Transforms
            ((TranslateTransform)this.rectTopLeft.RenderTransform).X = clipRect.Rect.X;
            ((TranslateTransform)this.rectTopLeft.RenderTransform).Y = clipRect.Rect.Y;
            ((TranslateTransform)this.rectTopRight.RenderTransform).X = -_clipRightPerc * this.imgSauce.ActualWidth;
            ((TranslateTransform)this.rectTopRight.RenderTransform).Y = clipRect.Rect.Y;
            ((TranslateTransform)this.rectBotLeft.RenderTransform).X = clipRect.Rect.X;
            ((TranslateTransform)this.rectBotLeft.RenderTransform).Y = - _clipBotPerc *  this.imgSauce.ActualHeight;
            ((TranslateTransform)this.rectBotRight.RenderTransform).X = -_clipRightPerc * this.imgSauce.ActualWidth;
            ((TranslateTransform)this.rectBotRight.RenderTransform).Y = -_clipBotPerc * this.imgSauce.ActualHeight;

            if (DidUpdateCropRect != null)
            {
                DidUpdateCropRect();
            }
        }
 
    }
}
