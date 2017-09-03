// ReSharper disable InconsistentNaming
// ReSharper disable ConvertPropertyToExpressionBody
namespace Flekosoft.Common.Video
{
    public class Resolution
    {
        public Resolution(int width, int height)
        {
            Height = height;
            Width = width;
        }

        public Resolution(Resolution resolution)
        {
            Height = resolution.Height;
            Width = resolution.Width;
        }

        public int Height { get; }
        public int Width { get; }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        /// <summary>
        /// QCIF	176 x 120	Quarter CIF(half the height and width as CIF)
        /// </summary>
        public static Resolution ResolutionQCIF { get { return new Resolution(176, 120); } }

        /// <summary>
        /// CIF	352 x 240
        /// </summary>
        public static Resolution ResolutionCIF { get { return new Resolution(352, 240); } }

        /// <summary>
        /// 2CIF	704 x 240	2 times CIF width
        /// </summary>
        public static Resolution Resolution2CIF { get { return new Resolution(704, 240); } }

        /// <summary>
        /// 4CIF	704 x 480	2 times CIF width and 2 times CIF height
        /// </summary>
        public static Resolution Resolution4CIF { get { return new Resolution(704, 480); } }

        /// <summary>
        /// D1	720 x 480	aka "Full D1"
        /// </summary>
        public static Resolution ResolutionD1 { get { return new Resolution(720, 480); } }

        /// <summary>
        /// 720p HD	1280 x 720	720p High Definition
        /// </summary>
        public static Resolution Resolution720p { get { return new Resolution(1280, 720); } }

        /// <summary>
        /// 960p HD	1280 x 960	960p High Definition(Sony HD standard)
        /// </summary>
        public static Resolution Resolution960p { get { return new Resolution(1280, 960); } }

        /// <summary>
        /// 1.3 MP	1280 x 1024	aka "1 Megapixel" or "1MP"
        /// </summary>
        public static Resolution Resolution1MP { get { return new Resolution(1280, 1024); } }

        /// <summary>
        /// 2 MP	1600 x 1200	2 Megapixel
        /// </summary>
        public static Resolution Resolution2MP { get { return new Resolution(1600, 1200); } }

        /// <summary>
        /// 1080p HD	1920 x 1080	1080p High Definition
        /// </summary>
        public static Resolution Resolution1080p { get { return new Resolution(1920, 1080); } }

        /// <summary>
        /// 3 MP	2048 x 1536	3 Megapixel
        /// </summary>
        public static Resolution Resolution3MP { get { return new Resolution(2048, 1536); } }

        /// <summary>
        /// 5 MP	2592 x 1944	5 Megapixel
        /// </summary>
        public static Resolution Resolution5MP { get { return new Resolution(2592, 1944); } }
    }
}