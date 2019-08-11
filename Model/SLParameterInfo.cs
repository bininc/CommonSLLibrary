using System;

namespace CommonLibSL.Model
{
    public class SLParameterInfo
    {
        public SLCar Car { get; set; }
        /// <summary>
        /// 跟踪方式 1-定时 2-定距
        /// </summary>
        public byte TrackMode { get; set; }
        /// <summary>
        /// 跟踪方式字符串
        /// </summary>
        public string TrackModeStr
        {
            get
            {
                if (TrackMode == 1)
                    return "定时";
                else if (TrackMode == 2)
                    return "定距";
                else
                    return "-";

            }
        }
        /// <summary>
        /// 跟踪间隔 秒/米
        /// </summary>
        public short TrackInterVal { get; set; }
        /// <summary>
        /// 跟踪间隔字符串
        /// </summary>
        public string TrackInterValStr
        {
            get
            {
                if (TrackMode == 1)
                    return TrackInterVal + "秒";
                else if (TrackMode == 2)
                    return TrackInterVal + "米";
                else
                    return TrackInterVal.ToString();
            }
        }
        /// <summary>
        /// 版本信息
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        public override string ToString()
        {
            string unit = "";
            string trackMode = "";
            if (TrackMode == 1)
            {
                trackMode = "定时";
                unit = "秒";
            }
            else if (TrackMode == 2)
            {
                trackMode = "定距";
                unit = "米";
            }
            return $"跟踪方式：{trackMode}\r\n跟踪间隔：{TrackInterVal} {unit}\r\n版本信息：{Version}";
        }
    }
}
