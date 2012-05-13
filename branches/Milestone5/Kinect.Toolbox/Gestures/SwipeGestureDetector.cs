using System;
using Microsoft.Kinect;
using Kinect.Toolbox.Gestures;
namespace Kinect.Toolbox
{
    public class SwipeGestureDetector : GestureDetector
    {
        const float SwipeMinimalLength = 0.4f;
        const float SwipeMaximalHeight = 0.2f;
        const int SwipeMininalDuration = 250;
        const int SwipeMaximalDuration = 1500;
        /// <summary>
        /// The swipe gesture detector detects whether sqipes are made with joints and raises
        /// gesture detected for 4 motions (up, down, left, and right)
        /// </summary>
        /// <param name="windowSize"></param>
        public SwipeGestureDetector(int windowSize = 20)
            : base(windowSize)
        {
            
        }


        ///
        /// <summary>
        /// Detects whether the appropriate swipe was made based on the functions defined for the swipes 
        /// (Up, down, left, and right)
        /// </summary>
        /// <param name="heightConstraint"></param>
        /// <param name="directionMaintainedConstraint"></param>
        /// <param name="lengthFunction"></param>
        /// <param name="minTime"></param>
        /// <param name="maxTime"></param>
        /// <returns></returns>
        bool ScanPositions(Func<Vector3, Vector3, bool> heightConstraint, 
            Func<Vector3, Vector3, bool> directionMaintainedConstraint, 
            Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightConstraint(Entries[0].Position, Entries[index].Position) || !directionMaintainedConstraint(Entries[index].Position, Entries[index + 1].Position))
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
        /// <summary>
        /// Utilize ScanPosition to determine movements
        /// </summary>
        protected override void LookForGesture()
        {
            // Swipe to right
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                (p1, p2) => p2.X - p1.X > -0.01f, // Progression to right
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected(KnownGestures.SwipeRight.ToString());
                return;
            }

            // Swipe to left
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  
                (p1, p2) => p2.X - p1.X < 0.01f, 
                (p1, p2) => Math.Abs(p2.X - p1.X) > SwipeMinimalLength, 
                SwipeMininalDuration, SwipeMaximalDuration))
            {
                RaiseGestureDetected(KnownGestures.SwipeLeft.ToString());
                return;
            }
            // Swipe Down
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight, 
               (p1, p2) => p1.Y - p2.Y > -0.01f, 
               (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, 
               SwipeMininalDuration, SwipeMaximalDuration)) 
            {
                RaiseGestureDetected(KnownGestures.SwipeDown.ToString());
                return;
            }
            //Swipe Up
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight, 
               (p1, p2) => p2.Y - p1.Y > -0.01f, 
               (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, 
               SwipeMininalDuration, SwipeMaximalDuration)) 
            {
                RaiseGestureDetected(KnownGestures.SwipeUp.ToString());
                return;
            }
        }
    }
}