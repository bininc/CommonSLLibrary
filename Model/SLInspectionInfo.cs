using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.Core;

namespace CommonLibSL.Model
{
    /// <summary>
    /// 巡检信息
    /// </summary>
    public class SLInspectionInfo
    {
        public SLCar Car { get; set; }
        public string UNITNAME { get { return Car.Unit.UNITNAME; } }
        public string LICENSE { get { return Car.LICENSE; } }
        public string MAC { get { return Car.MAC; } }
        /// <summary>
        /// 通讯模块CSQ
        /// </summary>
        public byte CSQ { get; set; }
        /// <summary>
        /// 可见卫星数量
        /// </summary>
        public byte AllSatellite { get; set; }
        /// <summary>
        /// 可用卫星数量
        /// </summary>
        public byte UseSatellite { get; set; }
        /// <summary>
        /// 卫星信噪比（平均值）
        /// </summary>
        public byte SNR { get; set; }
        /// <summary>
        /// UART1外接设备类型
        /// </summary>
        public byte UART1Type { get; set; }
        /// <summary>
        /// UART1外接设备类型描述
        /// </summary>
        public string UART1Type_DESC {
            get
            {
                if (UART1Status == 1)
                    return GetUARTDesc(UART1Type);
                else
                    return "";
            }
        } 
        /// <summary>
        /// UART1设备状态
        /// </summary>
        public byte UART1Status { get; set; }
        /// <summary>
        /// UART1设备状态描述
        /// </summary>
        public string UART1Status_DESC {
            get
            {
                if (UART1Status == 1)
                    return GetUARTLinkState(UART1Status);
                else
                    return "";
            }
        } 
        /// <summary>
        /// UART2外接设备类型
        /// </summary>
        public byte UART2Type { get; set; }
        /// <summary>
        /// UART2外接设备类型描述
        /// </summary>
        public string UART2Type_DESC {
            get
            {
                if (UART2Status == 1)
                    return GetUARTDesc(UART2Type);
                else
                    return "";
            }
        }
        /// <summary>
        /// UART2设备状态
        /// </summary>
        public byte UART2Status { get; set; }
        /// <summary>
        /// UART2设备状态描述
        /// </summary>
        public string UART2Status_DESC {
            get
            {
                if (UART2Status == 1)
                    return GetUARTLinkState(UART2Status);
                else
                    return "";
            }
        } 
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 获得UART设备描述
        /// </summary>
        /// <param name="uartType">UART设备类型</param>
        /// <returns></returns>
        public string GetUARTDesc(byte uartType)
        {
            string uartDesc = "未知";
            switch (uartType)
            {
                case 0:
                    uartDesc = "无";
                    break;
                case 1:
                    uartDesc = "采集器S";
                    break;
                case 2:
                    uartDesc = "油量传感器";
                    break;
                case 3:
                    uartDesc = "显屏";
                    break;
                case 4:
                    uartDesc = "锁控";
                    break;
                case 5:
                    uartDesc = "OBD";
                    break;
                case 6:
                    uartDesc = "PC";
                    break;
            }
            return uartDesc;
        }

        /// <summary>
        /// 获得UART链接状态
        /// </summary>
        /// <param name="uartStatus"></param>
        /// <returns></returns>
        public string GetUARTLinkState(byte uartStatus)
        {
            string uartState = "未知";
            switch (uartStatus)
            {
                case 0:
                    uartState = "未连接";
                    break;
                case 1:
                    uartState = "已连接";
                    break;
                case 2:
                    uartState = "未知";
                    break;
            }
            return uartState;
        }
    }
}
