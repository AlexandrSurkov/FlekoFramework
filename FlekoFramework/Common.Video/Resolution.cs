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
    }
}