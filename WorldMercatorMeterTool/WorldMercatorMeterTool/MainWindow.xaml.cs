using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;

namespace WorldMercatorMeterTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            InitEvents();
        }

        private event Action<int> ZoomEvent;

        private void InitEvents()
        {
            //注册MVVMLight消息
            Messenger.Default.Register<object>(this, "ClearMaps", ClearMaps);
            Messenger.Default.Register<Tuple<string, int, int>>(this, "ShowMaps", ShowMaps);

            //卸载当前(this)对象注册的所有MVVMLight消息
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this);
            this.Unloaded += (sender, e) => Messenger.Default.Unregister(this.DataContext);
        }

        private void ClearMaps(object e)
        {
            mainshow.Children.Clear();
        }
        private void ShowMaps(Tuple<string, int, int> message)
        {
            var image = new Image();
            image.Width = 256;
            image.Height = 256;
            image.Source = new BitmapImage(new Uri(message.Item1, UriKind.Absolute));
            var xLocation = 2 + message.Item2;
            var yLocation = 2 + message.Item3;
            image.VerticalAlignment = VerticalAlignment.Top;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.Margin = new Thickness(xLocation * 256, yLocation * 256, 0, 0);
            mainshow.Children.Add(image);
        }

        private void MainWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Messenger.Default.Send<int>(e.Delta/120, "ZoomMap");
            //ZoomEvent?.Invoke(e.Delta / 120);
        }
    }
}
