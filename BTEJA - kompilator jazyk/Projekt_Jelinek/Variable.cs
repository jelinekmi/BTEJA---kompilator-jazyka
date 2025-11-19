using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Projekt_Jelinek
{
    public class Variable
    {
        public string Name { get; }
        public object Value { get; private set; }  // Možnost jen číst hodnotu, ne měnit mimo třídu
        public string Type { get; }

        public Variable(string name, object value, string type)
        {
            Name = name;
            Value = value;
            Type = type;
        }


        // Metoda pro změnu hodnoty s kontrolou typu
        public void SetValue(object newValue)
        {
            if (newValue.GetType().Name != Type)
            {
                throw new InvalidOperationException($"Invalid type. Expected {Type}, but got {newValue.GetType().Name}");
            }

            Value = newValue;
        }

        public override string ToString()
        {
            return $"{Type} {Name} = {Value}";
        }
    }
}