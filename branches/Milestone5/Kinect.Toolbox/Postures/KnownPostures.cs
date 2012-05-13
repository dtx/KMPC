using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinect.Toolbox.Postures
{
   /// <summary>
   /// This enum contains all possible postures detected and raised by 
   /// posture detectors in the posture classes
   /// </summary>
    public enum KnownPostures
    {
        HandsJoined,
        HandsApart,
        LeftHandOverHead,
        RightHandOverHead,
        LeftHello,
        RightHello
    }
}
