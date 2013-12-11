using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest._2_0.Instancing
{
    public class World
    {
        /// <summary>
        /// Gets the constant max number of instances for any world
        /// </summary>
        public const int MAX_INSTANCES = 1000;

        /// <summary>
        /// The list of instances in this world
        /// </summary>
        List<ThreeDInstance> instances;

        /// <summary>
        /// The graphics device to render with
        /// </summary>
        GraphicsDevice GraphicsDevice;

        /// <summary>
        /// Creates a new world
        /// </summary>
        /// <param name="GraphicsDevice">The graphics device to use</param>
        /// <param name="maxInstances">The maximum number of instances for this world</param>
        public World(GraphicsDevice GraphicsDevice, int maxInstances = MAX_INSTANCES)
        {
            this.GraphicsDevice = GraphicsDevice;

            if (maxInstances <= MAX_INSTANCES)
                instances = new List<ThreeDInstance>(MAX_INSTANCES);
            else
                throw new ArgumentException("Number of instances cannot exceed " + MAX_INSTANCES, "maxInstances");
        }

        /// <summary>
        /// Renders this world
        /// </summary>
        public void Render()
        {
            for (int i = 0; i < instances.Count; i++)
                instances[i].Render(GraphicsDevice);
        }

        /// <summary>
        /// Adds an instance and return's it's ID
        /// </summary>
        /// <param name="Instance">The instance to add</param>
        /// <returns>The ID of the instance</returns>
        /// <exception cref="InsufficientMemoryException">
        /// Raised if the instance count surpasses the level's max (Instance is not added)</exception>
        public int AddInstance(ThreeDInstance Instance)
        {
            if (instances.Count < instances.Capacity)
            {
                instances.Add(Instance);
                return instances.Count - 1;
            }
            else
                throw new InsufficientMemoryException("Cannot add surpass level max instances!");
        }

        /// <summary>
        /// Removes an instance from the world and returns true if the instance existed
        /// </summary>
        /// <param name="Instance">The instance to remove</param>
        /// <returns>True if the instance was removed</returns>
        public bool RemoveInstance(ThreeDInstance Instance)
        {
            return instances.Remove(Instance);
        }

        /// <summary>
        /// Removes an instance from the world given it's ID
        /// </summary>
        /// <param name="ID">The ID of the instance to remove</param>
        /// <exception cref="IndexOutOfRangeException">
        /// Raised if the index does not exist</exception>
        public void RemoveInstance(int ID)
        {
            if (ID > 0 && ID < instances.Count)
                instances.RemoveAt(ID);
            else
                throw new IndexOutOfRangeException("The given ID does not exist!");
        }
    }
}
