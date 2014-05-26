using System;

namespace ArmoSystems.ArmoGet.VariablesManager.Classes
{
    internal sealed class Variable
    {
        public Variable( string name, string value, int priority )
        {
            Value = value;
            Name = name;
            Priority = priority;
        }

        public string Value { get; set; }

        public int Priority { get; private set; }
        public string Name { get; private set; }

        public void SetEnviromentVariable()
        {
            if ( Environment.GetEnvironmentVariable( Name, VariablesManager.VariableTarget ) != Value )
                Environment.SetEnvironmentVariable( Name, Value, VariablesManager.VariableTarget );
        }
    }
}