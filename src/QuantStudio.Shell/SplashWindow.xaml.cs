using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuantStudio.Shell
{
    /// <summary>
    /// SplashWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            txtDesctiption.Content = "ShowMe"; // "QuantStudio是一个实时算法交易平台,全栈采用了微软最新的.NET Core技术， 轻量级架构,支持行情，数据，交易，回测。";
        }

        private async void SplashView_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run( () => App.Current.ShellEngine.RunDataManagerAsync() );
            await Task.CompletedTask;
        }
    }
}
