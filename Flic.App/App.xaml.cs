namespace Flic.App
{
    using System;
    using System.Linq;
    
    using Windows.ApplicationModel.Background;
    using Windows.UI.Xaml;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private const string TaskName = "BluetoothTask";

        private const string TaskEntryPoint = "Flic.Tasks.Tasks";

        private readonly Guid FlicServiceUuid = new Guid("f02adfc0-26e7-11e4-9edc-0002a5d5c51b");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.RegisterTasks();
        }

        private void RegisterTasks()
        {
            if (BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == TaskName))
            {
                return;
            }

            var backgroundTaskBuilder = new BackgroundTaskBuilder { Name = TaskName, TaskEntryPoint = TaskEntryPoint };

            var trigger = new BluetoothLEAdvertisementWatcherTrigger();
            trigger.AdvertisementFilter.Advertisement.ServiceUuids.Add(FlicServiceUuid);
            trigger.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(1000);

            backgroundTaskBuilder.SetTrigger(trigger);
            backgroundTaskBuilder.Register();
        }
    }
}
