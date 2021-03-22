using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.Utility
{
    public class NativeCommand
    {
        public NativeCommand(string verb, params string[] parameters)
        {
            this.Verb = verb;
            this.Parameters = parameters;
        }

        public string Verb { get; set; }
        public string[] Parameters { get; set; }

        public static NativeCommand Parse(string cmd)
        {
            string[] raws = cmd.Trim().Split(' ');
            raws[0] = raws[0].Remove(0, 1);
            string verb = raws[0];
            string[] parameters = raws.Skip(1).ToArray();
            return new NativeCommand(verb, parameters);
        }
    }
}
