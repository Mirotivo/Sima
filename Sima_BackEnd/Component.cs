using System;
using System.Collections.Generic;
using System.Linq;
using static Simulator.Backend.DCSolver;

namespace Simulator.Backend
{
    public class Component
    {
        //Number of pins that the component has
        public virtual int GetNumberOfPins()
        {
            return 0;
        }

        //Return a type string identifying the component (e.g. R for resistor, AC for AC source)
        public virtual string GetComponentType()
        {
            return "0";
        }

        //Reference designator, e.g. R1 or D3
        public string ComponentID;

        //List of nets the component is connected to, identified by pin number
        public List<Net> PinConnections = new List<Net>();

        /*
        Returns a function that can be used by the non-linear DC operating point solver
        to solve as f(x) = 0
        For components with greater than 2 pins, f is the indentifier of the function 0 <= f < n-1
        */
        public virtual double DCFunction(DCSolver solver, int f)
        {
            return 0.0d;
        }

        /*
        Returns a function that can be used by the non-linear transient solver
        to solve as f(x) = 0
        For components with greater than 2 pins, f is the indentifier of the function 0 <= f < n-1
        */
        public virtual double TransientFunction(TransientSolver solver, int f)
        {
            return 0.0d;
        }

        /*
        Evaluate the partial derivatives for the above functions
        */
        public virtual double DCDerivative(DCSolver solver, int f, VariableIdentifier var)
        {
            return 0.0d;
        }
        public virtual double TransientDerivative(TransientSolver solver, int f, VariableIdentifier var)
        {
            return 0.0d;
        }

        /*
        Get the identifier for the current variable for a pin
        */
        public VariableIdentifier getComponentVariableIdentifier(int pin)
        {
            VariableIdentifier id = new VariableIdentifier();
            id.type = VariableType.COMPONENT;
            id.component = this;
            id.pin = pin;
            return id;
        }

        /*
        Initialise component parameters from a parameter set
        */
        public virtual void SetParameters(ParameterSet paramSet)
        {

        }
    }


    //Discrete Semiconductors
    //DiscreteSemis
    public class Diode : Component
    {
        private double SaturationCurrent = 1e-14; //Saturation current
        private double IdealityFactor = 1; //Ideality factor (1 for an ideal diode) 
        private double SeriesResistance = 0; //Series resistance

        public override string GetComponentType()
        {
            return "D";
        }

        public override int GetNumberOfPins()
        {
            return 2;
        }
        // f0: Is * (e ^ ((Vd - IRs)/(n*Vt)) - 1) - I
        public override double DCFunction(DCSolver solver, int f)
        {
            if (f == 0)
            {
                double L = SaturationCurrent *
                    (Math.exp_safe(((solver.GetNetVoltage(PinConnections[0]) - solver.GetNetVoltage(PinConnections[1])) - SeriesResistance * solver.GetPinCurrent(this, 0)) / (IdealityFactor * Math.vTherm)) - 1);
                double R = solver.GetPinCurrent(this, 0);
                return L - R;
            }
            else
            {
                return 0.0d;
            }
        }
        public override double DCDerivative(DCSolver solver, int f, VariableIdentifier var)
        {
            if (f == 0)
            {
                if (var.type == VariableType.NET)
                {
                    if (var.net == PinConnections[0])
                    {
                        return SaturationCurrent * (1 / (IdealityFactor * Math.vTherm)) * Math.exp_deriv(((solver.GetNetVoltage(PinConnections[0]) - solver.GetNetVoltage(PinConnections[1]) - SeriesResistance * solver.GetPinCurrent(this, 0))) / (IdealityFactor * Math.vTherm));
                    }
                    else if (var.net == PinConnections[1])
                    {
                        return -1 * SaturationCurrent * (1 / (IdealityFactor * Math.vTherm)) * Math.exp_deriv(((solver.GetNetVoltage(PinConnections[0]) - solver.GetNetVoltage(PinConnections[1]) - SeriesResistance * solver.GetPinCurrent(this, 0))) / (IdealityFactor * Math.vTherm));
                    }
                }
                else
                {
                    if ((var.component == this) && (var.pin == 0))
                    {
                        return -1 * SeriesResistance * SaturationCurrent * (1 / (IdealityFactor * Math.vTherm)) * Math.exp_deriv(((solver.GetNetVoltage(PinConnections[0]) - solver.GetNetVoltage(PinConnections[1]) - SeriesResistance * solver.GetPinCurrent(this, 0))) / (IdealityFactor * Math.vTherm)) - 1;
                    }
                }
            }
            return 0.0d;
        }
    }
    public class BJT : Component { }
    public class NMOS : Component { }

    //Digital ICs
    //LogicGates
    public class LogicGate : Component
    {
        public LogicGate(string type)
        {

        }
    }

    //Analog ICs
    //Opamp
    public class Opamp : Component { }

    //PassiveComponents
    public class Capacitor : Component { }
    public class Resistor : Component 
    {
        private double Resistance = 0; //Resistance in ohms
        public override string GetComponentType()
        {
            return "R";
        }
        public override int GetNumberOfPins()
        {
            return 2;
        }

        public override void SetParameters(ParameterSet paramSet) {
            Resistance = paramSet.getDouble("res", Resistance);
            //std::cerr << "res of " << ComponentID << " is " << resistance << std::endl;
        }

        // f0: (V1 - V2) / R - I
        public override double DCFunction(DCSolver solver, int f)
        {
            if (f == 0)
            {
                double L = (solver.GetNetVoltage(PinConnections[0]) - solver.GetNetVoltage(PinConnections[1])) / Resistance;
                double R = solver.GetPinCurrent(this, 0);
                return L - R;
            }
            else
            {
                return 0.0d;
            }
        }
        public override double DCDerivative(DCSolver solver, int f, VariableIdentifier var)
        {
            if (f == 0)
            {
                if (var.type == VariableType.NET)
                {
                    if (var.net == PinConnections[0]) return 1 / Resistance;
                    if (var.net == PinConnections[1]) return -1 / Resistance;
                }
                else
                {
                    if ((var.component == this) && (var.pin == 0)) return -1;
                }
            }
            return 0.0d;
        }
    }
}