using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kinect.Toolbox.Record
{
    public class SkeletonReplay
    {
        public event EventHandler<ReplaySkeletonFrameReadyEventArgs> SkeletonFrameReady;

        readonly List<ReplaySkeletonFrame> frames = new List<ReplaySkeletonFrame>();

        SynchronizationContext context;
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SkeletonReplay(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int frameNumber = 0;

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                ReplaySkeletonFrame frame = new ReplaySkeletonFrame(reader, frameNumber++);

                frames.Add(frame);
            }

            reader.Dispose();
            stream.Dispose();
        }

        public void Start()
        {
            context = SynchronizationContext.Current;

            CancellationToken token = cancellationTokenSource.Token;

            Task.Factory.StartNew(() =>
            {
                foreach (ReplaySkeletonFrame frame in frames)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(frame.TimeStamp));

                    if (token.IsCancellationRequested)
                        return;
                                              
                    ReplaySkeletonFrame closure = frame;
                    context.Send(state =>
                                    {
                                        if (SkeletonFrameReady != null)
                                            SkeletonFrameReady(this, new ReplaySkeletonFrameReadyEventArgs {SkeletonFrame = closure});
                                    }, null);
                }
            }, token);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
