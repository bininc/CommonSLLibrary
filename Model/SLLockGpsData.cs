using System;
using System.Windows.Media;
using CommLiby;
using Models;

namespace CommonLibSL.Model
{
    public class SLLockGpsData : ClassBase_Notify<SLLockGpsData>
    {
        private byte _mainLockState = 255;
        private string _lockState;
        private byte _lock1;
        private byte _lock2;
        private byte _lock3;
        private byte _lock4;
        private byte _lock5;
        private byte _lock6;
        private byte _lock7;
        private byte _lock8;
        private byte _lock9;
        private byte _lock10;
        private byte _lock11;
        private byte _lock12;
        private int _areaId;
        private DateTime _time;

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time == value) return;
                _time = value;
                OnPropertyChanged(nameof(Time), true);
            }
        }

        /// <summary>
        /// 主锁状态 0打开 1关闭
        /// </summary>
        public byte MainLockState
        {
            get { return _mainLockState; }
            set
            {
                if (_mainLockState == value) return;

                _mainLockState = value;
                if (value == 0)
                {
                    MainLockState_Str = "解封";
                }
                else if (value == 1)
                {
                    MainLockState_Str = "施封";
                }
                else
                {
                    MainLockState_Str = "";
                }
                OnPropertyChanged(nameof(MainLockState_Str), true);
            }
        }

        /// <summary>
        /// 主锁状态字符串
        /// </summary>
        public string MainLockState_Str { get; private set; }

        /// <summary>
        /// 分锁状态
        /// </summary>
        public string LockState
        {
            get { return _lockState; }
            set
            {
                if (_lockState == value) return;

                _lockState = value;
                if (!string.IsNullOrWhiteSpace(value) && value.Length > 11)
                {
                    Lock1 = (byte)value[3];
                    Lock2 = (byte)value[2];
                    Lock3 = (byte)value[1];
                    Lock4 = (byte)value[0];
                    Lock5 = (byte)value[7];
                    Lock6 = (byte)value[6];
                    Lock7 = (byte)value[5];
                    Lock8 = (byte)value[4];
                    Lock9 = (byte)value[8];
                    Lock10 = (byte)value[9];
                    Lock11 = (byte)value[10];
                    Lock12 = (byte)value[11];

                    //Lock1 = (byte)value[0];
                    //Lock2 = (byte)value[1];
                    //Lock3 = (byte)value[2];
                    //Lock4 = (byte)value[3];
                    //Lock5 = (byte)value[4];
                    //Lock6 = (byte)value[5];
                    //Lock7 = (byte)value[6];
                    //Lock8 = (byte)value[7];
                    //Lock9 = (byte)value[8];
                    //Lock10 = (byte)value[9];
                    //Lock11 = (byte)value[10];
                    //Lock12 = (byte)value[11];
                }
            }
        }

        /// <summary>
        /// 锁1状态
        /// </summary>
        public byte Lock1
        {
            get { return _lock1; }
            set
            {
                _lock1 = value;
                Lock1_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock1_Str), true);
            }
        }

        /// <summary>
        /// 锁1字符串
        /// </summary>
        public string Lock1_Str { get; private set; }

        /// <summary>
        /// 锁2状态
        /// </summary>
        public byte Lock2
        {
            get { return _lock2; }
            set
            {
                _lock2 = value;
                Lock2_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock2_Str), true);
            }
        }

        /// <summary>
        /// 锁2字符串
        /// </summary>
        public string Lock2_Str { get; private set; }

        /// <summary>
        /// 锁3状态
        /// </summary>
        public byte Lock3
        {
            get { return _lock3; }
            set
            {
                _lock3 = value;
                Lock3_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock3_Str), true);
            }
        }

        /// <summary>
        /// 锁3字符串
        /// </summary>
        public string Lock3_Str { get; private set; }

        /// <summary>
        /// 锁4状态
        /// </summary>
        public byte Lock4
        {
            get { return _lock4; }
            set
            {
                _lock4 = value;
                Lock4_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock4_Str), true);
            }
        }

        /// <summary>
        /// 锁4字符串
        /// </summary>
        public string Lock4_Str { get; private set; }

        /// <summary>
        /// 锁5状态
        /// </summary>
        public byte Lock5
        {
            get { return _lock5; }
            set
            {
                _lock5 = value;
                Lock5_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock5_Str), true);
            }
        }

        /// <summary>
        /// 锁5字符串
        /// </summary>
        public string Lock5_Str { get; private set; }

        /// <summary>
        /// 锁6状态
        /// </summary>
        public byte Lock6
        {
            get { return _lock6; }
            set
            {
                _lock6 = value;
                Lock6_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock6_Str), true);
            }
        }

        /// <summary>
        /// 锁6字符串
        /// </summary>
        public string Lock6_Str { get; private set; }

        /// <summary>
        /// 锁7状态
        /// </summary>
        public byte Lock7
        {
            get { return _lock7; }
            set
            {
                _lock7 = value;
                Lock7_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock7_Str), true);
            }
        }

        /// <summary>
        /// 锁7字符串
        /// </summary>
        public string Lock7_Str { get; private set; }

        /// <summary>
        /// 锁8状态
        /// </summary>
        public byte Lock8
        {
            get { return _lock8; }
            set
            {
                _lock8 = value;
                Lock8_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock8_Str), true);
            }
        }

        /// <summary>
        /// 锁8字符串
        /// </summary>
        public string Lock8_Str { get; private set; }

        /// <summary>
        /// 锁9状态
        /// </summary>
        public byte Lock9
        {
            get { return _lock9; }
            set
            {
                _lock9 = value;
                Lock9_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock9_Str), true);
            }
        }

        /// <summary>
        /// 锁9字符串
        /// </summary>
        public string Lock9_Str { get; private set; }

        /// <summary>
        /// 锁10状态
        /// </summary>
        public byte Lock10
        {
            get { return _lock10; }
            set
            {
                _lock10 = value;
                Lock10_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock10_Str), true);
            }
        }

        /// <summary>
        /// 锁10字符串
        /// </summary>
        public string Lock10_Str { get; private set; }

        /// <summary>
        /// 锁11状态
        /// </summary>
        public byte Lock11
        {
            get { return _lock11; }
            set
            {
                _lock11 = value;
                Lock11_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock11_Str), true);
            }
        }

        /// <summary>
        /// 锁11字符串
        /// </summary>
        public string Lock11_Str { get; private set; }

        /// <summary>
        /// 锁12状态
        /// </summary>
        public byte Lock12
        {
            get { return _lock12; }
            set
            {
                _lock12 = value;
                Lock12_Str = GetLockStr(value);
                OnPropertyChanged(nameof(Lock12_Str), true);
            }
        }

        /// <summary>
        /// 锁12字符串
        /// </summary>
        public string Lock12_Str { get; private set; }

        /// <summary>
        /// 根据字节得到锁状态描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string GetLockStr(byte value)
        {
            //解封
            if (value == 0x30)
                return "—";
            if (value == 0x31)
                return "关";
            if (value == 0x32)
                return "开";
            if (value == 0x33)
                return "超时";
            if (value == 0x34)
                return "被拆";
            if (value == 0x35)
                return "掉电";
            if (value == 0x36)
                return "半开";
            //施封
            if (value == 0x40)
                return "—";
            if (value == 0x41)
                return "关";
            if (value == 0x42)
                return "开";
            if (value == 0x43)
                return "超时";
            if (value == 0x44)
                return "被拆";
            if (value == 0x45)
                return "掉电";
            if (value == 0x46)
                return "半开";

            return string.Empty;
        }

        /*
        /// <summary>
        /// 区域ID
        /// </summary>
        public int AreaID
        {
            get { return _areaId; }
            set
            {
                if (_areaId == value) return;
                _areaId = value;
                Area_Str = NEWTRACK.GetAreaNameById(value);
                OnPropertyChanged(nameof(Area_Str), true);
            }
        }

        /// <summary>
        /// 区域名称
        /// </summary>
        public string Area_Str { get; private set; }
        */
    }
}