using System;
using Microsoft.Kinect;

namespace Kinect.Toolbox
{
    public class SwipeGestureDetector : GestureDetector
    {
        const float SwipeMinimalLength = 0.4f;
        const float SwipeMaximalHeight = 0.2f;
        const int SwipeMininalDuration = 250;
        const int SwipeMaximalDuration = 1500;

        public SwipeGestureDetector(int windowSize = 20)
            : base(windowSize)
        {
            
        }

        bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction, Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()
        {
            // Swipe to right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                //I think raiseGestureDetected is the method that prints the gesture we just performed in the GUI right hand side
                //it is defined in GestureDetector.cs, go to that file and check my comments there.
                //All We do is crosscheck the parameter string with a already known array.
                RaiseGestureDetected("SwipeToRight1");
                return;
            }

            // Swipe to left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                (p1, p2) => p2.X - p1.X < 0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration))// Duration
            {
                RaiseGestureDetected("SwipeToLeft");
                return;
            }

            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight, // Height
               (p1, p2) => p1.Y - p2.Y > -0.01f, // Progression to right
               (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, // Length
               SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected("SwipeDown");
                return;
            }
            
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight, // Height
               (p1, p2) => p2.Y - p1.Y > -0.01f, // Progression to right
               (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, // Length
               SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected("SwipeUp");
                return;
            }
        }
    }
}