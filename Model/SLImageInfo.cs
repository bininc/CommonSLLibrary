using CommLiby;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using Models;
using Newtonsoft.Json;

namespace CommonLibSL.Model
{
    public class SLImageInfo : INFOIMAGE
    {
        private int _seq;

        /// <summary>
        /// 图片id
        /// </summary>
        public int seq
        {
            get { return _seq; }
            set
            {
                _seq = value;
                WebTools.GET(((b, status, arg3) =>
                {
                    if (WebTools.IsReturnSuc(b, status, arg3))
                    {
                        INFOIMAGE img = JsonConvert.DeserializeObject<INFOIMAGE>(arg3.tag.ToString());
                        if (img != null)
                        {
                            ALARMID = img.ALARMID;
                            CARID = img.CARID;
                            CNLID = img.CNLID;
                            IMAGEBase64 = img.IMAGEBase64;
                            IMAGEID = img.IMAGEID;
                            RECORDTIME = img.RECORDTIME;
                            TASKNO = img.TASKNO;
                            license = img.license;
                            _imageBytes = Convert.FromBase64String(IMAGEBase64);

                            ImageLoadCompleted?.Invoke(this);
                        }
                    }
                }), "InfoImage", _seq.ToString());
            }
        }

        private byte[] _imageBytes;
        private BitmapImage _image;

        /// <summary>
        /// 图片
        /// </summary>
        public BitmapImage Image
        {
            get
            {
                if (_image == null && _imageBytes != null)
                {
                    _image = new BitmapImage();
                    _image.SetSource(new MemoryStream(_imageBytes));
                }
                return _image;
            }
            private set { _image = value; }
        }

        /// <summary>
        /// Mac地址
        /// </summary>
        public string Mac { get; set; }
        /// <summary>
        /// 图片加载完成
        /// </summary>
        public static event Action<SLImageInfo> ImageLoadCompleted;

    }
}
