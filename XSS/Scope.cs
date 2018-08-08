using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS
{
    class Scope
    {
        Dictionary<string, object> Variable;
        Scope parent = null;
        public Scope(Scope parent = null)
        {
            this.parent = parent;
            Variable = new Dictionary<string, object>();
        }

        public object Define(string var, object init = null)
        {
            if (!Variable.ContainsKey(var))
            {
                Variable.Add(var, init);
                return Variable[var];
            }
            else
            {
                return null;
            }
        }

        public object Get(string var)
        {
            if (Variable.ContainsKey(var))
            {
                return Variable[var];
            }
            else if(parent != null)
            {
                return parent.Get(var);
            }
            else
            {
                return null;
            }
        }

        public object Assign(string var,object value)
        {
            if (Variable.ContainsKey(var))
            {
                Variable[var] = value;
                return Variable[var];
            }
            else if (parent != null)
            {
                return parent.Assign(var,value);
            }
            else
            {
                return null;
            }
        }

        internal bool Contain(string varname,bool localOnly = true)
        {
            return Variable.ContainsKey(varname);
        }
    }
}
