using System;

namespace Flekosoft.Common.Video.H264
{
    // ReSharper disable once InconsistentNaming
    public class NALUnit : Frame
    {

        public NALUnit(DateTime timeStamp, byte[] rawData, Resolution resolution, FrameFormat frameFormat) : base(timeStamp, rawData, resolution, frameFormat)
        {
            if (RawData[0] == 0x00 && RawData[1] == 0x00 && RawData[2] == 0x00 && RawData[3] == 0x01) PrefixLength = 4;
            else if (RawData[0] == 0x00 && RawData[1] == 0x00 && RawData[2] == 0x01) PrefixLength = 3;
            else throw new ArgumentException("NAL Unit prefix error");
        }

        public int PrefixLength { get; protected set; }


        /// <summary>
        /// A value of 0 indicates that the NAL unit
        /// type octet and payload should not contain bit errors or other
        /// syntax violations.  A value of 1 indicates that the NAL unit
        /// type octet and payload may contain bit errors or other syntax
        /// violations.
        /// </summary>
        public bool ForbiddenZeroBit
        {
            get { return (RawData[PrefixLength + 0] & 0x80) >> 7 == 0x01; }
            set
            {
                if (value) RawData[PrefixLength + 0] |= 0x80;
                else RawData[PrefixLength + 0] &= 0x7F;
            }
        }

        public int NalRefIdc
        {
            get { return (RawData[PrefixLength + 0] & 0x60) >> 5; }
            set
            {
                RawData[PrefixLength + 0] |= (byte)(((byte)value & 0x03) << 5);
            }
        }

        public NALUnitType UnitType
        {
            get { return (NALUnitType)(RawData[PrefixLength + 0] & 0x1F); }
            set
            {
                RawData[PrefixLength + 0] |= (byte)((byte)value & 0x1F);
            }
        }



    }
}
