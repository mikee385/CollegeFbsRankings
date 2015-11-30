using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CollegeFbsRankings.Experiments
{
    public static partial class Experiment
    {
        private class Vector
        {
            private int _n;
            private double[] _data;

            public Vector(int n)
            {
                _n = n;
                _data = new double[_n];

                for (int i = 0; i < _data.Length; ++i)
                    _data[i] = 0.0;
            }

            public double Get(int index)
            {
                return _data[index];
            }

            public void Set(int index, double value)
            {
                _data[index] = value;
            }

            public int Dimension
            {
                get { return _n; }
            }
        }
    }
}