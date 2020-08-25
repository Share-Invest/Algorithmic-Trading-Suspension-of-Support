﻿using System;

namespace ShareInvest.Analysis.SecondaryIndicators
{
    class PeriodicSpline : SplineInterpolator
    {
        void CalcParameters()
        {
            int i;

            for (i = 0; i < n; i++)
                a[i] = givenYs[i];

            for (i = 0; i < n - 1; i++)
                h[i] = givenXs[i + 1] - givenXs[i];

            a[n] = givenYs[1];
            h[n - 1] = h[0];

            for (i = 0; i < n - 1; i++)
                for (int k = 0; k < n - 1; k++)
                {
                    m.a[i, k] = 0.0;
                    m.y[i] = 0.0;
                    m.x[i] = 0.0;
                }
            for (i = 0; i < n - 1; i++)
            {
                if (i == 0)
                {
                    m.a[i, 0] = 2.0 * (h[0] + h[1]);
                    m.a[i, 1] = h[1];
                }
                else
                {
                    m.a[i, i - 1] = h[i];
                    m.a[i, i] = 2.0 * (h[i] + h[i + 1]);

                    if (i < n - 2)
                        m.a[i, i + 1] = h[i + 1];
                }
                if ((h[i] != 0.0) && (h[i + 1] != 0.0))
                    m.y[i] = ((a[i + 2] - a[i + 1]) / h[i + 1] - (a[i + 1] - a[i]) / h[i]) * 3.0;

                else
                    m.y[i] = 0.0;
            }
            m.a[0, n - 2] = h[0];
            m.a[n - 2, 0] = h[0];

            if (gauss.Eliminate() == false)
                throw new InvalidOperationException();

            gauss.Solve();

            for (i = 1; i < n; i++)
                c[i] = m.x[i - 1];

            c[0] = c[n - 1];

            for (i = 0; i < n; i++)
                if (h[i] != 0.0)
                {
                    d[i] = 1.0 / 3.0 / h[i] * (c[i + 1] - c[i]);
                    b[i] = 1.0 / h[i] * (a[i + 1] - a[i]) - h[i] / 3.0 * (c[i + 1] + 2 * c[i]);
                }
        }
        internal PeriodicSpline(double[] xs, double[] ys, int resolution = 10) : base(xs, ys, resolution)
        {
            m = new Matrix(n - 1);
            gauss = new MatrixSolver(n - 1, m);
            a = new double[n + 1];
            b = new double[n + 1];
            c = new double[n + 1];
            d = new double[n + 1];
            h = new double[n];
            CalcParameters();
            Integrate();
            Interpolate();
        }
    }
}