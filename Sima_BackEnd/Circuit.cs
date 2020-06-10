using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Simulator.Backend
{
    public class Circuit
    {
        public Circuit()
        {

        }
        public void ReadNetlist(string data)
        {
            var ss1 = data.Split(new[] { '\r', '\n' });
            foreach (var line in ss1)
            {
                var ss2 = line.Split(new[] { ' ' });
                List<string> parts = new List<string>();
                foreach (var part in ss2)
                {
                    parts.Add(part);
                }
                if (parts.Count >= 2)
                {
                    if (parts[0] == "NET")
                    {
                        Net n = new Net();
                        n.NetName = parts[1];
                        if (parts.Count >= 3)
                        {
                            n.IsFixedVoltage = true;
                            n.NetVoltage = Convert.ToDouble(parts[2]);
                        }
                        Nets.Add(n);
                    }

                    else if (parts[0] == "RES")
                    {
                        AddComponent(parts, new Resistor());
                    }
                    else if (parts[0] == "CAP")
                    {
                        AddComponent(parts, new Capacitor());
                    }
                    else if (parts[0] == "DIODE")
                    {
                        AddComponent(parts, new Diode());
                    }
                    else if (parts[0] == "BJT")
                    {
                        AddComponent(parts, new BJT());
                    }
                    else if (parts[0] == "NMOS")
                    {
                        AddComponent(parts, new NMOS());
                    }
                    else if (parts[0] == "OPAMP")
                    {
                        AddComponent(parts, new Opamp());
                    }
                    else if (parts[0].StartsWith("LOGIC_"))
                    {
                        AddComponent(parts, new LogicGate(parts[0].Substring(6)));
                    }
                    else
                    {
                        Console.WriteLine("WARNING : Unknown component type " + parts[0]);
                    }
                }
            }
        }

        //DCSolver getSolver();
        public void AddComponent(List<string> parts, Component c)
        {
            c.ComponentID = parts[1];
            if (parts.Count >= (c.GetNumberOfPins() + 1))
            {
                for (int i = 0; i < c.GetNumberOfPins(); i++)
                {
                    string netName = parts[i + 2];
                    Net net = new Net();
                    bool foundNet = false;
                    foreach (var a in Nets)
                    {
                        if (a.NetName == netName)
                        {
                            net = a;
                            foundNet = true;
                            break;
                        }
                    }
                    if(!foundNet)
                    {
                        net = new Net();
                        net.NetName = netName;
                        Nets.Add(net);
                    }
                    c.PinConnections.Add(net);
                    NetConnection conn = new NetConnection();
                    conn.component = c;
                    conn.pin = i;
                    net.connections.Add(conn);
                }
            }
            c.SetParameters(new ParameterSet(parts));
            Components.Add(c);
        }
        public List<Net> Nets = new List<Net>();
        public List<Component> Components = new List<Component>();

        //Reports an error to the GUI. If fatal is set to true, then the program will subsequently hang until it is killed by the GUI.
        bool reportedConvergenceFail = false;
        public void ReportError(string desc, bool fatal)
        {
            //Only report a convergence failure once
            if (desc == "CONVERGENCE")
            {
                if (reportedConvergenceFail)
                    return;
                reportedConvergenceFail = true;
            }

            Console.WriteLine();
            Console.WriteLine("ERROR " + (fatal ? 0 : 1) + "," + desc);
            if (fatal)
            {
                while (true) ;
            }
            else
            {
                ContinueFromError = false;
                string str;
                while (!ContinueFromError)
                {
                    Thread.Sleep(100);
                }
            }
        }

        //Set true to continue after an error
        public bool ContinueFromError = false;
    }
}