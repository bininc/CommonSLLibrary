using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using CommLiby;
using Microsoft.Maps.MapControl;
using Microsoft.Maps.MapControl.Core;

namespace CommonLibSL.Map
{

    #region 地图TitleSources

    public class TileSources
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="roadTileSource"></param>
        /// <param name="aerialTileSource"></param>
        public TileSources(TileSource roadTileSource, params TileSource[] aerialTileSource)
        {
            RoadTileSource = roadTileSource;
            AerialTileSource = aerialTileSource;
        }

        /// <summary>
        /// 一般模式
        /// </summary>
        public TileSource RoadTileSource { get; set; }
        /// <summary>
        /// 卫星模式
        /// </summary>
        public TileSource[] AerialTileSource { get; set; }
    }

    #region 谷歌地图
    public class GoogleTileSource : MapTileSource
    {
        public GoogleTileSource()
            : base("{UriScheme}://mt{0}.google.cn/maps/vt?lyrs=m@271000000&hl=zh-Hans-CN&gl=CN&&x={1}&y={2}&z={3}")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            return new Uri(string.Format(this.UriFormat, x % 4, x, y, zoomLevel));
        }
    }
    public class GoogleAerialTileSource : MapTileSource
    {
        public GoogleAerialTileSource()
            : base("{UriScheme}://mt{0}.google.cn/maps/vt?lyrs=s@271000000&hl=zh-Hans-CN&gl=CN&&x={1}&y={2}&z={3}")
        { }
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            string url = string.Format(this.UriFormat, x % 4, x, y, zoomLevel);
            return new Uri(url);
        }
    }
    public class GoogleAerialRoadTileSource : MapTileSource
    {
        public GoogleAerialRoadTileSource()
            : base("{UriScheme}://mt{0}.google.cn/maps/vt?lyrs=h@271000000&hl=zh-Hans-CN&gl=CN&&x={1}&y={2}&z={3}")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            string url = string.Format(this.UriFormat, x % 4, x, y, zoomLevel);
            return new Uri(url);
        }
    }
    #endregion

    #region QQ地图 已关闭
    public class QQTileSource : TileSource
    {
        public QQTileSource()
            : base(@"https://p{0}.map.soso.com/maptilesv2/{1}/{2}/{3}/{4}_{5}.png")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            string str = string.Format(this.UriFormat, x % 4, zoomLevel, Math.Floor(x / 16.0), Math.Floor(y / 16.0), x, y);
            return new Uri(str);
        }
    }
    public class QQAerialTileSource : TileSource
    {
        public QQAerialTileSource()
            : base(@"https://p{0}.map.soso.com/sateTiles/{1}/{2}/{3}/{4}_{5}.jpg")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            string str = string.Format(this.UriFormat, x % 4, zoomLevel, Math.Floor(x / 16.0), Math.Floor(y / 16.0), x, y);
            return new Uri(str);
        }
    }
    public class QQAerialRoadTileSource : TileSource
    {
        public QQAerialRoadTileSource()
            : base(@"https://p{0}.map.soso.com/sateTranTiles/{1}/{2}/{3}/{4}_{5}.png")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            string str = string.Format(this.UriFormat, x % 4, zoomLevel, Math.Floor(x / 16.0), Math.Floor(y / 16.0), x, y);
            return new Uri(str);
        }
    }
    #endregion

    #region 腾讯地图 2019-06-20 更新url
    public class TencentTileSource : MapTileSource
    {
        public TencentTileSource()
            : base("{UriScheme}://rt{0}.map.gtimg.com/tile?z={3}&x={1}&y={2}&scene=0&version=349&styleid=1000")
        { }
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            return new Uri(string.Format(UriFormat, x % 4, x, y, zoomLevel));
        }
    }
    public class TencentAerialTileSource : MapTileSource
    {
        public TencentAerialTileSource()
            : base(@"{UriScheme}://p{0}.map.gtimg.com/sateTiles/{1}/{2}/{3}/{4}_{5}.jpg?version=230")
        { }
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            string str = string.Format(this.UriFormat, x % 4, zoomLevel, Math.Floor(x / 16.0), Math.Floor(y / 16.0), x, y);
            return new Uri(str);
        }
    }
    public class TencentAerialRoadTileSource : MapTileSource
    {
        public TencentAerialRoadTileSource()
            : base("{UriScheme}://rt{0}.map.gtimg.com/tile?z={3}&x={1}&y={2}&styleid=2&version=349")
        { }
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            y = int.Parse(Math.Pow(2, zoomLevel).ToString()) - 1 - y;
            return new Uri(string.Format(UriFormat, x % 4, x, y, zoomLevel));
        }
    }
    #endregion

    #region 必应地图
    public class BingTileSource : LocationRectTileSource
    {
        public BingTileSource()
        // : base("http://r2.tiles.ditu.live.com/tiles/r{quadkey}.png?g=87", new LocationRect(new Location(60, 60), new Location(13, 140)), new Range<double>(1, 16))
        {
            //设定瓦片源Tile系统的Uri格式，其中的{quadkey}就是每个瓦片quadkey的对应位置
            //这里使用的是必应地图（简体中文）的Tile系统
            UriFormat = "{UriScheme}://r1.tiles.ditu.live.com/tiles/r{quadkey}.png?g=2019&mkt=zh-cn";
        }
    }
    #endregion

    #region 高德地图
    public class AMapTileSource : MapTileSource
    {
        public AMapTileSource()
            : base(@"{UriScheme}://wprd0{0}.is.autonavi.com/appmaptile?lang=zh_cn&size=1&style=7&x={1}&y={2}&z={3}&scl=1&ltype=7")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, rd.Next(1, 4), x, y, zoomLevel);
            return new Uri(url);
        }
    }
    public class AMapAerialTileSource : MapTileSource
    {
        public AMapAerialTileSource()
            : base(@"{UriScheme}://webst0{0}.is.autonavi.com/appmaptile?style=6&x={1}&y={2}&z={3}")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, rd.Next(1, 4), x, y, zoomLevel);
            return new Uri(url);
        }
    }
    public class AMapAerialRoadTileSource : MapTileSource
    {
        public AMapAerialRoadTileSource()
            : base(@"{UriScheme}://wprd0{0}.is.autonavi.com/appmaptile?x={1}&y={2}&z={3}&lang=zh_cn&size=1&scl=1&style=8&ltype=7")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, rd.Next(1, 4), x, y, zoomLevel);
            return new Uri(url);
        }
    }
    #endregion

    #region OpenStreet
    public class OpenStreetTileSource : MapTileSource
    {
        public OpenStreetTileSource()
            : base(@"{UriScheme}://{3}.tile.openstreetmap.org/{2}/{0}/{1}.png")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, x, y, zoomLevel, servers[rd.Next(0, 2)]);
            return new Uri(url);
        }

        private static readonly string[] servers = new[] { "a", "b", "c" };
    }
    #endregion

    #region CycleMap
    public class OpenCycleTileSource : MapTileSource
    {
        public OpenCycleTileSource()
            : base(@"{UriScheme}://{3}.tile.thunderforest.com/cycle/{2}/{0}/{1}.png?apikey=a5dd6a2f1c934394bce6b0fb077203eb")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, x, y, zoomLevel, servers[rd.Next(0, 2)]);
            return new Uri(url);
        }

        private static readonly string[] servers = new[] { "a", "b", "c" };
    }
    #endregion

    #region 搜狗地图 未完成
    public class SogouTileSource : MapTileSource
    {
        public SogouTileSource()
            : base(@"{UriScheme}://p{0}.map.sogou.com/seamless1/0/174/717/3/1/795_254.png?v=2019625")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, rd.Next(0, 3), x, y, zoomLevel);
            return new Uri(url);
        }
        /*
        public string GetURL(int x, int y, int zoomLevel)
        {
            var tilez = zoomLevel - 1;
            var offsetX = Math.pow(2, tilez);
            var offsetY = offsetX - 1;
            var res = this.map.getResolution();
            var bbox = this.map.getMaxExtent();
            var size = this.tileSize;
            var bx = Math.round((bounds.left - bbox.left) / (res * size.w));
            var by = Math.round((bbox.top - bounds.top) / (res * size.h));
            var numX = bx - offsetX;
            var numY = (-by) + offsetY;
            tilez = tilez + 1;
            var zoomLevel = 729 - tilez;
            if (zoomLevel == 710) zoomLevel = 792;
            var blo = Math.floor(numX / 200);
            var bla = Math.floor(numY / 200);
            var blos, blas;
            if (blo < 0)
                blos = "M" + (-blo);
            else
                blos = "" + blo;
            if (bla < 0)
                blas = "M" + (-bla);
            else
                blas = "" + bla;
            var x = numX.toString().replace("-", "M");
            var y = numY.toString().replace("-", "M");
            var urlsNum = parseInt((bx + by) % this.url.length);
            var strURL = "";
            strURL = this.url[urlsNum] + zoomLevel + "/" + blos + "/" + blas + "/" + x + "_" + y + ".GIF";
            return strURL;
        }*/

    }
    #endregion

    #region 百度地图 错误的
    public class BaiduTileSource : TileSource
    {
        public BaiduTileSource()
            //: base(@"http://online{0}.map.bdimg.com/onlinelabel/?qt=tile&x={1}&y={2}&z={3}&styles=pl&udt=20160429&scaler=1&p=0")
            : base("http://online{0}.map.bdimg.com/onlinelabel/?qt=tile&x={1}&y={2}&z={3}&styles=pl&udt=20171214&scaler=1&p=1")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            GPSTool.GoogleToBaiDuTile(ref x, ref y, zoomLevel);
            string nx = x + "";
            nx = Regex.Replace(nx, "-", "M");
            string ny = y + "";
            ny = Regex.Replace(ny, "-", "M");
            string url = string.Format(UriFormat, x % 9, nx, ny, zoomLevel);
            return new Uri(url);
        }
    }
    public class BaiduAerialTileSource : TileSource
    {
        public BaiduAerialTileSource()
            : base(@"http://webst0{0}.is.autonavi.com/appmaptile?style=6&x={2}&y={3}&z={1}")
        { }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            Random rd = new Random();
            string url = string.Format(this.UriFormat, rd.Next(1, 4), zoomLevel, x, y);
            return new Uri(url);
        }
    }
    #endregion

    #region 本地地图
    public class LocalGoogleTileSource : TileSource
    {
        public static string MapUrl { get; set; }
        public LocalGoogleTileSource()
        { }
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            if (MapUrl == null)
                return null;
            string url = string.Format(MapUrl, zoomLevel, y, x);
            return new Uri(url);
        }
    }
    #endregion

    #endregion

    #region 地图模式
    /// <summary>
    /// 地图模式枚举
    /// </summary>
    public enum MapModes
    {
        [Description("高德地图")]
        AMapMode,
        [Description("腾讯地图")]
        TencentMapMode,
        //[Description("QQ地图")]
        //QQMapMode,
        [Description("谷歌地图")]
        GoogleMapMode,
        [Description("OpenStreet")]
        OpenstreetMapMode,
        [Description("OpenCycle")]
        OpencycleMapMode,
        [Description("必应地图")]
        BingMapMode,
        [Description("本地地图")]
        LocalGoogleMapMode,
        //[Description("百度地图")]
        //BaiduMapMode
    }

    public static class SLMapMode
    {
        /// <summary>
        /// 中国中心经纬度
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Location ChinaLocation(this Location location)
        {
            double la = 38.708333333333332;//28.708333333333332;
            double lo = 104.35416666666667;
            if (location == null)
            {
                location = new Location(la, lo);
            }
            else
            {
                location.Latitude = la;
                location.Longitude = lo;
            }
            return location;
        }       
    }

    public class GoogleMapMode : CustomMode
    {
        public GoogleMapMode()
        {
            CurrentTileSources = new TileSources(new GoogleTileSource(), new GoogleAerialTileSource(), new GoogleAerialRoadTileSource());
            MapAdjust = new MapAdjust(GeoType.GCJ02);
        }
    }

    public class QQMapMode : CustomMode
    {
        public QQMapMode()
        {
            CurrentTileSources = new TileSources(new QQTileSource(), new QQAerialTileSource(), new QQAerialRoadTileSource());
            MapZoomRange = new Range<double>(1, 18);
            MapAdjust = new MapAdjust(GeoType.GCJ02);
        }
    }

    public class TencentMapMode : CustomMode
    {
        public TencentMapMode()
        {
            CurrentTileSources = new TileSources(new TencentTileSource(), new TencentAerialTileSource(), new TencentAerialRoadTileSource());
            MapZoomRange = new Range<double>(4, 18);
            MapAdjust = new MapAdjust(GeoType.GCJ02);
        }
    }

    public class BingMapMode : CustomMode
    {
        public BingMapMode()
        {
            CurrentTileSources = new TileSources(new BingTileSource());
            MapZoomRange = new Range<double>(1, 18);
            MapAdjust = new MapAdjust(GeoType.GCJ02);
        }
    }

    public class AMapMode : CustomMode
    {
        public AMapMode()
        {
            CurrentTileSources = new TileSources(new AMapTileSource(), new AMapAerialTileSource(), new AMapAerialRoadTileSource());
            MapZoomRange = new Range<double>(3, 20);
            AerialZoomRange = new Range<double>(3, 18);
            MapAdjust = new MapAdjust(GeoType.GCJ02);
        }
    }

    public class OpenstreetMapMode : CustomMode
    {
        public OpenstreetMapMode()
        {
            CurrentTileSources = new TileSources(new OpenStreetTileSource());
            MapZoomRange = new Range<double>(1, 19);
            MapAdjust = new MapAdjust(GeoType.WGS84);
        }
    }

    public class OpencycleMapMode : CustomMode
    {
        public OpencycleMapMode()
        {
            CurrentTileSources = new TileSources(new OpenCycleTileSource());
            MapAdjust = new MapAdjust(GeoType.WGS84);
        }
    }

    public class BaiduMapMode : CustomMode
    {
        public BaiduMapMode()
        {
            CurrentTileSources = new TileSources(new BaiduTileSource(), new BaiduAerialTileSource());
            MapZoomRange = new Range<double>(3, 18);
            MapAdjust = new MapAdjust(GeoType.BD09);
        }
    }

    public class LocalGoogleMapMode : CustomMode
    {
        public LocalGoogleMapMode()
        {
            CurrentTileSources = new TileSources(new LocalGoogleTileSource());
            MapZoomRange = new Range<double>(5, 17);
            LongitudeRange = new Range<double>(110, 130);
            LatitudeRange = new Range<double>(35, 43);
        }
    }

    /// <summary>
    /// 自定义地图基类，实现了视野范围的限制和自定义瓦片源。
    /// 使用前请初始化：
    /// TileLayer（瓦片源）
    /// LatitudeRange（经度范围）
    /// LongitudeRange（纬度范围）
    /// MapZoomRange（缩放范围）
    /// </summary>
    public class CustomMode : MercatorMode
    {
        /// <summary>
        /// 用于呈现Tile层
        /// </summary>
        public override UIElement Content
        {
            get
            {
                return this.TileLayer;
            }
        }
        /// <summary>
        /// 存储Tile图层
        /// </summary>
        public MapTileLayer TileLayer;
        /// <summary>
        /// 纬度范围
        /// </summary>
        public Range<double> LatitudeRange;
        /// <summary>
        /// 经度范围
        /// </summary>
        public Range<double> LongitudeRange;
        /// <summary>
        /// 缩放范围
        /// </summary>
        public Range<double> MapZoomRange;
        /// <summary>
        /// 卫星模式缩放范围
        /// </summary>
        public Range<double> AerialZoomRange;
        /// <summary>
        /// 当前的tilesources
        /// </summary>
        public TileSources CurrentTileSources { get; protected set; }

        /// <summary>
        /// 是否是卫星图模式
        /// </summary>
        private bool isAerialMode;

        private MapAdjust _mapAdjust = new MapAdjust(GeoType.WGS84);    //默认需要矫正

        /// <summary>
        /// 是否需要偏移矫正
        /// </summary>
        public MapAdjust MapAdjust
        {
            get { return _mapAdjust; }
            set { _mapAdjust = value; }
        }

        /// <summary>
        /// 是否是卫星模式
        /// </summary>
        public bool IsAerialMode
        {
            get
            {
                return isAerialMode;
            }

            set
            {
                if (isAerialMode == value) return;

                isAerialMode = value;
                if (isAerialMode)
                {
                    Range<double> range = GetZoomRange(null);
                    if (range.To < ZoomLevel)
                        ZoomLevel = range.To;
                }
                UpdateView();
            }
        }

        /// <summary>
        /// 初始化基类字段
        /// </summary>
        public CustomMode()
        {
            this.TileLayer = new MapTileLayer();
            this.LatitudeRange = new Range<double>(-90, 90);
            this.LongitudeRange = new Range<double>(-180, 180);
            this.MapZoomRange = new Range<double>(1, 20);
        }
        /// <summary>
        /// 缩放范围
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        protected override Range<double> GetZoomRange(Location center)
        {
            if (isAerialMode && AerialZoomRange != null)
            {
                return AerialZoomRange;
            }

            return MapZoomRange;
        }
        //当地图视野改变时将调用该函数进行处理（即可达到限制地图范围的效果）
        public override bool ConstrainView(Location center, ref double zoomLevel, ref double heading, ref double pitch)
        {
            bool isChanged = base.ConstrainView(center, ref zoomLevel, ref heading, ref pitch);

            double newLatitude = center.Latitude;
            double newLongitude = center.Longitude;

            //如果视野纬度超出范围，则将视野范围限制在边界
            if (center.Longitude > LongitudeRange.To)
            {
                newLongitude = LongitudeRange.To;
            }
            else if (center.Longitude < LongitudeRange.From)
            {
                newLongitude = LongitudeRange.From;
            }

            //如果视野经度超出范围，则将视野范围限制在边界
            if (center.Latitude > LatitudeRange.To)
            {
                newLatitude = LatitudeRange.To;
            }
            else if (center.Latitude < LatitudeRange.From)
            {
                newLatitude = LatitudeRange.From;
            }

            //设置新的地图视野（限制在范围中）
            if (newLatitude != center.Latitude || newLongitude != center.Longitude)
            {
                center.Latitude = newLatitude;
                center.Longitude = newLongitude;
                isChanged = true;
            }

            //设置新的地图缩放级别（限制在范围中）
            Range<double> range = GetZoomRange(center);
            if (zoomLevel > range.To)
            {
                zoomLevel = range.To;
                isChanged = true;
            }
            else if (zoomLevel < range.From)
            {
                zoomLevel = range.From;
                isChanged = true;
            }

            return isChanged;
        }


        public override void Activated(MapLayerBase modeLayer, MapLayerBase modeForegroundLayer)
        {
            if (TileLayer.TileSources.Count == 0)
                UpdateView();
            base.Activated(modeLayer, modeForegroundLayer);
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        public void UpdateView()
        {
            if (CurrentTileSources != null)
            {
                if (isAerialMode)
                {
                    if (CurrentTileSources.AerialTileSource != null && CurrentTileSources.AerialTileSource.Length > 0)
                    {
                        TileLayer.TileSources.Clear();
                        foreach (TileSource tileSource in CurrentTileSources.AerialTileSource)
                        {
                            TileLayer.TileSources.Add(tileSource);
                        }
                        return;
                    }
                }

                TileLayer.TileSources.Clear();
                TileLayer.TileSources.Add(CurrentTileSources.RoadTileSource);
            }
        }
    }
    #endregion
}
