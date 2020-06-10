using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Simulator.Backend
{
    public class Math
    {
        //Same as argmax, but with different types for x and f(x)
        private static Tx argmax2<Tx, Tfx>(Func<Tx, Tfx> f, Tx start_, Tx stop_, Tx inc_)
            where Tfx : IComparable
        {
            dynamic start = start_;
            dynamic stop = stop_;
            dynamic inc = inc_;
            Tfx max = f(start);
            Tx maxPoint = start;
            for (Tx x = start; x <= stop; x += inc)
            {
                Tfx y = f(x);
                if (y.CompareTo(max) > 0)
                {
                    max = y;
                    maxPoint = x;
                }
            }
            return maxPoint;
        }

        //Thermal voltage at 300K
        public const double vTherm = 25.85e-3;

        public static void newtonIteration(int n, List<double> x, double[][] m)
        {
            bool atConvergence = true;

            gaussianElimination(n, m);
            double[] delta = new double[n];
            //Back substitution: see report section 2.4.1.5
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = m[i][n];
                for (int j = i + 1; j < n; j++)
                {
                    sum -= delta[j] * m[i][j];
                }
                delta[i] = sum / m[i][i];
            }
            for (int i = 0; i < n; i++)
            {
                x[i] += delta[i];
            }
        }

        //See report section 2.4.1.4
        private static void gaussianElimination(int n, double[][] m)
        {
            //One for each row: a list of non-zeros for each row

            for (int r = 0; r < n; r++)
            {
                int i_max = argmax2<int, double>((int x) => { return System.Math.Abs(m[x][r]); }, r, n - 1, 1);
                if (m[i_max][r] == 0)
                    throw new Exception("Matrix is singular");

                //Swap rows
                double[] tmpRow;
                tmpRow = m[r];
                m[r] = m[i_max];
                m[i_max] = tmpRow;

                for (int i = r+1; i < n; i++)
                {
                    if (m[i][r] != 0)
                    {
                        for (int j = r+1; j < n; j++)
                        {
                            if (m[r][j]!=0)
                            {
                                m[i][j] -= m[r][j] * (m[i][r] / m[r][r]);
                            }
                        }
                        m[i][r] = 0;
                    }
                }
            }
        }



        /*Safer exp function for use in a Newtonian solver - models as linear above a certain point*/
        public static double exp_safe(double x, double limit = 45)
        {
            if (x > limit)
            {
                return System.Math.Exp(limit) * (x - limit + 1);
            }
            else if (x < -limit)
            {
                return System.Math.Exp(-limit) * (x + limit + 1);
            }
            else
            {
                return System.Math.Exp(x);
            }
        }
        /*Derivative of above function*/
        public static double exp_deriv(double x, double limit = 45)
        {
            if (x > limit)
            {
                return System.Math.Exp(limit);
            }
            else if (x < -limit)
            {
                return System.Math.Exp(-limit);
            }
            else
            {
                return System.Math.Exp(x);
            }
        }
    }
}
