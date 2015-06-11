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
using Lumia.Imaging.Compositing;
using System.IO;

namespace PerfectCamera.Filters.MagicSkin
{
    public class MagicSkinNaturalFilter: AbstractFilter
    {
        private const string DebugTag = "MagicSkinNaturalFilter: ";

        public MagicSkinNaturalFilter(): base()
        {
            Name = "Natural";
            ShortDescription = "Natural";
        }

        protected override void SetFilters(FilterEffect effect)
        {
            
        }

        public async override Task<IBuffer> RenderJpegAsync(IBuffer buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                Debug.WriteLine(DebugTag + Name + ": RenderJpegAsync(): The given buffer is null or empty!");
                return null;
            }

            IBuffer outputBuffer = null;

            using (var source = new BufferImageSource(buffer))
            {
                /*_highpassEffect.Source = Source;

                using (var renderer = new JpegRenderer(_highpassEffect))
                {
                    outputBuffer = await renderer.RenderAsync();
                }

                _highpassEffect.Dispose();*/
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

                    //blend source layer with mode overlay
                    var blendEffect = new BlendEffect();
                    blendEffect.Source = Source;// new StreamImageSource(new MemoryStream());
                    blendEffect.ForegroundSource = Source;
                    blendEffect.BlendFunction = BlendFunction.Overlay;
                    blendEffect.GlobalAlpha = 0.0;

                    var renderer = new JpegRenderer(blendEffect);
                    var outBuffer = await renderer.RenderAsync();

                    blendEffect.Dispose();

                    var highPassSource = new BufferImageSource(outBuffer);
                    var highPassFilter = new HighpassEffect(6, false, 1);
                    highPassFilter.Source = highPassSource;
                    var highPassRenderer = new JpegRenderer(highPassFilter);

                    var highPassOutBuffer = await highPassRenderer.RenderAsync();

                    highPassFilter.Dispose();

                    var negativeFilter = new NegativeFilter();
                    var filterEffect = new FilterEffect();
                    filterEffect.Filters = new List<IFilter>() { negativeFilter };

                    filterEffect.Source = new BufferImageSource(highPassOutBuffer);
                    var invertRenderer = new JpegRenderer(filterEffect);
                    var invertOutBuffer = await invertRenderer.RenderAsync();


                    blendEffect = new BlendEffect(Source, new BufferImageSource(invertOutBuffer), BlendFunction.Overlay, 0.5);
                    using (var bmpRender = new WriteableBitmapRenderer(blendEffect, TmpBitmap))
                    {
                        await bmpRender.RenderAsync();
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
