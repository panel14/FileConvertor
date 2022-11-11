using FileConvertor.Utils;
using System;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;

namespace FileConvertor.Convetors
{
    internal class VideoConverter
    {
        private readonly MediaEncodingProfile profile;
        private readonly StatusLogger logger;
        public VideoConverter(MediaEncodingProfile _profile, StatusLogger _logger) 
        {
            profile = _profile;
            logger = _logger;
        }

        public async Task TranscodeFile(StorageFile source, StorageFile destination)
        {
            MediaTranscoder transcoder = new MediaTranscoder();
            PrepareTranscodeResult result = await transcoder.PrepareFileTranscodeAsync(source, destination, profile);
            if (result.CanTranscode)
            {
                var transcodeOp = result.TranscodeAsync();

                transcodeOp.Progress +=
                    new AsyncActionProgressHandler<double>(TranscodeProgress);


                transcodeOp.Completed +=
                    new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);

            }
            else
            {
                switch (result.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        logger.UpdateStatus(StatusType.ERROR, "Codec not found");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        logger.UpdateStatus(StatusType.ERROR, "Invalid profile");
                        break;
                    default:
                        logger.UpdateStatus(StatusType.ERROR, "Unknown failure");
                        break;
                }
            }
        }

        private async void TranscodeProgress(IAsyncActionWithProgress<double> asyncInfo, double percent)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 new DispatchedHandler(() =>
                 {
                    logger.UpdateProgress(percent);
                 }));
        }

        private async void TranscodeComplete(IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    asyncInfo.GetResults();
                    switch (asyncInfo.Status)
                    {
                        case AsyncStatus.Completed:
                            logger.UpdateStatus(StatusType.SUCCESS, "Operation completed");
                            break;
                        case AsyncStatus.Canceled:
                            logger.UpdateStatus(StatusType.WARNING, "Operation canceled");
                            break;
                        default:
                            logger.UpdateStatus(StatusType.SUCCESS, "Operation error");
                            break;
                    }
                }));
        }
    }
}
