using System;
using Models;

namespace CommonLibSL.Model
{
    public class SLLcvAlarm : LCV_ALARM
    {
        private SLLockGpsData _lockGpsData;

        public SLLcvAlarm()
        {

        }

        public SLLockGpsData LockGpsData
        {
            get
            {
                if (_lockGpsData == null)
                {
                    _lockGpsData = new SLLockGpsData() { MainLockState = MAINLOCK, LockState = SUBLOCK };
                }
                return _lockGpsData;
            }
        }

        public string AlarmStatus_Str { get; set; }

        public string Area_Str { get; set; }

        public override string ToString()
        {
            return AlarmStatus_Str;
        }
    }
}
