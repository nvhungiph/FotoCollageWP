/*
 * Copyright (c) 2014 Microsoft Mobile
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using Lumia.Imaging.Custom;
using PerfectCamera.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Lumia.Imaging.Compositing;
using Lumia.Imaging.Transforms;
using PerfectCamera.Filters;
using PerfectCamera.Filters.MagicSkin;
using PerfectCamera.Filters.Funny;
using PerfectCamera.Filters.Lomo;

namespace PerfectCamera
{
    public class EffectIndex
    {
        public int EffectIdx { get; set; }
        public int FilterIdx { get; set; }

        public EffectIndex()
        {
            EffectIdx = 0;
            FilterIdx = 0;
        }

        public EffectIndex(int eIdx, int fIdx)
        {
            EffectIdx = eIdx;
            FilterIdx = fIdx;
        }
    }

    public class Effects : ICameraEffect
    {
        private PhotoCaptureDevice _photoCaptureDevice = null;
        private CameraPreviewImageSource _cameraPreviewImageSource = null;
        private int _effectIndex = 0;
        private Semaphore _semaphore = new Semaphore(1, 1);

        public EffectGroup<AbstractFilter> EffectGroup = null;

        public Effects()
        {
            InitializeEffectList();
        }

        public String EffectName
        {
            get
            {
                if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count)
                {
                    AbstractFilter filter = EffectGroup[_effectIndex];
                    return filter.Name;
                }

                return "";
            }
        }

        public PhotoCaptureDevice PhotoCaptureDevice
        {
            set
            {
                if (_photoCaptureDevice != value)
                {
                    while (!_semaphore.WaitOne(100));

                    _photoCaptureDevice = value;

                    Initialize();

                    _semaphore.Release();
                }
            }
        }

        public CameraPreviewImageSource CameraPreviewImageSource
        {
            set
            {
                if (_cameraPreviewImageSource != value)
                {
                    while (!_semaphore.WaitOne(100)) ;

                    _cameraPreviewImageSource = value;

                    _semaphore.Release();
                }
            }
        }

        ~Effects()
        {
            while (!_semaphore.WaitOne(100));

            if (_cameraPreviewImageSource != null)
            {
                _cameraPreviewImageSource.Dispose();
                _cameraPreviewImageSource = null;
            }

            Uninitialize();

            _semaphore.Release();
        }

        public async Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize)
        {
            if (_semaphore.WaitOne(500))
            {
                _cameraPreviewImageSource.InvalidateLoad();

                var scanlineByteSize = (uint)frameSize.Width * 4; // 4 bytes per pixel in BGRA888 mode
                var bitmap = new Bitmap(frameSize, ColorMode.Bgra8888, scanlineByteSize, frameBuffer);

                if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count)
                {
                    AbstractFilter filter = EffectGroup[_effectIndex];
                    await filter.RenderPreview(bitmap);
                }
                else
                {
                    var renderer = new BitmapRenderer(_cameraPreviewImageSource, bitmap);
                    await renderer.RenderAsync();
                }

                _semaphore.Release();
            }
        }

        public void NextEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();

                if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count)
                {
                    _effectIndex++;
                    if (_effectIndex >= EffectGroup.Count)
                    {
                        _effectIndex = 0;
                    }
                }

                Initialize();

                _semaphore.Release();
            }
        }

        public void PreviousEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();

                if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count)
                {
                    _effectIndex--;
                    if (_effectIndex < 0)
                    {
                        _effectIndex = EffectGroup.Count-1;
                    }
                }

                Initialize();

                _semaphore.Release();
            }
        }

        public void SetSelectedIndex(int idx)
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();

                _effectIndex = idx;

                Initialize();

                _semaphore.Release();
            }
        }

        private void Uninitialize()
        {
            /*if (_cameraPreviewImageSource != null)
            {
                _cameraPreviewImageSource.Dispose();
                _cameraPreviewImageSource = null;
            }*/
        }

        private void Initialize()
        {
            if (_cameraPreviewImageSource == null && _photoCaptureDevice != null)
            {
                _cameraPreviewImageSource = new CameraPreviewImageSource(_photoCaptureDevice);
            }
            
            if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count && _cameraPreviewImageSource != null)
            {
                AbstractFilter filter = EffectGroup[_effectIndex];
                filter.RealtimeEffectSource = _cameraPreviewImageSource;
            }
        }

        public async Task<Stream> ApplyEffect(MemoryStream inputStream)
        {
            if (_effectIndex >= 0 && _effectIndex < EffectGroup.Count)
            {
                AbstractFilter filter = EffectGroup[_effectIndex];
                IBuffer outputBuffer = null;

                if (_semaphore.WaitOne(500))
                {
                    outputBuffer = await filter.RenderJpegAsync(inputStream.GetWindowsRuntimeBuffer());
                    _semaphore.Release();
                }

                if (outputBuffer != null)
                {
                    return outputBuffer.AsStream();
                }
            }
            return null;
        }

        //effect list
        private void InitializeEffectList()
        {
            EffectGroup = new EffectGroup<AbstractFilter>()
            {
                new LomoNeutralFilter(),
                new LomoRedFilter(),
                new LomoGreenFilter(),
                new LomoBlueFilter(),
                new LomoYellowFilter()
            };
        }
    }
}