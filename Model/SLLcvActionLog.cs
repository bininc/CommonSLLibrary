using Models;

namespace CommonLibSL.Model
{
    public class SLLcvActionLog : LCV_ACTION_LOG
    {
        public SLCar Car { get; set; }

        public string IsCenterStr
        {
            get
            {
                if (ISCENTER == 1)
                    return "中心";
                else
                    return "刷卡";
            }
        }

        public string IsBlackListStr
        {
            get
            {
                if (ISBLACKLIST == 1)
                    return "是";
                else
                    return "否";
            }
        }

        public string ActionStr
        {
            get
            {
                if (ACTION == 1)
                    return "施封";
                else
                    return "解封";
            }
        }

        public string ResultStr
        {
            get
            {
                switch (RESULT)
                {
                    case 0:
                        return "成功";
                    case 1:
                        return "失败";
                    case 2:
                        return "拒绝";
                    case 3:
                        return "超时";
                    default:
                        return "未知";
                }
            }
        }

        public string IsLocateStr
        {
            get
            {
                if (Car == null||Car.LastGpsData==null)
                    return "-";
                return Car.LastGpsData.LOCATE_STR;
            }
        }

        public string AreaStr { get; set; }

        public string Place { get; set; }
    }
}
