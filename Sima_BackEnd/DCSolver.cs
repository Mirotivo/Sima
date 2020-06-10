using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.InteropServices;
using static Simulator.Backend.DCSolver;

namespace Simulator.Backend
{
    public enum VariableType
    {
        COMPONENT,
        NET
    }
    public struct VariableIdentifier
    {
        public VariableType type;
        public Component component;
        public int pin;
        public Net net;

        public static bool operator ==(VariableIdentifier first, VariableIdentifier other)
        {
            if (first.type == other.type)
            {
                if (first.type == VariableType.COMPONENT)
                {
                    return ((first.component == other.component) && (first.pin == other.pin));
                }
                else
                {
                    return (first.net == other.net);
                }
            }
            return false;
        }
        public static bool operator !=(VariableIdentifier first, VariableIdentifier other)
        {
            return !(first == other);
        }
    }
    public delegate void fnTickCallback(TransientSolver t);
    public class DCSolver
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public class TransientSolver
        {
            public TransientSolver()
            {

            }
            public TransientSolver(DCSolver init)
            {
                NetVariables = init.NetVariables;
                ComponentVariables = init.ComponentVariables;
                VariableData = init.VariableData;
                VariableValues.Push(init.VariableValues);
                SolverCircuit = init.SolverCircuit;
                times.Push(0);
            }

            //Add components and nets to be included in the solver
            public void AddComponent(Component c)
            {
                //Due to Kirchoff's Laws, for a component with n pins we only need n-1 equations

                ComponentVariables[c] = nextFreeVariable;
                nextFreeVariable += c.GetNumberOfPins() - 1;

                for (int i = 0; i < c.GetNumberOfPins() - 1; i++)
                {
                    VariableValues[0].Add(0);
                    VariableIdentifier id = new VariableIdentifier();
                    id.type = VariableType.COMPONENT;
                    id.component = c;
                    id.pin = i;
                    VariableData[ComponentVariables[c] + i] = id;
                }
            }
            public void AddNet(Net net)
            {
                NetVariables[net] = nextFreeVariable;
                VariableValues.PopFirst().Add(0);
                VariableIdentifier id = new VariableIdentifier();
                id.net = net;
                VariableData[nextFreeVariable] = id;
                nextFreeVariable++;
            }

            //Run a single Newton-Raphson solve 'tick'
            public int Tick(double tol = 1e-6, int maxIter = 50, bool? convergenceFailureFlag = null)
            {
                return 0;
            }

            //Run the solver in interactive mode
            public void RunInteractive(double simSpeed, double tol = 1e-6, int maxIter = 100)
            {
                double currentTime = 0;
                ////In order to see timestep recommendations and initialise stateful components, run a timestep at 0s - but discard it, as the steady state represents the initial conditions
                bool firstRun = true;
                bool running = true;
                int ticktimestoAvg = 1200;
                //Deque<double> ticktimes = new Deque<double>();
                while (running)
                {
                    //    currentTick++;
                    //    nextTimestep = simSpeed / 10;
                    //    if (!firstRun)
                    //    {
                    //        nextTimestep = simSpeed * averageTickTime;
                    //    }
                    //    //VariableValues.Push(VariableValues[currentTick - 1]);
                    //    times.Push(currentTime);

                    //    if (VariableValues.Count > bufferSize)
                    //    {
                    //        VariableValues.PopFirst();
                    //        times.PopFirst();
                    //        currentTick--;
                    //    }

                    //    long startT, endT, elapseduS;
                    //    long freq;

                    //    QueryPerformanceFrequency(out freq);
                    //    QueryPerformanceCounter(out startT);

                    //    bool convergenceFailure = false;
                    //    if (firstRun)
                    //    {
                    //        try
                    //        {
                    //            Tick(tol, maxIter);
                    //        }
                    //        catch (Exception e)
                    //        {

                    //        }
                    //    }
                    //    else
                    //    {
                    //        try
                    //        {
                    //            Tick(tol, maxIter, convergenceFailure);
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            Console.WriteLine("RUNTIME ERROR AT T=" + currentTime + " : " + e.Message);
                    //            SolverCircuit.ReportError("EXCEPTION", true);
                    //            running = false;
                    //        }
                    //    }

                    //    if (!firstRun)
                    //    {
                    //        if (((clock() - lastUpdateTime) / ((double)CLOCKS_PER_SEC)) > 2e-3)
                    //        {
                                if (InteractiveCallback != null)
                                {
                                    InteractiveCallback(this);
                                }
                    //            lastUpdateTime = clock();
                    //        }
                    //    }

                    //    QueryPerformanceCounter(out endT);
                    //    while (((endT.QuadPart - startT.QuadPart) / ((double)freq.QuadPart)) < 1e-4) QueryPerformanceCounter(out endT);
                    //    //std::cerr << ((endT.QuadPart - startT.QuadPart) / ((double)freq.QuadPart)) << std::endl;
                    //    elapseduS.QuadPart = endT.QuadPart - startT.QuadPart;
                    //    elapseduS.QuadPart *= 1000000;
                    //    elapseduS.QuadPart /= freq.QuadPart;

                    //    //Recalculate tick time 
                    //    double timeForTick = elapseduS.QuadPart / 1000000.0;
                    //    ticktimes.Push(timeForTick);
                    //    if (ticktimes.Count > ticktimestoAvg)
                    //    {
                    //        ticktimes.PopFirst();
                    //    }
                    //    double ttsum = 0;
                    //    foreach (double iter in ticktimes)
                    //    {
                    //        ttsum += iter;
                    //    }
                    //    averageTickTime = ttsum / ticktimes.Count;

                    //    totalNumberOfTicks++;
                    //    if ((totalNumberOfTicks % 30) == 0)
                    //    {
                    //        Console.WriteLine(averageTickTime);
                    //    }
                    //    currentTime += nextTimestep;
                    //    if (convergenceFailure)
                    //        SolverCircuit.ReportError("CONVERGENCE", false);

                    //    if (firstRun)
                    //    {
                    //        VariableValues.Remove(VariableValues.PopLast() - 1);
                    //        times.Remove(times.PopLast() - 1);
                    //        currentTick--;
                    //        firstRun = false;
                    //    }
                }
            }

            /*
            Performs a DC 'ramp-up' simulation. Initial operating point must have all fixed voltage nets at 0V
            Returns whether or not successful
            */
            public int RampUp(Dictionary<Net, double> originalVoltages, double tol = 1e-12, int maxIter = 400)
            {
                return 1;
            }

            //Get value of a net voltage at current point in solve routine, given the tick number (-1 for current time)
            public double GetNetVoltage(Net net, int n = -1)
            {
                if (n == -1) n = currentTick;
                if (net.IsFixedVoltage)
                {
                    return net.NetVoltage;
                }
                else
                {
                    return VariableValues[n][NetVariables[net]];
                }
            }

            //Get value of current going INTO a pin at current point in solve routine given the tick number
            public double GetPinCurrent(Component c, int pin, int n = -1)
            {
                if (n == -1) n = currentTick;
                if (pin < c.GetNumberOfPins() -1)
                {
                    NetConnection conn = new NetConnection();
                    conn.component = c;
                    conn.pin = pin;
                    return VariableValues[n][ComponentVariables[c] + pin];
                }
                else
                {
                    double sum = 0;
                    for (int i = 0; i < c.GetNumberOfPins() - 1; i++)
                    {
                        sum += GetPinCurrent(c, i, n);
                    }
                    return -sum;
                }
            }

            //Get current tick number
            public int GetCurrentTick()
            {
                return 0;
            }

            //Get time that a given tick occurred
            public double GetTimeAtTick(int n)
            {
                return 0.0d;
            }

            //To be called by components, to recommend the next timestep
            public void RequestTimestep(double deltaT)
            {

            }

            //Clears all results
            public void Reset()
            {

            }

            //Get variable value given ID and tick
            public double GetVarValue(int id, int tick = -1)
            {
                if (tick == -1) tick = currentTick;
                return VariableValues[tick][id];
            }

            //This function is called after an interactive simulation tick
            public fnTickCallback InteractiveCallback = null;

            //Sets the guess value for a net voltage
            public void SetNetVoltageGuess(Net net, double value)
            {
                VariableValues[currentTick][NetVariables[net]] = value;
            }

            private int nextFreeVariable = 0;
            private double nextTimestep = 0;
            private int currentTick = 0;
            private int totalNumberOfTicks = 0;
            private double averageTickTime = 0;

            private Dictionary<Net, int> NetVariables; //Map pointers to nets to net voltage variable IDs

            //Note that there are n-1 (where n=number of pins) variables allocated to each component
            private Dictionary<Component, int> ComponentVariables; //Map component pins to pin current variable IDs

            private Dictionary<int, VariableIdentifier> VariableData; //Allow variables to be looked up

            private Deque<List<double>> VariableValues = new Deque<List<double>>(); //Map variable IDs to values at a given tick

            private Deque<double> times = new Deque<double>(); //Time at each tick

            private const int bufferSize = 10000; //During interactive simulations, number of components to store

            //Max time for single tick
            private const double maxTickTime = 0.4;

            private Circuit SolverCircuit;
        }

        public DCSolver(Circuit circuit)
        {
            SolverCircuit = circuit;
            foreach (Net net in circuit.Nets)
            {
                AddNet(net);
            }
            foreach (Component c in circuit.Components)
            {
                AddComponent(c);
            }
        }

        //Run a solve routine, returning whether or not successful
        public bool Solve(double tol = 1e-8, int maxIter = 200, bool attemptRamp = true)
        {
            int n = VariableValues.Count;
            double worstTol = 0;
            //The matrix to solve by Gaussian elimination for the next Newton-Raphson iteration, the rows representing functions. The first n-1 columns are
            //the Jacobian matrix of partial derivatives (each column representing a variable), and the final column is the value of -f(x) for that function
            //This is solved to find the values of (x_n+1 - x_n)
            double[][] matrix = new double[n][];
            int i;
            for (i = 0; i < n; i++) matrix[i] = new double[n + 1];

            for (i = 0; i < maxIter; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    VariableIdentifier varData = VariableData[j];
                    if (varData.type == VariableType.COMPONENT)
                    {
                        //Set the value of -f(x)
                        matrix[j][n] = -varData.component.DCFunction(this, varData.pin);
                        //Populate the matrix of derivatives
                        for (int k = 0; k < n; k++)
                        {
                            matrix[j][k] = varData.component.DCDerivative(this, varData.pin, VariableData[k]);
                        }
                    }
                    else
                    {
                        //Set the value of -f(x)
                        matrix[j][n] = -varData.net.DCFunction(this);
                        //Populate the matrix of derivatives
                        for (int k = 0; k < n; k++)
                        {
                            matrix[j][k] = varData.net.DCDerivative(this, VariableData[k]);

                        }

                    }
                }


                worstTol = 0;
                for (int j = 0; j < n; j++)
                {
                    if (System.Math.Abs(matrix[j][n]) > worstTol)
                        worstTol = System.Math.Abs(matrix[j][n]);
                }
                if (worstTol < tol) break;
                //Call the Newton-Raphson solver, which updates VariableValues with their new values
                Math.newtonIteration(n, VariableValues, matrix);
            }

            //If conventional Newton's method solution to find the operating point fails
            //Fixed nets are ramped up from zero volts to full in 10% steps in an attempt to find the operating point
            //This works to prevent convergence failures in unstable circuits such as oscillators
            if ((i==maxIter) && (worstTol > 1))
            {
                if(attemptRamp)
                {
                    Console.WriteLine("WARNING: DC simulation failed to converge (error=" + worstTol + ")");
                    Dictionary<Net, double> netVoltages = new Dictionary<Net, double>();
                    foreach (Net net in SolverCircuit.Nets)
                    {
                        if ((net.IsFixedVoltage) && (net.NetVoltage != 0))
                        {
                            netVoltages[net] = net.NetVoltage;
                            net.NetVoltage = 0;
                        }
                        for (int j = 0; j < VariableValues.Count; j++)
                        {
                            VariableValues[j] = 0;
                        }
                        Solve(tol, maxIter, false);
                        TransientSolver rampSolver = new TransientSolver(this);
                        int ticks;
                        ticks = rampSolver.RampUp(netVoltages);
                        for (int j = 0; j < VariableValues.Count; j++)
                        {
                            VariableValues[j] = rampSolver.GetVarValue(j, ticks - 1);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: DC Ramp analysis OP failed to converge (error=" + worstTol + ")");
                    return false;
                }
            }
            return true;
        }

        //Get value of a net voltage at current point in solve routine
        public double GetNetVoltage(Net n)
        {
            if (n.IsFixedVoltage)
            {
                return n.NetVoltage;
            }
            else
            {
                return VariableValues[NetVariables[n]];
            }
        }

        //Get value of current going INTO a pin at current point in solve routine
        public double GetPinCurrent(Component c, int pin)
        {
            if (pin < c.GetNumberOfPins() - 1)
            {
                NetConnection conn = new NetConnection();
                conn.component = c;
                conn.pin = pin;
                return VariableValues[ComponentVariables[c] + pin];
            }
            else
            {
                double sum = 0.0d;
                for (int i = 0; i < c.GetNumberOfPins() - 1; i++)
                {
                    sum += GetPinCurrent(c, i);
                }
                return -sum;
            }
        }
        public Circuit SolverCircuit;

        private int nextFreeVariable = 0;

        //Add components and nets to be included in the solver
        private void AddComponent(Component c)
        {
            //Due to Kirchoff's Laws, for a component with n pins we only need n-1 equations

            ComponentVariables[c] = nextFreeVariable;
            nextFreeVariable += c.GetNumberOfPins() - 1;
            for (int i = 0; i < c.GetNumberOfPins() -1; i++)
            {
                VariableValues.Add(0.1);
                VariableIdentifier id = new VariableIdentifier();
                id.type = VariableType.COMPONENT;
                id.component = c;
                id.pin = i;
                VariableData[ComponentVariables[c] + i] = id;
            }
        }
        private void AddNet(Net net)
        {
            if (!net.IsFixedVoltage)
            {
                NetVariables[net] = nextFreeVariable;
                VariableValues.Add(0.1);
                VariableIdentifier id = new VariableIdentifier();
                id.type = VariableType.NET;
                id.net = net;
                VariableData[nextFreeVariable] = id;
                nextFreeVariable++;
            }
        }


        private Dictionary<Net, int> NetVariables = new Dictionary<Net, int>(); //Map pointers to nets to net voltage variable IDs

        //Note that there are n-1 (where n=number of pins) variables allocated to each component
        private Dictionary<Component, int> ComponentVariables = new Dictionary<Component, int>(); //Map component pins to pin current variable IDs

        private Dictionary<int, VariableIdentifier> VariableData = new Dictionary<int, VariableIdentifier>(); //Allow variables to be looked up

        private List<double> VariableValues = new List<double>(); //Map variable IDs to values

    }
}