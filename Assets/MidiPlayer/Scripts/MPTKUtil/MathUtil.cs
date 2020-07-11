using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiPlayerTK
{
    public class MovingAverage
    {
        private Queue<int> samples;
        private int windowSize = 16;
        private int sampleAccumulator;

        public int Average
        {
            get
            {
                if (samples.Count > 0)
                    return sampleAccumulator / samples.Count;
                else
                    return 0;
            }
        }

        public MovingAverage()
        {
            samples = new Queue<int>();
        }

        public MovingAverage(int size)
        {
            samples = new Queue<int>();
            windowSize = size;
        }

        /// <summary>
        /// Computes a new windowed average each time a new sample arrives
        /// </summary>
        /// <param name="newSample"></param>
        public void Add(int newSample)
        {
            sampleAccumulator += newSample;
            samples.Enqueue(newSample);

            if (samples.Count > windowSize)
            {
                sampleAccumulator -= samples.Dequeue();
            }
        }
    }
}
