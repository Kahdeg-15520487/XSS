using System;

namespace simple_interpreter.Utility
{
    public static class TypeSwitch
    {
        public enum CaseType
        {
            Case,
            Default,
            Finally
        }

        public class CaseInfo
        {
            public CaseType CaseType { get; set; }
            public Type Target { get; set; }
            public Action<object> Action { get; set; }
        }

        public static void Do(object source, params CaseInfo[] cases)
        {
            var type = source.GetType();
            foreach (var c in cases)
            {
                if (c.CaseType == CaseType.Default || (c.CaseType == CaseType.Case && c.Target.IsAssignableFrom(type)))
                {
                    c.Action(source);
                    break;
                }
            }

            if (cases[cases.Length - 1].CaseType == CaseType.Finally)
            {
                cases[cases.Length - 1].Action(source);
            }
        }

        public static CaseInfo Case<T>(Action action)
        {
            return new CaseInfo()
            {
                Action = x => action(),
                Target = typeof(T),
                CaseType = CaseType.Case
            };
        }

        public static CaseInfo Case<T>(Action<T> action)
        {
            return new CaseInfo()
            {
                Action = (x) => action((T)x),
                Target = typeof(T),
                CaseType = CaseType.Case
            };
        }

        public static CaseInfo Default(Action action)
        {
            return new CaseInfo()
            {
                Action = x => action(),
                CaseType = CaseType.Default
            };
        }

        public static CaseInfo Finally(Action action)
        {
            return new CaseInfo()
            {
                Action = x => action(),
                CaseType = CaseType.Finally
            };
        }
    }
}
