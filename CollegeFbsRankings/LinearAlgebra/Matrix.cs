using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CollegeFbsRankings.LinearAlgebra
{
    public class Matrix
    {
        private readonly int _n;
        private double[] _data;

        public Matrix(int n)
        {
            _n = n;
            _data = new double[_n * _n];

            for (int i = 0; i < _data.Length; ++i)
                _data[i] = 0.0;
        }

        public double Get(int row, int column)
        {
            return _data[row * _n + column];
        }

        public void Set(int row, int column, double value)
        {
            _data[row * _n + column] = value;
        }

        public int Dimension
        {
            get { return _n; }
        }

        public Matrix LUDecompose()
        {
            var a = new Matrix(_n);
            a._data = _data.ToArray();

            for (int j = 0; j < _n; ++j)
            {
                for (int i = 0; i <= j; ++i)
                {
                    var sum = 0.0;
                    for (int k = 0; k < i; ++k)
                        sum += a.Get(k, j) * a.Get(i, k);
                    a.Set(i, j, a.Get(i, j) - sum);
                }

                for (int i = j + 1; i < _n; ++i)
                {
                    var sum = 0.0;
                    for (int k = 0; k < j; ++k)
                        sum += a.Get(k, j) * a.Get(i, k);
                    a.Set(i, j, (a.Get(i, j) - sum) / a.Get(j, j));
                }
            }

            return a;
        }

        public Vector LUSolve(Vector b)
        {
            var y = new Vector(_n);
            for (int i = 0; i < _n; ++i)
            {
                var sum = 0.0;
                for (int j = 0; j < i; ++j)
                    sum += Get(i, j) * y.Get(j);
                y.Set(i, b.Get(i) - sum);
            }

            var x = new Vector(_n);
            for (int i = _n; i > 0; --i)
            {
                var sum = 0.0;
                for (int j = i; j < _n; ++j)
                    sum += Get(i-1, j) * x.Get(j);
                x.Set(i-1, (y.Get(i-1) - sum) / Get(i-1, i-1));
            }

            return x;
        }
    }
}