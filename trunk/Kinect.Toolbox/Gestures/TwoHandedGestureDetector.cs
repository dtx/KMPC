using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox.Postures;
using Microsoft.Kinect;
using System.IO;
namespace Kinect.Toolbox.Gestures
{
    /// <summary>
    /// This class specializes in two handed gestures which currently include:
    /// Hands Opening
    /// Hands Closing
    /// </summary>
    public class TwoHandedGestureDetector : GestureDetector
    {
        /// <summary>
        /// Relevant timestamps for all of the different hand closings
        /// </summary>
        private DateTime _prevHandClose;
        private DateTime _prevGesture;
        private DateTime _prevLHandOpen;
        private DateTime _prevRHandOpen;
        private DateTime _prevLHandClose;
        private DateTime _prevRHandClose;
        
        /// <summary>
        /// Constraints for gesture detection
        /// </summary>
        private const int TimeBetweenGestures = 1000;
        private const int HandOpeningTimeout = 800;

        /// <summary>
        /// Diagonal Detectors for both hands
        /// </summary>
        private DiagonalGestureDetector _rightHandDetector;
        private DiagonalGestureDetector _leftHandDetector;
        
        
        /// <summary>
        /// Strings to label the intermediate gestures
        /// </summary>
        private const string RightHandOpen = "RightHandOpen";
        private const string RightHandClose = "RightHandClose";
        private const string LeftHandOpen = "LeftHandOpen";
        private const string LeftHandClose = "LeftHandClose";

        private AlgorithmicPostureDetector _postureDetector;
        /// <summary>
        /// This class handles detection of two handed gewtures. It inputs
        /// an algorithmic posture detector to detect changes in posture
        /// and appropriately react to these changes        
        /// </summary>
        /// <param name="postureDetector"></param>
        
        public TwoHandedGestureDetector( AlgorithmicPostureDetector postureDetector)
        {
            //Initalize values to absolute minimum
            _prevGesture = DateTime.MinValue;
            _prevHandClose = DateTime.MinValue;
            _prevRHandOpen = DateTime.MinValue;
            _prevLHandOpen = DateTime.MinValue;
            _prevRHandClose = DateTime.MinValue;
            _prevLHandClose = DateTime.MinValue;

            _rightHandDetector = new DiagonalGestureDetector(RightHandOpen, RightHandClose);
            _leftHandDetector = new DiagonalGestureDetector(LeftHandOpen, LeftHandClose);
            _rightHandDetector.OnGestureDetected += GestureDetect;
            _leftHandDetector.OnGestureDetected += GestureDetect;
            
            _postureDetector = postureDetector;
            postureDetector.PostureDetected += GestureDetect;
        }

        /// <summary>
        /// Required override for gesture detection
        /// </summary>
        protected override void LookForGesture(){
        }
        
        /// <summary>
        /// Adds right hand joint for right diagonal detection and left hand joint for
        /// left hand detection
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="sensor"></param>
        public void Add(Skeleton skeleton, KinectSensor sensor)
        {
            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.JointType == JointType.HandLeft)
                {
                    _leftHandDetector.Add(joint.Position, sensor);
                }
                else if (joint.JointType == JointType.HandRight)
                {
                    _rightHandDetector.Add(joint.Position, sensor);
                }
            }
        }


        /// <summary>
        /// This method makes sure all of the conditions are met to 
        /// raise gesture detection for hands opening and closing
        /// </summary>
        /// <param name="gesture"></param>
        public void GestureDetect(string gesture)
        {
            //Update timestamps for all relevant DateTime's
            if (gesture == RightHandOpen) _prevRHandOpen = DateTime.Now;
            else if (gesture == LeftHandOpen) _prevLHandOpen = DateTime.Now;
            else if (gesture == RightHandClose) _prevRHandClose = DateTime.Now;
            else if (gesture == LeftHandClose) _prevLHandClose = DateTime.Now;
            else if (gesture == KnownPostures.HandsJoined.ToString()) _prevHandClose = DateTime.Now;
            if ((gesture == KnownPostures.HandsJoined.ToString() ||   gesture== RightHandClose || gesture == LeftHandOpen) && 
                Math.Min(millisDiff(_prevLHandOpen), millisDiff(_prevRHandClose)) < TimeBetweenGestures &&
                Math.Abs((_prevLHandOpen - _prevRHandClose).TotalMilliseconds) < HandOpeningTimeout)
            {
                RaiseGestureDetected(KnownGestures.HandsClosed.ToString());
            }
            if ((gesture == RightHandOpen || gesture == LeftHandClose) &&
                 Math.Min(millisDiff(_prevRHandOpen), millisDiff(_prevLHandClose)) < TimeBetweenGestures &&
                Math.Abs((_prevLHandClose - _prevRHandOpen).TotalMilliseconds) < HandOpeningTimeout)
                //&& millisDiff(_prevHandClose) < HandOpeningTimeout)
            {
                RaiseGestureDetected(KnownGestures.HandsOpened.ToString());
            }
        }


        /// <summary>
        /// Simple calculator to tell difference between time and
        /// current time
        /// </summary>
        /// <param name="time">time to compare to</param>
        /// <returns>Difference between time and Now in milliseconds</returns>
        private double millisDiff(DateTime time)
        {
            return (DateTime.Now - time).TotalMilliseconds;
        }
    }


    /// <summary>
    /// Class aids in detecting whether hands moved in a diagonal fashion
    /// </summary>
    class DiagonalGestureDetector : GestureDetector
    {
        const float DiagonalMinimalLength = 0.3f;
        const float DiagonalMaximalHeight = 0.2f;
        const int DiagonalMininalDuration = 250;
        const int DiagonalMaximalDuration = 1500;
        private string _gestureUpwards;
        private string _gestureDownwards;

        /// <summary>
        /// Initailizes Diagonal Gesture detection for one hand and detects upward and 
        /// downward diagonal
        /// </summary>
        /// <param name="gestureUp"> Gesture to raise for upward detection</param>
        /// <param name="gestureDown"> Gesture to raise for downward detection</param>
        /// <param name="windowSize"></param>
        public DiagonalGestureDetector(string gestureUp, string gestureDown, int windowSize = 20)
            : base(windowSize)
        {
            _gestureUpwards = gestureUp;
            _gestureDownwards = gestureDown;
        }

        /// <summary>
        /// Scan positions to aid in diagonal detection
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
        /// This specifies the parameters for progress in both diagonal directions
        /// </summary>
        protected override void LookForGesture()
        {
            if (ScanPositions((p1, p2) => true,
               (p1, p2) => p2.X - p1.X > -0.01f && p2.Y - p1.Y > -.01f,
               (p1, p2) => Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2)) > DiagonalMinimalLength,
               DiagonalMininalDuration, DiagonalMaximalDuration))
            {
                RaiseGestureDetected(_gestureUpwards);
                return;
            }
            if (ScanPositions((p1, p2) => true,
               (p1, p2) => p1.X - p2.X > -0.01f && p1.Y - p2.Y > -.01f,
               (p1, p2) => Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2)) > DiagonalMinimalLength,
               DiagonalMininalDuration, DiagonalMaximalDuration))
            {
                RaiseGestureDetected(_gestureDownwards);
                return;
            }
        }
    }
}
