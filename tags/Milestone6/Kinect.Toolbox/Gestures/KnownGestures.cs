using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinect.Toolbox.Gestures
{
    /// <summary>
    /// This is an enum of all possible gestures that will be detected and raised
    /// by the relevant gesture detectors
    /// </summary>
    public enum KnownGestures
    {
        SwipeDown,
        SwipeUp,
        SwipeLeft,
        SwipeRight,
        Clockwise,
        CounterClockwise,
        HandsOpened,
        HandsClosed
    }
}
