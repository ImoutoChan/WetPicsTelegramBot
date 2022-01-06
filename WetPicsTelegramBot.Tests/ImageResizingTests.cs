using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using WetPicsTelegramBot.WebApp.Services;
using Xunit;

namespace WetPicsTelegramBot.Tests
{
    public class ImageResizingTests
    {
        // [Fact]
        // public void ResizeSampleImage()
        // {
        //     var inputImage = @"Q:\!playground\wetpicsbot-image-resizing-test\original.jpg";
        //     var outputImage = @"Q:\!playground\wetpicsbot-image-resizing-test\resized.jpg";
        //
        //     var preparer = new TelegramImagePreparing(NullLogger<TelegramImagePreparing>.Instance);
        //
        //     var stream = File.OpenRead(inputImage);
        //     var preparedImage = preparer.Prepare(stream, stream.Length);
        //
        //     var outStream = File.OpenWrite(outputImage);
        //
        //     preparedImage.CopyTo(outStream);
        //     preparedImage.Dispose();
        //     stream.Dispose();
        //     outStream.Dispose();
        // }

        [Fact]
        public void NetVipsResizeSampleImage()
        {
            var inputImage = @"Q:\!playground\wetpicsbot-image-resizing-test\original.jpg";
            var outputImage = @"Q:\!playground\wetpicsbot-image-resizing-test\resized.jpg";

            var preparer = new NetVipsTelegramImagePreparer();

            var stream = File.OpenRead(inputImage);
            var preparedImage = preparer.Prepare(stream, stream.Length);

            var outStream = File.OpenWrite(outputImage);

            preparedImage.CopyTo(outStream);
            preparedImage.Dispose();
            stream.Dispose();
            outStream.Dispose();
        }
    }
}
