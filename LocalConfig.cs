using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Windows.Media;

namespace CommonLibSL
{
    public class LocalConfig
    {
        private IsolatedStorageSettings localSettings = IsolatedStorageSettings.ApplicationSettings;
        private string appid;

        private string saveKey
        {
            get
            {
                return appid + "saveKey";
            }
        }

        public SystemSetting SystemSet;

        public LocalConfig(string appid)
        {
            this.appid = appid;
            Refresh();
        }

        public void Refresh()
        {
            if (localSettings.Contains(saveKey) && localSettings[saveKey] != null)
                SystemSet = (SystemSetting)localSettings[saveKey];
            else
                SystemSet = new SystemSetting();
        }

        public bool Save()
        {
            localSettings[saveKey] = SystemSet;
            localSettings.Save();
            return true;
        }

        public bool Clear()
        {
            bool suc = localSettings.Remove(saveKey);
            localSettings.Save();
            return suc;
        }
    }

    public class SystemSetting
    {
        //默认显示车辆
        public bool IsDefaultShowCar = false;
        //显示轨迹线
        public bool IsShowTrackLine = false;
        //轨迹线宽度
        public int TrackLineWidth = 2;
        //轨迹线颜色
        public Color TrackLineColor = Colors.Red;
        //显示轨迹点
        public bool IsShowTrackPoint = false;
        //轨迹点宽度
        public int TrackPointWidth = 5;
        //轨迹点颜色
        public Color TrackPointColor = Colors.Blue;
        //地图
        public int Map = 0;

        //是否有报警提示音
        public bool IsAlarmSound = true;
        //是否显示报警信息
        public bool IsShowAlarm = true;

        public List<bool> PosSet = new List<bool>();
    }
}
