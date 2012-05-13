using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
namespace Kinect.Toolbox.Gestures
{
    public class GestureSuite : GestureDetector
    {
        private CircleGestureDetector _circleDetector;
        private SwipeGestureDetector _swipeGestureRecognizer;
        private TwoHandedGestureDetector _twoHandedDetector;
        private readonly AlgorithmicPostureDetector _algorithmicPostureRecognizer = new AlgorithmicPostureDetector();
        private DateTime _prevGesture;
        private const int TimeBetweenGestures = 1200;
        /// <summary>
        /// This suite consolidates all of the detectors to one suite
        /// so clients only have to add one overall detector to get all of the
        /// gestures
        /// </summary>
        public GestureSuite()
        {
            _circleDetector = new CircleGestureDetector();
            _swipeGestureRecognizer = new SwipeGestureDetector();
            _twoHandedDetector = new TwoHandedGestureDetector(_algorithmicPostureRecognizer);
            _circleDetector.OnGestureDetected += GestureDelegate;
            _swipeGestureRecognizer.OnGestureDetected += GestureDelegate;
            _twoHandedDetector.OnGestureDetected += GestureDelegate;
            _prevGesture = DateTime.MinValue;
        }
        /// <summary>
        /// This captures all of the gestures detected and makes sure
        /// none go out too often to the delegates associated to this suite.
        /// </summary>
        /// <param name="gesture"></param>
        private void GestureDelegate(string gesture)
        {
            if ((DateTime.Now - _prevGesture).TotalMilliseconds > TimeBetweenGestures)
            {
                _prevGesture = DateTime.Now;
                RaiseGestureDetected(gesture);
            }
        }

        /// <summary>
        /// Required Override for GestureDetector
        /// </summary>
        protected override void LookForGesture()
        {
        }

        /// <summary>
        /// Adds the skeleton to all detectors
        /// </summary>
        /// <param name="?"></param>
        /// <param name="sensor"></param>
        public void Add(Microsoft.Kinect.Skeleton skeleton, Microsoft.Kinect.KinectSensor sensor)
        {
            _twoHandedDetector.Add(skeleton, sensor);
            _algorithmicPostureRecognizer.TrackPostures(skeleton);

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.JointType == JointType.HandRight)
                {
                    _circleDetector.Add(joint.Position, sensor);
                    _swipeGestureRecognizer.Add(joint.Position, sensor);
                }
            }
        }
    }
}
