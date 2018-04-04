using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Processing.Transforms.Resamplers;
using System.IO;
using SixLabors.ImageSharp.Memory;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class TelegramImagePreparing : ITelegramImagePreparing
    {
        private static readonly int _photoSizeLimit = 1024 * 1024 * 5;
        private static readonly int _photoHeightLimit = 1024 * 5 - 1;
        private static readonly int _photoWidthLimit = 1024 * 5 - 1;

        private readonly ILogger<TelegramImagePreparing> _logger;


        public TelegramImagePreparing(ILogger<TelegramImagePreparing> logger)
        {
            _logger = logger;
        }


        public Stream Prepare(Stream input, long inputLength)
        {
            _logger.LogTrace($"ContentLength: {inputLength} | SizeLimit: {_photoSizeLimit}");
           
            _logger.LogTrace($"Resizing (rescale: {inputLength >= _photoSizeLimit})");
            var resizedStream = Resize(input, inputLength >= _photoSizeLimit);
            _logger.LogTrace($"New ContentLength: {resizedStream.Length} | SizeLimit: {_photoSizeLimit}");

            return resizedStream;
        }

        private static Stream Resize(Stream stream, bool resize = false)
        {
            using (var image = Image.Load(stream))
            {
                stream.Dispose();

                if (image.Height - _photoHeightLimit > 0 || image.Width - _photoWidthLimit > 0)
                {
                    double ratioH = _photoHeightLimit / (double)image.Height;
                    double ratioW = _photoWidthLimit / (double)image.Width;

                    ratioH = ratioH >= 1 ? 1 : ratioH;
                    ratioW = ratioW >= 1 ? 1 : ratioW;

                    var minRatio = ratioW < ratioH ? ratioW : ratioH;

                    image.Mutate(x 
                        => x.Resize((int) (image.Width * minRatio), 
                                    (int) (image.Height * minRatio),
                                    new Lanczos3Resampler()));
                }
                else if (resize)
                {
                    image.Mutate(x 
                        => x.Resize(
                            (int)(image.Width * 0.9), 
                            (int)(image.Height * 0.9),
                                    new Lanczos3Resampler()));
                }

                var outStream = new MemoryStream();


                image.SaveAsJpeg(outStream, new JpegEncoder { Quality = 95, });
                outStream.Seek(0, SeekOrigin.Begin);

                return outStream.Length >= _photoSizeLimit
                    ? Resize(outStream, true)
                    : outStream;
            }
        }
    }
}