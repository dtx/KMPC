using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinect.Toolbox;
using Microsoft.Kinect;

namespace Kinect.Toolbox.Gestures
{   
    
    /// <summary>
    /// Detects and raises clockwise and counterclockwise gestures
    /// by comparing to known instances of these gestures stored in a 
    /// file (known as knowledge bases) 
    /// </summary>
    
    public class CircleGestureDetector : GestureDetector
    {
        //initialize the two detectors for both gestures
        private TemplatedGestureDetector _clockwiseDetector;
        private TemplatedGestureDetector _counterClockwiseDetector;

        public TemplatedGestureDetector ClockwiseDetector
        {
            get; set;
        }


        public TemplatedGestureDetector CounterClockwiseDetector
        {   
            get; set;
        }

        string clockwisePath;
        string counterClockwisePath;

        /// <summary>
        /// Initializes the circle detector with knowledge bases for clockwise
        /// counter-clockwise motions. 
        /// </summary>
 
        public CircleGestureDetector()
        {
            //initialize the streams for the two knowledge bases
            clockwisePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\clockwise.save");
            counterClockwisePath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\counterClockwise.save");

            Stream clockwiseStream = File.Open(clockwisePath, FileMode.OpenOrCreate);
            Stream counterClockwiseStream = File.Open(counterClockwisePath, FileMode.OpenOrCreate);
            
            //initialize the detectors with the existing knowledge bases
            _clockwiseDetector = new TemplatedGestureDetector(KnownGestures.Clockwise.ToString(), clockwiseStream);
            _counterClockwiseDetector = new TemplatedGestureDetector(KnownGestures.CounterClockwise.ToString(), counterClockwiseStream);

            //associate gesture detection with function in this class
            _clockwiseDetector.OnGestureDetected += gestureDetected;
            _counterClockwiseDetector.OnGestureDetected += gestureDetected;
        }
        
        /// <summary>
        /// Adds joint to detect circular motion on (usually right or left hand)
        /// </summary>
        /// <param name="position"> Hand to add </param>
        /// <param name="sensor"> Kinect reference </param>
        public override void Add(SkeletonPoint position, KinectSensor sensor)
        {
            _clockwiseDetector.Add(position, sensor);
            _counterClockwiseDetector.Add(position, sensor);
        }

        protected override void LookForGesture()
        {
        }
        /// <summary>
        /// Called when the templated detectors detect a gesture
        /// We simply have to pass it on to the structures subscribed to this detector 
        /// </summary>
        /// <param name="gesture"></param>
        private void gestureDetected(String gesture)
        {
            RaiseGestureDetected(gesture);
        }

    }
}
