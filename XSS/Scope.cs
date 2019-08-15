using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS
{
    class Scope : IEnumerable<KeyValuePair<string, object>>
    {
        Dictionary<string, object> Variable;
        public Scope parent { get; set; } = null;
        public bool IsGlobal => this.parent == null;
        public int NestLevel => this.parent == null ? 0 : this.parent.NestLevel + 1;

        public Scope(Scope parent = null)
        {
            this.parent = parent;
            this.Variable = new Dictionary<string, object>();
        }

        public void Clear()
        {
            this.Variable.Clear();
        }

        public object Define(string var, object init = null)
        {
            if (!this.Variable.ContainsKey(var))
            {
                this.Variable.Add(var, init);
                return this.Variable[var];
            }
            else
            {
                return null;
            }
        }

        public void Undefine(string var)
        {
            if (!this.Variable.ContainsKey(var))
            {
                throw new Exception($"{var} is undefined");
            }
            else
            {
                this.Variable.Remove(var);
            }
        }

        public object Get(string var)
        {
            if (this.Variable.ContainsKey(var))
            {
                Console.WriteLine("getting var {0} from scope level {1}", var, this.NestLevel);
                return this.Variable[var];
            }
            else if (this.parent != null)
            {
                Console.WriteLine("getting var {0} from scope global", var);
                return this.parent.Get(var);
            }
            else
            {
                return null;
            }
        }

        public object Assign(string var, object value)
        {
            if (this.Variable.ContainsKey(var))
            {
                this.Variable[var] = value;
                return this.Variable[var];
            }
            else if (this.parent != null)
            {
                return this.parent.Assign(var, value);
            }
            else
            {
                return null;
            }
        }

        internal bool Contain(string varname, bool localOnly = false)
        {
            return this.Variable.ContainsKey(varname) || (!localOnly && this.parent != null && this.parent.Contain(varname, localOnly));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Variable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
