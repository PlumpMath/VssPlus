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

        private Dictionary<string, Dictionary<string, string>> sources;
        private List<string> commands;
        private Dictionary<string,string> targets = new Dictionary<string, string>();

        private Vss vss;

        private string command = "Get";

        #endregion

        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // 历史消息记录处理
            History.Factory.Pushed += WriteHistory;

            // 设定VSS源
            this.sources = this.GetSources();
            this.CmbSource.ItemsSource = this.sources.Keys;
            this.CmbSource.SelectedIndex = 0;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Targets.json");
            if (File.Exists(path))
            {
                this.targets = path.JsonDeserialize<Dictionary<string, string>>()
                               ?? new Dictionary<string, string>();
            }

            // 设定命令
            this.commands = new List<string> { "Get", "Push", "Dir" };
            this.CmbCommand.ItemsSource = this.commands;
            this.CmbCommand.SelectedIndex = 0;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            this.targets[this.command] = this.TbTargets.Text;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Targets.json");
            this.targets.JsonSerialize(path);

            if (this.vss != null)
            {
                this.vss.Dispose();
            }
        }

        /// <summary>
        /// 历史消息记录处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteHistory(object sender, string e)
        {
            this.TbHistory.Dispatcher.InvokeAsync(
                () =>
                {
                    this.TbHistory.AppendText(e + "\r\n");
                    this.TbHistory.ScrollToEnd();
                },
                DispatcherPriority.Background
                );
        }

        #endregion

        #region Methods

        /// <summary>
        /// 重新选择VSS库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbSource_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CmbSource.SelectedIndex != -1)
            {
                if (this.vss != null)
                {
                    this.vss.Dispose();
                }

                var file = this.sources.ElementAt(this.CmbSource.SelectedIndex).Key;
                var config = this.sources[file];
                this.vss = Vss.Create(config);

                if (this.vss == null)
                {
                    this.CmbSource.SelectedIndex = -1;
                }
            }
        }

        /// <summary>
        /// 切换命令选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbCommand_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && this.CmbCommand.SelectedIndex != -1)
            {
                if (e.RemovedItems.Count > 0)
                {
                    var oldCommand = e.RemovedItems[0].ConvertTo<string>();
                    if (!oldCommand.IsNullOrWhiteSpace())
                    {
                        this.targets[oldCommand] = this.TbTargets.Text;
                    }
                }

                this.command = this.commands[this.CmbCommand.SelectedIndex];

                if (this.targets.ContainsKey(this.command))
                {
                    this.TbTargets.Text = this.targets[this.command];
                }

                switch (this.command)
                {
                case "Get":
                    this.SpGetOptions.Visibility = Visibility.Visible;
                    this.SpPushOptions.Visibility = Visibility.Hidden;
                    this.SpDirOptions.Visibility = Visibility.Hidden;

                    break;
                case "Push":
                    this.SpGetOptions.Visibility = Visibility.Hidden;
                    this.SpPushOptions.Visibility = Visibility.Visible;
                    this.SpDirOptions.Visibility = Visibility.Hidden;
                    break;
                case "Dir":
                    this.SpGetOptions.Visibility = Visibility.Hidden;
                    this.SpPushOptions.Visibility = Visibility.Hidden;
                    this.SpDirOptions.Visibility = Visibility.Visible;
                    break;
                default:

                    break;
                }
            }
        }

        /// <summary>
        /// 命令执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtCommand_OnClick(object sender, RoutedEventArgs e)
        {
            this.BtCommand.IsEnabled = false;

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
                    await this.GetExecute();
                    break;
                case "Push":
                    History.Factory.Push(string.Format("Command is not support : {0}", this.command));
                    break;
                case "Dir":
                    await this.DirExecute();
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
                this.BtCommand.IsEnabled = true;
            }
        }

        private async Task GetExecute()
        {
            if (string.IsNullOrEmpty(this.TbTargets.Text.Trim('\r', '\n', '\t', ' ')))
            {
                History.Factory.Push("Target is not set");
                return;
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
        }

        private async Task DirExecute()
        {
            if (string.IsNullOrEmpty(this.TbTargets.Text.Trim('\r', '\n', '\t', ' ')))
            {
                History.Factory.Push("Target is not set");
                return;
            }

            var config = new Dictionary<string, string>();
            config["Recursive"] = this.CbRecursiveFile.IsChecked.ToString();

            var swatch01 = Stopwatch.StartNew();

            var targets = this.GetTargets(this.TbTargets.Text);

            foreach (var target in targets)
            {
                var swatch02 = Stopwatch.StartNew();

                var result = await this.vss.DirAsync(target.Key, config);

                swatch02.Stop();

                History.Factory.Push(string.Format("Spend {0} seconds", swatch02.Elapsed.TotalSeconds));
            }

            swatch01.Stop();
            History.Factory.Push(string.Format("Total spend {0} seconds", swatch01.Elapsed.TotalSeconds));
        }

        private Dictionary<string, Dictionary<string, string>> GetSources()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"\Setting\Source.json";

            var config = path.JsonDeserialize<Dictionary<string, Dictionary<string, string>>>()
                    ?? new Dictionary<string, Dictionary<string, string>>();

            return config;
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
                    var item = values[0].Trim('\t', ' ').Replace("\"", string.Empty);
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