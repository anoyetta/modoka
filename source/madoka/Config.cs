using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using madoka.Common;
using madoka.Models;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace madoka
{
    public partial class Config : JsonConfigBase
    {
        #region Singleton

        private static Config instance;

        public static Config Instance => instance ?? (instance = Config.Load<Config>(FileName));

        public Config()
        {
        }

        #endregion Singleton

        public static string FileName => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "madoka.config.json");

        public void Save() => this.Save(FileName);

        #region Data

        [JsonIgnore]
        public string AppName => Assembly.GetExecutingAssembly().GetTitle();

        [JsonIgnore]
        public string AppNameWithVersion => $"{this.AppName} - {this.AppVersionString}";

        [JsonIgnore]
        public Version AppVersion => Assembly.GetExecutingAssembly().GetVersion();

        [JsonIgnore]
        public string AppVersionString => $"v{this.AppVersion.ToString()}";

        private double processScaningInterval = 5.0;

        [JsonProperty(PropertyName = "process_scaning_interval")]
        public double ProcessScaningInterval
        {
            get => this.processScaningInterval;
            set => this.SetProperty(ref this.processScaningInterval, Math.Round(value, 1));
        }

        private double scale = 1.0;

        [JsonProperty(PropertyName = "scale")]
        public double Scale
        {
            get => this.scale;
            set => this.SetProperty(ref this.scale, Math.Round(value, 2));
        }

        private double mainWindowX;

        [JsonProperty(PropertyName = "mainwindow_x")]
        public double MainWindowX
        {
            get => this.mainWindowX;
            set
            {
                if (MainWindow.Instance != null &&
                    MainWindow.Instance.WindowState != WindowState.Normal)
                {
                    return;
                }

                this.SetProperty(ref this.mainWindowX, value);
            }
        }

        private double mainWindowY;

        [JsonProperty(PropertyName = "mainwindow_y")]
        public double MainWindowY
        {
            get => this.mainWindowY;
            set
            {
                if (MainWindow.Instance != null &&
                    MainWindow.Instance.WindowState != WindowState.Normal)
                {
                    return;
                }

                this.SetProperty(ref this.mainWindowY, value);
            }
        }

        private double mainWindowW;

        [JsonProperty(PropertyName = "mainwindow_w")]
        public double MainWindowW
        {
            get => this.mainWindowW;
            set
            {
                if (MainWindow.Instance != null &&
                    MainWindow.Instance.WindowState != WindowState.Normal)
                {
                    return;
                }

                this.SetProperty(ref this.mainWindowW, value);
            }
        }

        private double mainWindowH;

        [JsonProperty(PropertyName = "mainwindow_h")]
        public double MainWindowH
        {
            get => this.mainWindowH;
            set
            {
                if (MainWindow.Instance != null &&
                    MainWindow.Instance.WindowState != WindowState.Normal)
                {
                    return;
                }

                this.SetProperty(ref this.mainWindowH, value);
            }
        }

        private bool isStartupWithWindows;

        [JsonProperty(PropertyName = "is_startup_with_windows")]
        public bool IsStartupWithWindows
        {
            get => this.isStartupWithWindows;
            set
            {
                if (this.SetProperty(ref this.isStartupWithWindows, value))
                {
                    this.SetStartup(value);
                }
            }
        }

        public async void SetStartup(
            bool isStartup) =>
            await Task.Run(() =>
            {
                using (var regkey = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    true))
                {
                    if (isStartup)
                    {
                        regkey.SetValue(
                            Assembly.GetExecutingAssembly().GetProduct(),
                            $"\"{Assembly.GetExecutingAssembly().Location}\"");
                    }
                    else
                    {
                        regkey.DeleteValue(
                            Assembly.GetExecutingAssembly().GetProduct(),
                            false);
                    }
                }
            });

        private bool isMinimizeStartup;

        [JsonProperty(PropertyName = "is_minimize_startup")]
        public bool IsMinimizeStartup
        {
            get => this.isMinimizeStartup;
            set => this.SetProperty(ref this.isMinimizeStartup, value);
        }

        private ObservableCollection<ManagedWindowModel> managedWindowList = new ObservableCollection<ManagedWindowModel>();

        [JsonProperty(PropertyName = "managed_windows")]
        public ObservableCollection<ManagedWindowModel> ManagedWindowList
        {
            get => this.managedWindowList;
            set
            {
                this.managedWindowList.Clear();
                this.managedWindowList.AddRange(value);
            }
        }

        #endregion Data
    }
}
