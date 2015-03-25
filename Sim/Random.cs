using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sim
{
    public sealed class Random
    {
        private static volatile Random _instance;
        private static readonly object SyncRoot = new Object();

        private readonly System.Random _random = new System.Random(1); // Guid.NewGuid().GetHashCode()

        private Random() { }

        public static Random Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new Random();
                    }
                }

                return _instance;
            }
        }

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public T Next<T>()
        {
            var values = Enum.GetValues(typeof(T));
            var randomState = (T)values.GetValue(_random.Next(values.Length));
            return randomState;
        }

        public T Next<T>(IEnumerable<T> list)
        {
            return list.OrderBy(x=>Guid.NewGuid()).Take(1).FirstOrDefault();
        }
    }
 
}
