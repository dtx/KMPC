using System;
using System.IO;
using Microsoft.Kinect;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kinect.Toolbox.Record
{
    public class SkeletonRecorder
    {
        Stream recordStream;
        BinaryWriter writer;
        DateTime referenceTime;

        public void Start(Stream stream)
        {
            recordStream = stream;
            writer = new BinaryWriter(recordStream);

            referenceTime = DateTime.Now;
        }

        public void Record(SkeletonFrame frame)
        {
            if (writer == null)
                throw new Exception("You must call Start before calling Record");

            TimeSpan timeSpan = DateTime.Now.Subtract(referenceTime);
            referenceTime = DateTime.Now;
            writer.Write((long)timeSpan.TotalMilliseconds);
            writer.Write(frame.FloorClipPlane.Item1);
            writer.Write(frame.FloorClipPlane.Item2);
            writer.Write(frame.FloorClipPlane.Item3);
            writer.Write(frame.FloorClipPlane.Item4);

            Skeleton[] skeletons = Tools.GetSkeletons(frame);
            frame.CopySkeletonDataTo(skeletons);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, skeletons);
        }

        public void Stop()
        {
            if (writer == null)
                throw new Exception("You must call Start before calling Stop");

            writer.Close();
            writer.Dispose();

            recordStream.Dispose();
            recordStream = null;
        }
    }
}
