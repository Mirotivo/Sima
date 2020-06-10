using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Simulator.Backend.DCSolver;

namespace Simulator.Backend
{
    public struct NetConnection
    {
        public Component component;
        public int pin;
    }
    public class Net
    {
        //Net name - must be unique throughout the circuit
        public string NetName;

        //Whether or not net is fixed voltage
        public bool IsFixedVoltage = false;

        //Voltage that the net is fixed at
        public double NetVoltage = 0;

        //Pins the net is connected to
        public List<NetConnection> connections = new List<NetConnection>();

        /*Evaluate a Kirchoff-based function for the currents in the pins connected to the net,
        available for both DC and transient simulations
        (irrelevant for fixed-voltage nets)
        */
        public double DCFunction(DCSolver solver)
        {
            double sum = 0;
            foreach (NetConnection iter in connections)
            {
                sum -= solver.GetPinCurrent(iter.component, iter.pin);
            }
            return sum;
        }

        /*
            Evaluate the partial derivatives for the above functions
        */
        public double DCDerivative(DCSolver solver, VariableIdentifier var)
        {
            if (var.type == VariableType.NET) return 0.0d;
            double deriv = 0.0d;
            foreach (NetConnection iter in connections)
            {
                if (var == iter.component.getComponentVariableIdentifier(iter.pin))
                    deriv += -1;
                if (iter.component == var.component)
                {
                    if (iter.pin == iter.component.GetNumberOfPins() - 1)
                    {
                        if (var.pin < iter.component.GetNumberOfPins() - 1)
                            deriv += 1;
                    }
                }
            }
            return deriv;
        }

        public double TransientFunction(TransientSolver solver)
        {
            return 0.0d;
        }

        public double TransientDerivative(TransientSolver solver, VariableIdentifier var)
        {
            return 0.0d;
        }

        public VariableIdentifier GetNetVariableIdentifier()
        {
            VariableIdentifier id = new VariableIdentifier();
            id.type = VariableType.NET;
            id.net = this;
            return id;
        }
    }
}