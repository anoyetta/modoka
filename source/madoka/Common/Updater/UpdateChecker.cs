using System;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using madoka.Common.Views;

namespace madoka.Common.Updater
{
    public static class UpdateChecker
    {
        /// <summary>
        /// アップデートチェック間隔(h)の初期値
        /// </summary>
        public const double DefaultUpdateCheckInterval = 12.0d;

        public static string UpdateSourceUri { get; set; }

        public static Action<DateTimeOffset> LastUpdateCheckCallback { get; set; }

        public static ReleaseChannels LastCheckedUpdateChannel { get; private set; } = ReleaseChannels.Stable;

        public static async Task<bool> IsUpdateAsync(
            DateTimeOffset lastUpdateCheckDateTime,
            ReleaseChannels updateChannel = ReleaseChannels.Stable,
            double updateCheckInterval = DefaultUpdateCheckInterval)
        {
            // アップデートChannelを保存する
            LastCheckedUpdateChannel = updateChannel;

#if !DEBUG
            // チェック間隔にランダムなゆらぎを与える
            var rnd = new Random((int)DateTime.Now.Ticks);
            var adjust = rnd.Next(-2, 2);
            updateCheckInterval += adjust;

            if ((DateTimeOffset.Now - lastUpdateCheckDateTime).TotalHours <= updateCheckInterval)
            {
                return false;
            }
#endif

            return await IsUpdateAsync(updateChannel);
        }

        public static async Task<bool> IsUpdateAsync()
            => await IsUpdateAsync(LastCheckedUpdateChannel);

        public static async Task<bool> IsUpdateAsync(
            ReleaseChannels updateChannel)
        {
            SetupSSL();

            // アップデートChannelを保存する
            LastCheckedUpdateChannel = updateChannel;

            if (string.IsNullOrEmpty(UpdateSourceUri))
            {
                return false;
            }

            var now = DateTimeOffset.Now;
            var targetAssembly = Assembly.GetEntryAssembly();

            try
            {
                // リリースノートを取得する
                var notes = await ReleaseNotes.DeserializeAsync(new Uri(UpdateSourceUri));
                if (notes == null)
                {
                    AppLogger.Error($"Update checker error. RELEASE_NOTES.xml not found. uri={UpdateSourceUri}");
                    return false;
                }

                // より新しいバージョンがあるか？
                var newer = notes.GetNewerVersion(
                    targetAssembly,
                    updateChannel);
                if (newer == null)
                {
                    AppLogger.Write("Update checker. this version is up-to-date.");
                    return false;
                }

                AppLogger.Write($"Update checker found newer version. v{newer.Version}-{newer.ReleaseChannel}");

                // アップデートWindowを表示する
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateCheckerView.Show(
                        notes.Name,
                        targetAssembly,
                        newer);
                });
            }
            catch (Exception ex)
            {
                AppLogger.Fatal("Update checker fatal error.", ex);
                throw;
            }
            finally
            {
                LastUpdateCheckCallback?.Invoke(now);
                AppLogger.Flush();
            }

            return true;
        }

        public static void SetupSSL()
        {
            // TLS1.2を有効にする
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;

            // 自己発行証明書の警告を無視する
            ServicePointManager.ServerCertificateValidationCallback =
                OnRemoteCertificateValidationCallback;
        }

        private static bool OnRemoteCertificateValidationCallback(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors) => true;
    }
}
