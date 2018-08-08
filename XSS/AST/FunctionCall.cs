using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.AST
{
    class FunctionCall : ASTNode
    {
        public string FunctionName;
        public List<ASTNode> Parameters;

        public FunctionCall(string functionName, List<ASTNode> parameters)
        {
            FunctionName = functionName;
            Parameters = parameters;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Value()
        {
            return $"{FunctionName} ({string.Join(", ", Parameters)})";
        }
    }
}
