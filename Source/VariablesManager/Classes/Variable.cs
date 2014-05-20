using System;

namespace ArmoSystems.ArmoGet.VariablesManager.Classes
{
    internal struct Variable
    {
        
        private readonly string value;

        public Variable( string name, string value, int priority ) : this()
        {
            this.value = value;
            Name = name;
            Priority = priority;
        }

        public int Priority { get; private set; }
        public string Name { get; private set; }

        public void SetEnviromentVariable()
        {
            if ( Environment.GetEnvironmentVariable( Name, VariablesManager.VariableTarget ) != value )
                Environment.SetEnvironmentVariable( Name, value, VariablesManager.VariableTarget );
        }
    }
}