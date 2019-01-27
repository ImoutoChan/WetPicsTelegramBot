using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class TelegramImagePreparing : ITelegramImagePreparing
    {
        private static readonly int _photoSizeLimit = 1024 * 1024 * 5;
        private static readonly int _photoHeightLimit = 2560;
        private static readonly int _photoWidthLimit = 2560;

        private readonly ILogger<TelegramImagePreparing> _logger;


        public TelegramImagePreparing(ILogger<TelegramImagePreparing> logger)
        {
            _logger = logger;
        }


        public Stream Prepare(Stream input, long inputLength)
        {
            _logger.LogTrace($"ContentLength: {inputLength} | SizeLimit: {_photoSizeLimit}");
            _logger.LogTrace($"Resizing (rescale: {inputLength >= _photoSizeLimit})");

            var inputStream = Reread(input);

            var resizedStream = MonoResize(inputStream, inputLength >= _photoSizeLimit);
            _logger.LogTrace($"New ContentLength: {resizedStream.Length} | SizeLimit: {_photoSizeLimit}");

            return resizedStream;
        }

        private Stream Reread(Stream input)
        {
            var outStream = new MemoryStream();

            input.CopyTo(outStream);

            outStream.Seek(0, SeekOrigin.Begin);

            input.Dispose();

            return outStream;
        }

        private Stream MonoResize(Stream inputStream, bool resize = false)
        {
            using (var image = new Bitmap(inputStream))
            {
                double scaleFactor = CalculateScaleFactor(resize, image);

                _logger.LogTrace($"Scale factor: {scaleFactor}");

                if (Math.Abs(scaleFactor - 1) < double.Epsilon)
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    return inputStream;
                }

                using (var resized = MonoBitmapResize(image,
                                                      (int) (image.Width * scaleFactor),
                                                      (int) (image.Height * scaleFactor)))
                {
                    var outStream = new MemoryStream();

                    resized.Save(outStream, ImageFormat.Jpeg);
                    outStream.Seek(0, SeekOrigin.Begin);
                    inputStream.Dispose();

                    return outStream.Length >= _photoSizeLimit
                        ? MonoResize(outStream, true)
                        : outStream;
                }
            }
        }

        private static double CalculateScaleFactor(bool resize, Bitmap image)
        {
            double minRatio = 1;

            if (image.Height - _photoHeightLimit >= 0 || image.Width - _photoWidthLimit >= 0)
            {
                double ratioH = _photoHeightLimit / (double) image.Height;
                double ratioW = _photoWidthLimit / (double) image.Width;

                ratioH = Math.Min(1, ratioH);
                ratioW = Math.Min(1, ratioW);

                minRatio = Math.Min(ratioW, ratioH);
            }

            if (resize)
            {
                minRatio = Math.Min(0.9, minRatio);
            }

            return minRatio;
        }

        private static Bitmap MonoBitmapResize(Bitmap sourcePhoto, int destWidth, int destHeight)
        {
            var resultPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);

            using (var grPhoto = Graphics.FromImage(resultPhoto))
            {

                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.CompositingQuality = CompositingQuality.HighQuality;
                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

                grPhoto.DrawImage(
                    sourcePhoto,
                    new Rectangle(0, 0, destWidth, destHeight),
                    new Rectangle(0, 0, sourcePhoto.Width, sourcePhoto.Height),
                    GraphicsUnit.Pixel);
            }

            return resultPhoto;
        }
    }
}