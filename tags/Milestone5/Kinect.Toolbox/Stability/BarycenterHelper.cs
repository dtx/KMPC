using System.Collections.Generic;
using System.Diagnostics;

namespace Kinect.Toolbox
{
    public class BarycenterHelper
    {
        readonly Dictionary<int, List<Vector3>> positions = new Dictionary<int, List<Vector3>>();
        readonly int windowSize;

        public float Threshold { get; set; }

        public BarycenterHelper(int windowSize = 20, float threshold = 0.05f)
        {
            this.windowSize = windowSize;
            Threshold = threshold;
        }

        public bool IsStable(int trackingID)
        {
            List<Vector3> currentPositions = positions[trackingID];
            if (currentPositions.Count != windowSize)
                return false;

            Vector3 current = currentPositions[currentPositions.Count - 1];

            for (int index = 0; index < currentPositions.Count - 2; index++)
            {
                if ((currentPositions[index] - current).Length > Threshold)
                    return false;
            }

            return true;
        }

        public void Add(Vector3 position, int trackingID)
        {
            if (!positions.ContainsKey(trackingID))
                positions.Add(trackingID, new List<Vector3>());

            positions[trackingID].Add(position);

            if (positions[trackingID].Count > windowSize)
                positions[trackingID].RemoveAt(0);
        }
    }
}
