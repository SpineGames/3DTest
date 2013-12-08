using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3DTest._2_0.Instancing
{
    public class World
    {
        public const int MAX_INSTANCES = 1000;

        ThreeDInstance[] Instances;

        public World(int maxInstances = MAX_INSTANCES)
        {
            if (maxInstances <= MAX_INSTANCES)
                Instances = new ThreeDInstance[maxInstances];
            else
                throw new ArgumentException("Number of instances cannot exceed " + MAX_INSTANCES, "maxInstances");
        }
    }
}
