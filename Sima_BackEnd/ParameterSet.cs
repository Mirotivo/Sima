using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulator.Backend
{
    public class ParameterSet
    {
        public Dictionary<string, string> paramSet = new Dictionary<string, string>();
        public ParameterSet(List<string> parts)
        {
            foreach (string a in parts)
            {
                int delpos = a.IndexOf('=');
                if (delpos != -1)
                {
                    string before = a.Substring(0, delpos).ToLower();
                    string after = a.Substring(delpos + 1).ToLower();
                    paramSet[before] = after;
                }
            }
        }
        public string getString(string key, string defaultValue)
        {
            int a = paramSet.Values.ToList().IndexOf(key.ToLower());
            if (a != paramSet.Count)
            {
                return Convert.ToString(a);
            }
            else
            {
                return defaultValue;
            }
        }
        public double getDouble(string key, double defaultValue = 0)
        {
            int a = paramSet.Values.ToList().IndexOf(key.ToLower());
            if (a != paramSet.Count)
            {
                return Convert.ToDouble(a);
            }
            else
            {
                return defaultValue;
            }
        }
    }
}