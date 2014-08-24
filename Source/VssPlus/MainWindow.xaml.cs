namespace VssPlus
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using VssPlus.Extensions;

    #endregion

    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly Dictionary<string, Dictionary<string, string>> sources;
        private readonly List<string> commands;

        private Vss vss;

        private string command = "Get";

        #endregion

        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();

            History.Factory.Pushed += WriteHistory;

            this.sources = this.GetSources();
            this.CmbSource.ItemsSource = this.sources.Keys;

            this.commands = new List<string> { "Get", "Push" };
            this.CmbCommand.ItemsSource = this.commands;
        }

        void WriteHistory(object sender, string e)
        {
            this.TbHistory.Dispatcher.InvokeAsync(
                () =>
                    this.TbHistory.AppendText(e + "\r\n"),
                DispatcherPriority.Background
                );
        }

        #endregion

        #region Methods

        private void CmbSource_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CmbSource.SelectedIndex != -1)
            {
                //var file = this.CmbSource.SelectedValue.ToString();
                var file = this.sources.ElementAt(this.CmbSource.SelectedIndex).Key;
                var config = this.sources[file];
                this.vss = Vss.Create(config);

                if (this.vss == null)
                {
                    this.CmbSource.SelectedIndex = -1;
                }
            }
        }

        private void CmbCommand_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && this.CmbCommand.SelectedIndex != -1)
            {
                this.command = this.commands[this.CmbCommand.SelectedIndex];

                if (this.CmbCommand.SelectedIndex == 0)
                {
                    this.SpGetOptions.Visibility = Visibility.Visible;
                    this.SpPushOptions.Visibility = Visibility.Hidden;
                }
                else if (this.CmbCommand.SelectedIndex == 1)
                {
                    this.SpGetOptions.Visibility = Visibility.Hidden;
                    this.SpPushOptions.Visibility = Visibility.Visible;
                }
            }
        }

        private async void BtCommand_OnClick(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                if (this.vss == null)
                {
                    History.Factory.Push("[Error]Incorrect VSS library Settings");
                    return;
                }

                switch (this.command)
                {
                case "Get":
                    if (string.IsNullOrEmpty(this.TbTargets.Text.Trim('\r','\n','\t',' ')))
                    {
                        History.Factory.Push("Target is not set");
                        break;
                    }

                    var config = new Dictionary<string, string>();
                    config["Recursive"] = this.CbRecursive.IsChecked.ToString();
                    config["Writable"] = this.CbWritable.IsChecked.ToString();
                    config["Replace"] = this.CbReplace.IsChecked.ToString();

                    var swatch01 = Stopwatch.StartNew();

                    var targets = this.GetTargets(this.TbTargets.Text);

                    foreach (var target in targets)
                    {
                        var swatch02 = Stopwatch.StartNew();

                        var result = await this.vss.GetAsync(
                            target.Key,
                            new Dictionary<string, string>(config)
                                {
                                    { "Version", target.Value }
                                }
                                               );

                        swatch02.Stop();

                        History.Factory.Push(string.Format("Spend {0} seconds", swatch02.Elapsed.TotalSeconds));
                    }

                    swatch01.Stop();
                    History.Factory.Push(string.Format("Total spend {0} seconds", swatch01.Elapsed.TotalSeconds));

                    break;
                case "Push":
                    History.Factory.Push(string.Format("Command is not support : {0}", this.command));
                    break;
                default:
                    History.Factory.Push(string.Format("Command is not support : {0}", this.command));
                    break;
                }
            }
            catch (Exception ex)
            {
                History.Factory.Push(string.Format("[Error]An unknown error occurred : {0}", ex.Message.Replace("\r\n", " ")));
            }
            finally
            {
                this.IsEnabled = true;
            }
        }

        private Dictionary<string, Dictionary<string, string>> GetSources()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + @"\Setting";
            var files = Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly);

            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var file in files)
            {
                var config = file.JsonDeserialize<Dictionary<string, string>>();
                if (config != null)
                {
                    result.Add(Path.GetFileName(file), config);
                }
            }
            return result;
        }

        private Dictionary<string, string> GetTargets(string text)
        {
            var result = new Dictionary<string, string>();

            var step01 = text.Trim('\r', '\n', '\t', ' ');
            var targets = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var target in targets)
            {
                var step02 = target.Trim('\t', ' ');
                var values = step02.Split(new[] { '\t', ',', '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Length == 1)
                {
                    var item = values[0].Trim('\t', ' ');
                    result[item] = string.Empty;
                }
                else if (values.Length >= 1)
                {
                    var item = values[0].Trim('\t', ' ');
                    var option = values[1].Trim('\t', ' ');
                    result[item] = option;
                }
            }

            return result;
        }

        #endregion

    }
}