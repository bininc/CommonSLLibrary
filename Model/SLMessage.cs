using System;

namespace CommonLibSL.Model
{
    public class SLMessage
    {
        public string Message { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Time { get; set; }
        public SLCar Car { get; set; }
        public short MessageType { get; set; }
    }
}
