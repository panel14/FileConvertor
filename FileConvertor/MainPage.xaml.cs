using FileConvertor.Convetors;
using FileConvertor.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileConvertor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile source;
        private Dictionary<string, string> FORMATS = new Dictionary<string, string>()
        {
            {"MKV", ".mkv" },
            {"MPEG4", ".mp4" }
        };

        private StatusLogger logger;

        public MainPage()
        {
            InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    try
                    {
                        source = storageFile;
                        dropBox.Text += $"\nDropped file: {source.DisplayName}";
                        button.IsEnabled = true;
                        logger.UpdateStatus(StatusType.SUCCESS,"File uploaded");
                    }
                    catch
                    {
                        logger.UpdateStatus(StatusType.ERROR, "Uncorrect file format");
                    }
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var format in FORMATS)           
                destinationFormat.Items.Add(format.Key);
            destinationFormat.SelectedItem = "MPEG4";

            logger = new StatusLogger(statusField, progressBar);

            progressBar.Maximum = 100;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            StorageFile destination = await GetDestinationFile();
            if (destination == null)
            {
                logger.UpdateStatus(StatusType.WARNING, "Operation canceled.");
                return;
            }

            button.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            logger.UpdateStatus(StatusType.WARNING, "in progress: ");

            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
            VideoConverter converter = new VideoConverter(profile, logger);

            await converter.TranscodeFile(source, destination);

            //When process end
            progressBar.Value = 0;
            button.IsEnabled = true;
            progressBar.Visibility = Visibility.Collapsed;
        }

        private async Task<StorageFile> GetDestinationFile()
        {
            string choisingFormat = destinationFormat.SelectedValue.ToString();
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary,

                DefaultFileExtension = FORMATS[choisingFormat],
                SuggestedFileName = GetDestinationFileName()
            };
            savePicker.FileTypeChoices.Add(choisingFormat, new string[] { FORMATS[choisingFormat] });
            return await savePicker.PickSaveFileAsync();
        }

        public string GetDestinationFileName()
        {
            return source.DisplayName;
        }
    }
}
