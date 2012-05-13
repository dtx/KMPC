using System;
using System.IO;
using Microsoft.Kinect;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kinect.Toolbox.Record
{
    public class ReplaySkeletonFrame
    {
        public Tuple<float, float, float, float> FloorClipPlane { get; private set; }
        public int FrameNumber { get; private set; }
        public Skeleton[] Skeletons { get; private set; }
        public long TimeStamp { get; private set; }

        public ReplaySkeletonFrame(SkeletonFrame frame)
        {
            FloorClipPlane = frame.FloorClipPlane;
            FrameNumber = frame.FrameNumber;
            TimeStamp = frame.Timestamp;
            Skeletons = Tools.GetSkeletons(frame);
        }

        internal ReplaySkeletonFrame(BinaryReader reader, int frameNumber)
        {
            TimeStamp = reader.ReadInt64();
            FloorClipPlane = new Tuple<float, float, float, float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            FrameNumber = frameNumber;

            BinaryFormatter formatter = new BinaryFormatter();
            Skeletons = (Skeleton[]) formatter.Deserialize(reader.BaseStream);
        }

        public static implicit operator ReplaySkeletonFrame(SkeletonFrame frame)
        {
            return new ReplaySkeletonFrame(frame);
        }
    }
}
