using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace madoka.Common
{
    public static class WPFHelper
    {
        public delegate Task<MessageDialogResult> ShowMessageAsyncDelegate(
            string title,
            string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings settings = null);

        public delegate void EnqueueSnackMessageDelegate(
            string message,
            bool neverDuplicate = true);

        public static MetroWindow MainWindow => Application.Current?.MainWindow as MetroWindow;

        public static Dispatcher Dispatcher => Application.Current?.Dispatcher;

#if DEBUG
        private static bool isDebugMode = false;
#endif

        public static bool IsDesignMode
        {
            get
            {
#if DEBUG
                if (!isDebugMode)
                {
                    if (System.ComponentModel.LicenseManager.UsageMode ==
                        System.ComponentModel.LicenseUsageMode.Designtime)
                    {
                        isDebugMode = true;
                    }
                    else
                    {
                        using (var p = System.Diagnostics.Process.GetCurrentProcess())
                        {
                            isDebugMode =
                                p.ProcessName.Equals("DEVENV", System.StringComparison.OrdinalIgnoreCase) ||
                                p.ProcessName.Equals("XDesProc", System.StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }

                return isDebugMode;
#else
                return false;
#endif
            }
        }
    }
}
