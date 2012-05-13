using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox.Postures;
namespace Kinect.Toolbox.Gestures
{
    public class TwoHandedGestureDetector : GestureDetector
    {
        private AlgorithmicPostureDetector _postureDetector;
        private DateTime _prevHandClose;
        private DateTime _prevGesture;
        private const int TimeBetweenGestures = 1000;
        private const int HandOpeningTimeout = 800;

        /// <summary>
        /// This class handles detection of two handed gewtures. It inputs
        /// an algorithmic posture detector to detect changes in posture
        /// and appropriately react to these changes        
        /// </summary>
        /// <param name="postureDetector"></param>
        
        public TwoHandedGestureDetector(AlgorithmicPostureDetector postureDetector)
        {
            this._postureDetector = postureDetector;
            this._postureDetector.PostureDetected += DetectHandsOpening;
            _prevGesture = DateTime.MinValue;
            _prevHandClose = DateTime.MinValue;
        }

        /// <summary>
        ///  
        /// Is called when a posture changes to determine whether
        ///the gesture of hands opening ocvcured 
        /// </summary>
        /// <param name="currentPosture"></param>
        private void DetectHandsOpening(String currentPosture)
        {
            if (currentPosture == KnownPostures.HandsApart.ToString())
            {
                if ( ((DateTime.Now - _prevHandClose).TotalMilliseconds < HandOpeningTimeout))
                {
                    _prevGesture = DateTime.Now;
                    _prevHandClose = DateTime.MinValue;
                    //hands opened in the threshold amount of time, so raise the gesture for hands opening
                    RaiseGestureDetected(KnownGestures.HandsOpened.ToString());
                }
            }
            else if (currentPosture == KnownPostures.HandsJoined.ToString())
            {
                _prevHandClose = DateTime.Now;
            }
        }

        protected override void LookForGesture(){
        }
    }
}
