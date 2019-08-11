using System;
using System.Linq.Expressions;
using System.Windows.Browser;
using CommLiby;
using CommLiby.Cyhk;
using CommLiby.Cyhk.Models;
using Models;

namespace CommonLibSL.Model
{
    public enum AlarmType
    {
        /// <summary>
        /// 常规报警
        /// </summary>
        Normal,
        /// <summary>
        /// 锁控报警
        /// </summary>
        Lock
    }

    public delegate void NewAlarmEventHandler(AlarmType alarmType, SLCar car);
    /// <summary>
    /// 车辆信息Silverlight版本
    /// </summary>
    public class SLCar : CARS
    {
        #region 事件
        /// <summary>
        /// 新报警事件
        /// </summary>
        public event NewAlarmEventHandler HaveNewAlarm;

        public void OnHaveNewAlarm(AlarmType alarmType)
        {
            if (HaveNewAlarm != null)
            {
                if (string.IsNullOrWhiteSpace(MAC)) return;
                HaveNewAlarm(alarmType, Copy());
            }
        }
        #endregion

        #region 字段
        private NEWTRACK _lastGpsData;
        private CarState _carState;
        private AlarmStatus _alarmStatus;
        private CarStatus _carStatus;
        private string _stateIcon = "/ImageResource;component/Images/car/stateNoGps.png";
        private SLLockGpsData _lastLockGpsData;
        private SLLcvAlarm _lockAlarm;
        private bool _isCopy;
        private string _passenger;
        private string _passengerPhone;
        private DateTime _beginTime;
        private DateTime _endTime;
        private int _state;
        private string _beginAddress;
        private string _endAddress;
        private string _carTypeStr;
        private string _driverName;
        private string _driverPhone;

        public SLCar()
        {
            CarState = CarState.OffLine;
            //LastLockGpsData = new SLLockGpsData();
        }
        #endregion

        #region 属性

        public bool IsCopy
        {
            get { return _isCopy; }
        }

        /// <summary>
        /// 最后一条GPS数据
        /// </summary>
        public NEWTRACK LastGpsData
        {
            get { return _lastGpsData; }
            set
            {
                _lastGpsData = value;
                if (value == null) return;

                RefreshState(value, null, true);
                if (!_isCopy)
                {
                    value.PropertyChanged += (x, y) =>
                    {
                        if (_isCopy)
                        {
                            return;
                        }

                        NEWTRACK gps = (NEWTRACK)x;
                        if (RefreshState(gps, y.PropertyName))
                        {
                            if (!CarStatus.blindarea_status)    //盲区补偿状态不刷新
                            {
                                OnPropertyChanged(nameof(LastGpsData), true);
                                OnPropertyChanged(nameof(LastGpsData) + "." + y.PropertyName);
                            }
                        }
                    };
                }
                else
                {
                    OnPropertyChanged(nameof(LastGpsData) + "." + nameof(NEWTRACK));
                }
                OnPropertyChanged(nameof(LastGpsData), true);
            }
        }

        /// <summary>
        /// 刷新位置数据
        /// </summary>
        public void RefreshPosition(string position = null)
        {
            if (position != null)
            {
                LastGpsData.POSITION = position;
            }
            OnPropertyChanged(nameof(LastGpsData), true);
            OnPropertyChanged(nameof(LastGpsData) + "." + nameof(NEWTRACK));
        }

        /// <summary>
        /// 历史回放模式
        /// </summary>
        //public bool HistoryMode { get; set; }

        bool RefreshState(NEWTRACK gps, string fieldName = null, bool noWeb = false)
        {
            if (gps == null) return false;

            CarStatus.RefreshStatus(gps.STATUS, gps.STOPTIME);  //车辆状态
            OnPropertyChanged(nameof(CarStatus), true);
            AlarmStatus.RefreshAlarmStatus(gps.ALARM);
            OnPropertyChanged(nameof(AlarmStatus), true);

            if (gps.IsRealTime || _isCopy || (!gps.IsRealTime && !_isCopy))
            {   //在线或者历史回放状态刷新

                if (!_isCopy && (CarStatus.offline == true || !gps.IsRealTime))
                {  //车辆下线
                    CarState = CarState.OffLine;
                }
                else
                {
                    //在线状态下
                    if (AlarmStatus.HasAlarm)
                    {
                        //报警状态
                        CarState = CarState.Alarm;
                        if (!_isCopy)
                            OnHaveNewAlarm(AlarmType.Normal); //新的报警 
                    }
                    else if (gps.LOCATE != 1) //不定位
                    {
                        CarState = CarState.NoGps;
                    }
                    else if (gps.SPEED >= 3)    //行驶中
                    {
                        CarState = CarState.Run;
                    }
                    else
                    {   //速度小于3km/h 认定为停车（可去除部分漂移）
                        CarState = CarState.Stop;
                    }
                }
            }
            else
            {
                //不在线状态判断
                CarState = CarState.OffLine;
            }

            if (noWeb) return true;
            if (!gps.IsRealTime) return true;
            if (CarStatus.blindarea_status) return true;
            if (!NEWTRACK.IsWebPosition) return true;
            if (!IsInView) return true;
            if (!GPSTool.CheckLaLo(gps.LATITUDE, gps.LONGITUDE)) return true;
           
            GeoLatLng geo = GPSTool.wgs84togcj02(gps.LONGITUDE, gps.LATITUDE);
            geo.longitude = Math.Round(geo.longitude, 6);
            geo.latitude = Math.Round(geo.latitude, 6);
            Common_liby.InvokeMethodOnMainThread(() =>
            {
                HtmlPage.Window.Invoke("GetWebPosition", geo.longitude, geo.latitude, MAC);
            });
            return false;

        }

        /// <summary>
        /// 最后一条锁状态数据
        /// </summary>
        public SLLockGpsData LastLockGpsData
        {
            get
            {
                if (_lastLockGpsData == null)
                {
                    _lastLockGpsData = new SLLockGpsData();
                    _lastLockGpsData.PropertyChanged += (x, y) =>
                    {
                        //if (_isCopy)
                        //    return;
                        OnPropertyChanged(nameof(LastLockGpsData));
                    };
                }
                return _lastLockGpsData;
            }
            //private set
            //{
            //    _lastLockGpsData = value;
            //    if (value != null)
            //        _lastLockGpsData.PropertyChanged += (x, y) => OnPropertyChanged("LastLockGpsData");
            //    OnPropertyChanged("LastLockGpsData");
            //}
        }

        /// <summary>
        /// 车辆显示(图标)状态
        /// </summary>
        public CarState CarState
        {
            get { return _carState; }
            set
            {
                if (_carState != value)
                {
                    _carState = value;
                    switch (value)
                    {
                        case CarState.NoStatus:
                        case CarState.OffLine:
                            StateIcon = "/ImageResource;component/Images/car/stateOff.png";
                            break;
                        case CarState.NoGps:
                            StateIcon = "/ImageResource;component/Images/car/stateNoGps.png";
                            break;
                        case CarState.Stop:
                            StateIcon = "/ImageResource;component/Images/car/stateStop.png";
                            break;
                        case CarState.Run:
                            StateIcon = "/ImageResource;component/Images/car/stateOn.png";
                            break;
                        case CarState.Alarm:
                            StateIcon = "/ImageResource;component/Images/car/stateAlarm.png";
                            break;
                    }
                    OnPropertyChanged(nameof(CarState));
                }
            }
        }

        /// <summary>
        /// 车辆状态图标
        /// </summary>
        public string StateIcon
        {
            get { return _stateIcon; }
            private set
            {
                _stateIcon = value;
                OnPropertyChanged(nameof(StateIcon), true);
            }
        }

        /// <summary>
        /// 报警状态
        /// </summary>
        public AlarmStatus AlarmStatus
        {
            get
            {
                if (_alarmStatus == null)
                {
                    _alarmStatus = new AlarmStatus();
                }
                return _alarmStatus;
            }
        }

        /// <summary>
        /// 锁控报警
        /// </summary>
        public SLLcvAlarm LockAlarm
        {
            get { return _lockAlarm; }
            set
            {
                if (_lockAlarm != null)
                    _lockAlarm = null;

                _lockAlarm = value;
                OnPropertyChanged(nameof(LockAlarm));
                if (value != null)
                {
                    OnHaveNewAlarm(AlarmType.Lock);
                }
            }
        }

        /// <summary>
        /// 车辆状态
        /// </summary>
        public CarStatus CarStatus
        {
            get
            {
                if (_carStatus == null)
                {
                    _carStatus = new CarStatus();
                }
                return _carStatus;
            }
        }

        /// <summary>
        /// 车辆所在单位
        /// </summary>
        public UNIT Unit { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 重写ToString()方法 显示名字
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = LICENSE;
            if (string.IsNullOrEmpty(str))
                str = "未设置车牌";
            if (!string.IsNullOrWhiteSpace(MAC))
                str += $" ({MAC})";
            return str;
        }
        /// <summary>
        /// 复制一个对象
        /// </summary>
        /// <returns></returns>
        public new SLCar Copy()
        {
            SLCar copyCar = Common_liby.DeepCopy(this);
            if (copyCar != null)
            {
                copyCar._isCopy = true;
                if (copyCar.LastGpsData != null && copyCar.LastGpsData.IsRealTime)
                    copyCar.LastGpsData.IsRealTime = false;
            }
            return copyCar;
        }

        /// <summary>
        /// 车辆是否在服务期内
        /// </summary>
        /// <returns></returns>
        public bool IsInService()
        {
            if (!STOP_CAR_DATE.IsValid()) return true;
            return DateTimeHelper.DateTimeNow < STOP_CAR_DATE;
        }

        /// <summary>
        /// 是否在用户视野中
        /// </summary>
        public bool IsInView { get; set; }
        #endregion

        #region 扩展公务车相关属性

        /// <summary>
        /// 乘客
        /// </summary>
        public string PASSENGER
        {
            get { return _passenger; }
            set
            {
                _passenger = value;
                OnPropertyChanged(nameof(PASSENGER));
            }
        }

        /// <summary>
        /// 乘客电话
        /// </summary>
        public string PASSENGER_PHONE
        {
            get { return _passengerPhone; }
            set
            {
                _passengerPhone = value;
                OnPropertyChanged(nameof(PASSENGER_PHONE));
            }
        }

        /// <summary>
        /// 用车开始时间
        /// </summary>

        public DateTime BEGIN_TIME
        {
            get { return _beginTime; }
            set
            {
                _beginTime = value;
                OnPropertyChanged(nameof(BEGIN_TIME));
            }
        }

        /// <summary>
        /// 用车结束时间
        /// </summary>
        public DateTime END_TIME
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged(nameof(END_TIME));
            }
        }

        /// <summary>
        /// 调度状态 0 待命 1 调度中 2 维修 3 封存
        /// </summary>
        public int STATE
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged(nameof(STATE));
            }
        }

        /// <summary>
        /// 开始地点
        /// </summary>
        public string BEGIN_ADDRESS
        {
            get { return _beginAddress; }
            set
            {
                _beginAddress = value;
                OnPropertyChanged(nameof(BEGIN_ADDRESS));
            }
        }

        /// <summary>
        /// 结束地点
        /// </summary>
        public string END_ADDRESS
        {
            get { return _endAddress; }
            set
            {
                _endAddress = value;
                OnPropertyChanged(nameof(END_ADDRESS));
            }
        }

        /// <summary>
        /// 车辆类型
        /// </summary>
        public string CAR_TYPE_STR
        {
            get { return _carTypeStr; }
            set
            {
                _carTypeStr = value;
                OnPropertyChanged(nameof(CAR_TYPE_STR));
            }
        }

        /// <summary>
        /// 驾驶员姓名
        /// </summary>
        public new string DRIVER_NAME
        {
            get { return _driverName; }
            set
            {
                _driverName = value;
                OnPropertyChanged(nameof(DRIVER_NAME));
            }
        }

        /// <summary>
        /// 驾驶员电话
        /// </summary>
        public string DRIVER_PHONE
        {
            get { return _driverPhone; }
            set
            {
                _driverPhone = value;
                OnPropertyChanged(nameof(DRIVER_PHONE));
            }
        }

        #endregion
    }
}
