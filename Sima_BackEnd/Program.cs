using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Simulator.Backend.DCSolver;

namespace Simulator.Backend
{
    class Program
    {
        static Circuit circuit = new Circuit();
        static List<string> lineBuffer = new List<string>();
        static Mutex lineBufferMutex = new Mutex();

        private static void interactiveTick(TransientSolver solver)
        {
            Console.Write("RESULT " + solver.GetTimeAtTick(solver.GetCurrentTick()) + ",");
            for (int i = 0; i < circuit.Nets.Count; i++)
            {
                Console.Write(solver.GetNetVoltage(circuit.Nets[i]) + ",");
            }
            for (int i = 0; i < circuit.Components.Count; i++)
            {
                for (int j = 0; j < circuit.Components[i].GetNumberOfPins(); j++)
                {
                    Console.Write(solver.GetPinCurrent(circuit.Components[i], j) + ",");
                }
            }
            Console.WriteLine();
            lineBufferMutex.WaitOne();
            foreach (var line in lineBuffer)
            {
                List<string> parts = new List<string>();
                foreach (string part in line.Split(new[] { ' ' }))
                {
                    parts.Add(part);
                }
                if (parts.Count > 2)
                {
                    if (parts[0] == "CHANGE")
                    {
                        foreach (Component c in circuit.Components)
                        {
                            if (c.ComponentID == parts[1])
                            {
                                c.SetParameters(new ParameterSet(parts));
                            }
                        }
                    }
                }
            }
            lineBuffer.Clear();
            lineBufferMutex.ReleaseMutex();
        }
        private static void iothread()
        {
            string line;
            while (true)
            {
                line = Console.ReadLine();
                if (line == "CONTINUE")
                {
                    circuit.ContinueFromError = true;
                }
                else
                {
                    lineBufferMutex.WaitOne();
                    lineBuffer.Add(line);
                    lineBufferMutex.ReleaseMutex();
                }
            }
        }

        static void Main(string[] args)
        {
            string line = "";
            string netlist = "";
            double simSpeed = 0;
            while (true)
            {
                line = Console.ReadLine();
                if (line.Contains("START"))
                {
                    simSpeed = Convert.ToDouble(line.Substring(6));
                    break;
                }
                netlist += line;
                netlist += "\n";
            }

            circuit.ReadNetlist(netlist);
            Console.Write("VARS t,");

            for (int i = 0; i < circuit.Nets.Count; i++)
            {
                Console.Write("V(" + circuit.Nets[i].NetName + "),");
            }
            for (int i = 0; i < circuit.Components.Count; i++)
            {
                for (int j = 0; j < circuit.Components[i].GetNumberOfPins(); j++)
                {
                    Console.Write("I(" + circuit.Components[i].ComponentID + "." + j + "),");
                }
            }
            Console.WriteLine();

            DCSolver solver = new DCSolver(circuit);
            bool result = false;
            try
            {
                result = solver.Solve();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to obtain initial operating point");
                circuit.ReportError("EXCEPTION", true);
            }
            if (!result)
            {
                circuit.ReportError("CONVERGENCE", false);
            }

            TransientSolver tranSolver = new TransientSolver(solver);
            tranSolver.InteractiveCallback = interactiveTick;
            Thread updaterThread = new Thread(new ThreadStart(iothread));
            updaterThread.Start();
            tranSolver.RunInteractive(simSpeed);
        }
    }
}
