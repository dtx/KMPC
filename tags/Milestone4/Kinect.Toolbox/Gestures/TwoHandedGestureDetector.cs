using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox.Postures;
namespace Kinect.Toolbox.Gestures
{
    public class TwoHandedGestureDetector : GestureDetector
    {
        private AlgorithmicPostureDetector postureDetector;
        private DateTime prevHandClose;
        private const int timeBetweenGestures = 1000;
        private DateTime prevGesture;
        private const int handOpeningTimeout = 800;
        public TwoHandedGestureDetector(AlgorithmicPostureDetector postureDetector)
        {
            this.postureDetector = postureDetector;
            this.postureDetector.PostureDetected += detectHandsOpening;
            prevGesture = DateTime.MinValue;
            prevHandClose = DateTime.MinValue;
        }

        private void detectHandsOpening(String currentPosture)
        {
            if (currentPosture == KnownPostures.HandsApart)
            {
                if ( ((DateTime.Now - prevHandClose).TotalMilliseconds < handOpeningTimeout))
                    //&& ((DateTime.Now - prevGesture).TotalMilliseconds > timeBetweenGestures) )
                {
                   // if ((DateTime.Now - prevGesture).TotalMilliseconds < 1000) return;
                    prevGesture = DateTime.Now;
                    prevHandClose = DateTime.MinValue;
                    RaiseGestureDetected("HandsOpened");
                }
            }
            else if (currentPosture == KnownPostures.HandsJoined)
            {
                prevHandClose = DateTime.Now;
            }
        }
        protected override void LookForGesture(){
        }
    }
}
