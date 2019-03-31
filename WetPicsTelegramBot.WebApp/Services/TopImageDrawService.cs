using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    class TopImageDrawService : ITopImageDrawService
    {
        private readonly int _imageWidth = 600;
        private readonly int _imageHeight = 900;
        private readonly Rgba32 _borderColor = Rgba32.Teal;

        public Stream DrawTopImage(List<Stream> imageStreams)
        {
            var sourceImages = imageStreams
                .AsParallel()
                .Select(Image.Load)
                .Select(Resize)
                .ToList();

            sourceImages
                .Skip(1)
                .AsParallel()
                .Select(RemoveTriangle)
                .ToList();

            var resultImageWidth = (sourceImages.Count - 1) * _imageWidth / 3 * 2 + _imageWidth;
            var resultImage = new Image<Rgba32>(Configuration.Default, resultImageWidth, _imageHeight);
            
            resultImage.Mutate(x => x.ApplyProcessor(new DrawImages(sourceImages)));

            int shift = _imageWidth / 3 * 1;

            FontCollection collection = new FontCollection();
            FontFamily impact = collection.Install("impact.ttf");
            var font = new Font(impact, 150);

            Parallel.For(0, sourceImages.Count - 1, (i, state) =>
            {
                resultImage.Mutate(x 
                    => x.DrawLines(new GraphicsOptions(true),
                        _borderColor,
                        8,
                        new PointF(_imageWidth * (i + 1) - shift * i, _imageHeight),
                        new PointF((_imageWidth - shift) * (i + 1), 0)));
            });


            Parallel.For(0, sourceImages.Count, (i, state) =>
            {
                resultImage.Mutate(x 
                    => x.DrawText(new TextGraphicsOptions(true),
                        $"{i + 1}",
                        font,
                        new SolidBrush<Rgba32>(_borderColor),
                        new Pen<Rgba32>(Rgba32.WhiteSmoke, 5),
                        new PointF(_imageWidth * (i + 1) - shift * i - 130, _imageHeight - 180)));
            });

            resultImage.Mutate(x
                => x.DrawLines(_borderColor,
                    16,
                    new PointF(0, 0),
                    new PointF(0, resultImage.Height),
                    new PointF(resultImage.Width, resultImage.Height),
                    new PointF(resultImage.Width, 0),
                    new PointF(0, 0)));

            var ms = new MemoryStream();
            resultImage.SaveAsJpeg(ms, new JpegEncoder {Quality = 95});

            return ms;
        }

        private Image<Rgba32> Resize(Image<Rgba32> image)
        {
            image.Mutate(y => y.Resize(new ResizeOptions
                                 {
                                     Size = new Size(_imageWidth, _imageHeight),
                                     Mode = ResizeMode.Crop,
                                 }));

            return image;
        }

        private Image<Rgba32> RemoveTriangle(Image<Rgba32> image)
        {
            image.Mutate(y => y.ApplyProcessor(new TriangleRemoveImageProcessor()));

            return image;
        }
    }

    class DrawImages : IImageProcessor<Rgba32>
    {
        private readonly List<Image<Rgba32>> _resizedImages;

        public DrawImages(List<Image<Rgba32>> resizedImages)
        {
            _resizedImages = resizedImages;
        }

        public void Apply(Image<Rgba32> source, Rectangle sourceRectangle)
        {
            DrawManual(source, sourceRectangle);
        }

        private void DrawManual(Image<Rgba32> source, Rectangle sourceRectangle)
        {
            int shift = _resizedImages.First().Width / 3 * 2;
            
            for (var currentImageIndex = 0; currentImageIndex < _resizedImages.Count; currentImageIndex++)
            {
                var resizedImage = _resizedImages[currentImageIndex];
                var index = currentImageIndex;

                Parallel.For(currentImageIndex * shift, currentImageIndex * shift + resizedImage.Width, (x, state1) =>
                {
                    for (int y = 0; y < sourceRectangle.Height; y++)
                    {
                        var newPixel = resizedImage[x - index * shift, y];
                        if (newPixel.A != 0)
                            source[x, y] = newPixel;
                    }
                });
            }
        }
    }


    class TriangleRemoveImageProcessor : IImageProcessor<Rgba32>
    {
        public void Apply(Image<Rgba32> source, Rectangle sourceRectangle)
        {
            var startWidth = sourceRectangle.Width / 3;

            Parallel.For(0, sourceRectangle.Height, (i, state) =>
            {
                var v = (int)(startWidth * (double)i / sourceRectangle.Height + 1);

                for (int j = 0; j < v; j++)
                {
                    source[j, i] = new Rgba32(0, 0, 0, 0);
                }
            });
        }
    }
}
