using Lumia.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;

namespace PerfectCamera
{
    public class DataContext
    {
        /// <summary>
        /// Returns the singleton instance of this class.
        /// </summary>
        private static DataContext _instance;
        public static DataContext Instance
        {
            get { return _instance ?? (_instance = new DataContext()); }
        }

        public MemoryStream PreviewResolutionStream
        {
            get;
            set;
        }

        public MemoryStream FullResolutionStream
        {
            get;
            set;
        }

        public Windows.Foundation.Size PreviewResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Private contructor.
        /// </summary>
        private DataContext()
        {
            PreviewResolutionStream = new MemoryStream();
            FullResolutionStream = new MemoryStream();

            CameraRatio = DefaultCameraRatio;
            SensorLocation = DefaultSensorLocation;
            FlashState = DefaultFlashState;
            CameraType = PerfectCameraType.EasyCam;
        }

        public void ResetStreams()
        {
            PreviewResolutionStream.Seek(0, SeekOrigin.Begin);
            FullResolutionStream.Seek(0, SeekOrigin.Begin);
        }

        private const CameraRatio DefaultCameraRatio = CameraRatio.Ratio_16x9;
        private const CameraSensorLocation DefaultSensorLocation = CameraSensorLocation.Back;
        private const FlashState DefaultFlashState = FlashState.Auto;

        public CameraRatio CameraRatio { get; set; }
        public CameraSensorLocation SensorLocation { get; set; }
        public FlashState FlashState { get; set; }

        public PerfectCameraType CameraType { get; set; }
    }
}
