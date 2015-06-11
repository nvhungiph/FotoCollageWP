using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lumia.Imaging;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Transforms;
using PerfectCamera.Filters.FilterControls;
using PerfectCamera.Resources;
using System.Windows.Controls;
using System.Diagnostics;
using Lumia.Imaging.Custom;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Windows.Media.Imaging;

namespace PerfectCamera.Filters.Funny
{
    public class FishEyeEffect: CustomEffectBase
    {
        public FishEyeEffect(IImageProvider source)
            : base(source)
        {

        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;
            int rowindex = 0;
            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                // normalize y coordinate to -1 ... 1 
                double ny = ((2 * rowindex) / sourcePixelRegion.ImageSize.Height) - 1;
                // for each column 
                for (int x = 0; x < sourcePixelRegion.ImageSize.Width; x++, index++)
                {
                    // normalize x coordinate to -1 ... 1 
                    double nx = ((2 * x) / sourcePixelRegion.ImageSize.Width) - 1;
                    // calculate distance from center (0,0)
                    double radius = Math.Sqrt(nx * nx + ny * ny);

                    bool fillBlack = true;
                    // discard pixels outside from circle! 
                    if (0 <= radius && radius <= 1)
                    {
                        //compute the distorted radius
                        double newRadius = (radius + (1 - Math.Sqrt(1 - radius * radius))) / 2;
                        // discard radius greater than 1, which will result in black zones
                        if (newRadius <= 1)
                        {
                            // calculate the angle for polar coordinates 
                            double theta = Math.Atan2(ny, nx);
                            // calculate new x,y position using new distance in same angle 
                            double nxn = newRadius * Math.Cos(theta);
                            double nyn = newRadius * Math.Sin(theta);
                            // map from -1 ... 1 to image coordinates
                            int x2 = (int)(((nxn + 1) * sourcePixelRegion.ImageSize.Width) / 2);
                            int y2 = (int)(((nyn + 1) * sourcePixelRegion.ImageSize.Height) / 2);
                            // find (x2,y2) position from source pixels 
                            int srcpos = (int)(y2 * sourcePixelRegion.ImageSize.Width + x2);
                            // make sure that position stays within arrays 
                            if (srcpos >= 0 & srcpos < sourcePixelRegion.ImageSize.Width * sourcePixelRegion.ImageSize.Height)
                            {
                                targetPixels[index] = sourcePixels[srcpos];
                                fillBlack = false;
                            }
                        }
                    }
                    
                    if (fillBlack)
                    {
                        targetPixels[index] = 0xff000000;
                    }
                }
                rowindex++;
            });
        }
    }

    public class FishEyeFilter: AbstractFilter
    {
        private const string DebugTag = "FishEyeFilter: ";

        protected FishEyeEffect _fishEyeEffect;
        public FishEyeFilter(): base()
        {
            Name = "Fish Eye";
            ShortDescription = "Fish Eye";
        }

        protected override void SetFilters(FilterEffect effect)
        {
            _fishEyeEffect = new FishEyeEffect(Source);
        }

        public async override Task<IBuffer> RenderJpegAsync(IBuffer buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                Debug.WriteLine(DebugTag + Name + ": RenderJpegAsync(): The given buffer is null or empty!");
                return null;
            }

            IBuffer outputBuffer;

            using (var source = new BufferImageSource(buffer))
            {
                _fishEyeEffect.Source = Source;

                using (var renderer = new JpegRenderer(_fishEyeEffect))
                {
                    outputBuffer = await renderer.RenderAsync();
                }

                _fishEyeEffect.Dispose();
            }

            return outputBuffer;
        }

        protected override async void Render()
        {
            try
            {
                if (Source != null)
                {
                    Debug.WriteLine(DebugTag + Name + ": Rendering...");

                    foreach (var change in Changes)
                    {
                        change();
                    }

                    Changes.Clear();

                    _fishEyeEffect.Source = Source;

                    using (var renderer = new WriteableBitmapRenderer(_fishEyeEffect, TmpBitmap))
                    {
                        await renderer.RenderAsync();
                    }

                    TmpBitmap.Pixels.CopyTo(PreviewBitmap.Pixels, 0);
                    PreviewBitmap.Invalidate(); // Force a redraw
                }
                else
                {
                    Debug.WriteLine(DebugTag + Name + ": Render(): No buffer set!");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(DebugTag + Name + ": Render(): " + e.Message);
            }
            finally
            {
                switch (State)
                {
                    case States.Apply:
                        State = States.Wait;
                        break;
                    case States.Schedule:
                        State = States.Apply;
                        Render();
                        break;
                    default:
                        break;
                }
            }
        }

        

    }
}
