using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorldMercatorMeterTool
{
    class Utils
    {
        public static async Task<bool> DownLoadFile(string url,string filename)
        {
            return await Task.Run<bool>(() =>
            {
                try
                {
                    var req = WebRequest.CreateHttp(url);
                    req.Method = "GET";
                    //req.Headers.Add("cache-control", "no-cache");
                    //req.Headers.Add("Token", System.Guid.NewGuid().ToString());
                    //req.Headers.Add("accept-encoding", "gzip, deflate");
                    //req.KeepAlive = true;
                    //req.Accept = "*/*";
                    //req.Host = "mt2.google.cn";
                    req.UserAgent = "Mozilla/5.0 (Windows NT 5.2; rv:12.0) Gecko/20100101 Firefox/12.0";
                    //                cache - control: no - cache
                    //Postman - Token: 370d9a28 - 57f4 - 4a66 - af5d - 92aa26a45d59
                    //User - Agent: PostmanRuntime / 6.4.1
                    //Accept: */*
                    //Host: mt2.google.cn
                    //accept-encoding: gzip, deflate
                    //Connection: keep-alive

                    var res = req.GetResponse() as HttpWebResponse;
                    var stream = res.GetResponseStream();
                    var image = Image.FromStream(stream);
                    image.Save(filename);
                    return true;
                    //var wc = new WebClient();
                    //wc.DownloadFile(url, filename);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return false;
                }

            });

            
        }
        public static Position lonLat2Mercator(Position lonLat)
        {
            Position mercator = new Position();
            double x = lonLat.X * 20037508.3427892 / 180;
            double y = Math.Log(Math.Tan((90 + lonLat.Y) * Math.PI / 360)) / (Math.PI / 180);
            y = y * 20037508.3427892 / 180;
            mercator.X = x;
            mercator.Y = y;
            return mercator;
        }

        /// <summary>
        /// Web墨卡托转经纬度
        /// </summary>
        /// <param name="x">X坐标值（单位：米）</param>
        /// <param name="y">Y坐标值（单位：米）</param>
        /// <returns>转换后的位置</returns>
        public static Position WebMercatorMeter2Degree(double x, double y)
        {
            var xValue = x / 20037508.3427892 * 180;
            var yValue = y / 20037508.3427892 * 180;
            yValue = 180 / Math.PI * (2 * Math.Atan(Math.Exp(yValue * Math.PI / 180)) - Math.PI / 2);
            var longitude = xValue;
            var latitude = yValue;
            return new Position(longitude, latitude);
        }
    }

    public class Position
    {
        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
        public Position(string[] array)
        {
            X = Convert.ToDouble(array[0]);
            Y = Convert.ToDouble(array[1]);
        }

        public Position()
        {
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Log
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();




        public static void Info(string info)
        {

            logger.Info(info);

        }

        /// <summary>
        /// DEBUG （调试信息）：记录系统用于调试的一切信息，内容或者是一些关键数据内容的输出。
        /// </summary>
        /// <param name="info"></param>
        public static void Debug(string info)
        {

            logger.Debug(info);

        }

        /// <summary>
        /// WARN（警告）：记录系统中不影响系统继续运行，但不符合系统运行正常条件，有可能引起系统错误的信息。例如，记录内容为空，数据内容不正确等。
        /// </summary>
        /// <param name="info"></param>
        public static void Warn(string info)
        {
            logger.Warn(info);
        }

        /// <summary>
        /// ERROR（一般错误）：记录系统中出现的导致系统不稳定，部分功能出现混乱或部分功能失效一类的错误。例如，数据字段为空，数据操作不可完成，操作出现异常等。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void Error(string info, Exception se)
        {
            Error(info);
            logger.Error(se);

        }

        /// <summary>
        /// 错误记录
        /// </summary>
        /// <param name="info"></param>
        public static void Error(string info)
        {
            logger.Error(info);
        }

        public static void Error(Exception info)
        {

            logger.Error(info);

        }

        public static void Trace(string info)
        {
            logger.Trace(info);
        }

        /// <summary>
        /// FATAL（致命错误）：记录系统中出现的能使用系统完全失去功能，服务停止，系统崩溃等使系统无法继续运行下去的错误。例如，数据库无法连接，系统出现死循环。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void Fatal(string info, Exception se)
        {
            logger.Fatal(se, info);
        }
    }
}
