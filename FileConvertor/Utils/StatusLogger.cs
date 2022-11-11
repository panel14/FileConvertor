using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FileConvertor.Utils
{
    internal class StatusLogger
    {
        private TextBlock statusField;
        private ProgressBar progressBar;
        private Dictionary<StatusType, Color> statusColors = new Dictionary<StatusType, Color>()
        {
            {StatusType.SUCCESS, Colors.Green},
            {StatusType.WARNING, Colors.Orange},
            {StatusType.ERROR, Colors.Red}
        };

        public StatusLogger(TextBlock _statuField, ProgressBar _progressBar)
        {
            statusField = _statuField;
            progressBar = _progressBar;
        }

        public void UpdateStatus(StatusType type, string statusString)  
        {
            statusField.Text = string.Empty;
            statusField.Foreground = new SolidColorBrush(statusColors[type]);
            statusField.Text = statusString;
        }

        public void ClearStatus()
        {
            statusField.Text = string.Empty;
        }

        public void UpdateProgress(double percent)
        {
            progressBar.Value = percent;
        }
    }
}
