using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace WorldMercatorMeterTool.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        Position Center;
        private double[] Bounds = new double[] { -20037508.3427892, -20037508.3427892, 20037508.3427892, 20037508.3427892 };
        private string mapUrl = "http://mt2.google.cn/vt/lyrs=m@167000000&hl=zh-CN&gl=cn&x={x}&y={y}&z={z}&s=Galil";

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<int>(this, "ZoomMap", ZoomMap);
            //Messenger.Default.Send<Action<int>>(ZoomMap, "RegistZoomEvent");
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            ///
        }

        //31.2993590614,117.9931369140
        //27.1373852884,117.9931369140
        //31.2993590614,122.8051774815
        //27.1373852884,122.8051774815

        private object lockobj = new object();

        private double _centerlongitude = 120;
        private double _centerlatitude = 30;
        private int _Level = 4;
        private string _ConvertResult;
        private string _MappictureResult;
        private int _downloadmin = 3;
        private int _downloadmax = 18;
        private string _leftTop = "117.9931369140,31.2993590614";
        private string _leftBottom = "117.9931369140,27.1373852884";
        private string _rightTop = "122.8051774815,31.2993590614";
        private string _rightBottom = "122.8051774815,27.1373852884";
        private int _DownLoadCount = 0;
        private int _DownLoadErrorCount = 0;
        private int _DownLoadedCount = 0;
        private bool isshowing = false;

        private RelayCommand _convertcommand;
        private RelayCommand _CenterShowComand;
        private RelayCommand _DownLoadCommand;

        /// <summary>
        /// 中心点经度
        /// </summary>
        public double Centerlongitude
        {
            get { return _centerlongitude; }
            set
            {
                Set(ref _centerlongitude, value);
            }
        }


        /// <summary>
        /// 中心点纬度
        /// </summary>
        public double Centerlatitude
        {
            get { return _centerlatitude; }
            set { Set(ref _centerlatitude, value); }
        }

        /// <summary>
        /// 级别
        /// </summary>
        public int Level
        {
            get { return _Level; }
            set { Set(ref _Level, value); }
        }

        /// <summary>
        /// 转换结果
        /// </summary>
        public string ConvertResult
        {
            get { return _ConvertResult; }
            set { Set(ref _ConvertResult, value); }
        }

        public string MappictureResult
        {
            get { return _MappictureResult; }
            set { Set(ref _MappictureResult, value); }
        }


        public int Downloadmin
        {
            get { return _downloadmin; }
            set { Set(ref _downloadmin, value); }
        }

        public int Downloadmax
        {
            get { return _downloadmax; }
            set { Set(ref _downloadmax, value); }
        }


        public string LeftTop
        {
            get { return _leftTop; }
            set { Set(ref _leftTop, value); }
        }

        public string LeftBottom
        {
            get { return _leftBottom; }
            set { Set(ref _leftBottom, value); }
        }

        public string RightTop
        {
            get { return _rightTop; }
            set { Set(ref _rightTop, value); }
        }

        public string RightBottom
        {
            get { return _rightBottom; }
            set { Set(ref _rightBottom, value); }
        }

        public int DownLoadCount
        {
            get { return _DownLoadCount; }
            set { Set(ref _DownLoadCount, value); }
        }

        public int DownLoadErrorCount
        {
            get { return _DownLoadErrorCount; }
            set { Set(ref _DownLoadErrorCount, value); }
        }

        public int DownLoadedCount
        {
            get { return _DownLoadedCount; }
            set { Set(ref _DownLoadedCount, value); }
        }
        #region Command without parameters


        public RelayCommand DownLoadCommand
        {
            get
            {
                if (_DownLoadCommand == null)
                    _DownLoadCommand = new RelayCommand(DownLoad);
                return _DownLoadCommand;
            }
        }



        private void DownLoad()
        {
            DownLoadCount = 0;
            DownLoadErrorCount = 0;
            DownLoadedCount = 0;
            Task.Run(() =>
            {
                try
                {
                    for (var zoomindex = Downloadmin; zoomindex <= Downloadmax; zoomindex++)
                    {
                        _Level = zoomindex;
                        var lefttopPosition = GetPicLocation(new Position(LeftTop.Split(',')), true);
                        var leftbottomPosition = GetPicLocation(new Position(LeftBottom.Split(',')), true);
                        var righttopPosition = GetPicLocation(new Position(RightTop.Split(',')), true);
                        var rightbottomPosition = GetPicLocation(new Position(RightBottom.Split(',')), true);

                        var xCount = righttopPosition.Item1 - lefttopPosition.Item1;
                        var yCount = leftbottomPosition.Item2 - lefttopPosition.Item2;
                        DownLoadCount += xCount * yCount == 0 ? 1 : xCount * yCount;
                        if (xCount * yCount == 0)
                        {
                            xCount = 1;
                            yCount = 1;
                        }

                        //new ParallelOptions() { MaxDegreeOfParallelism = 8 }
                        Parallel.For(0, xCount,(i) =>
                        {
                            Parallel.For(0, yCount, async (index) =>
                            {
                                var filename = string.Empty;
                                var imageurl = GetUrl(_Level, lefttopPosition.Item1 + i, lefttopPosition.Item2 + index,
                                    out filename);
                                var result = true;
                                if (!System.IO.File.Exists(filename))
                                {
                                    result = await Utils.DownLoadFile(imageurl, filename);
                                }

                                lock (lockobj)
                                {
                                    if (result)
                                    {
                                        DownLoadedCount += 1;
                                    }
                                    else
                                    {
                                        DownLoadErrorCount += 1;
                                    }
                                }


                            });
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            });

        }

        public RelayCommand ConvertCommand
        {
            get
            {
                if (_convertcommand == null)
                    _convertcommand = new RelayCommand(Convert);
                return _convertcommand;
            }
        }



        private void Convert()
        {
            Center = Utils.lonLat2Mercator(new Position(Centerlongitude, Centerlatitude));
            ConvertResult = $"X:{Center.X},Y:{Center.Y}";
        }


        public RelayCommand CenterShowComand
        {
            get
            {
                if (_CenterShowComand == null)
                    _CenterShowComand = new RelayCommand(CenterShowAsync);
                return _CenterShowComand;
            }
        }



        private Tuple<int, int> GetPicLocation(Position position,bool convert = false)
        {
            if (convert)
            {
                position = Utils.lonLat2Mercator(new Position(position.X, position.Y));
            }
            var pictureSize = Math.Pow(4, _Level);
            var sizecount = ((Bounds[2] - Bounds[0]) / (pictureSize / Math.Pow(2, _Level)));
            var x = System.Convert.ToInt32(Math.Floor((position.X - Bounds[0]) / sizecount));
            var y = System.Convert.ToInt32(Math.Floor(Math.Pow(2, _Level) - (position.Y - Bounds[1]) / sizecount));

            return new Tuple<int, int>(x, y);
        }

        private async void CenterShowAsync()
        {
            if (!isshowing)
            {
                try
                {
                    isshowing = true;
                    var picposition = GetPicLocation(Center);
                    var x = picposition.Item1;
                    var y = picposition.Item2;

                    MappictureResult = $"X:{x},Y:{y}";

                    string filename;

                    Messenger.Default.Send(this, "ClearMaps");

                    for (var i = -2; i <= 2; i++)
                    {
                        for (var j = -2; j <= 2; j++)
                        {
                            var imageurl = GetUrl(_Level, x + i, y + j, out filename);
                            if (!System.IO.File.Exists(filename))
                            {
                                await Utils.DownLoadFile(imageurl, filename);
                            }

                            Messenger.Default.Send(new Tuple<string, int, int>(filename, i, j), "ShowMaps");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }

                isshowing = false;
            }
        }
        #endregion




        private string GetUrl(int level, object x, object y, out string filanme)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + $"MapData\\{level}\\{x}";
            System.IO.Directory.CreateDirectory(dir);
            filanme = $"{dir}\\{y}.png";
            return mapUrl.Replace("{x}", x.ToString()).Replace("{y}", y.ToString()).Replace("{z}", level.ToString());
        }

        public void ZoomMap(int value)
        {
            if (!isshowing)
            {
                var newlevel = _Level + value;
                if (newlevel < 18 && newlevel > 3)
                {
                    Level = newlevel;
                    CenterShowAsync();
                }
            }
        }
    }
}