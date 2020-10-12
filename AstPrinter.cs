using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static crafinginterpreters.cslox.Expr;

namespace crafinginterpreters.cslox
{
    internal class AstPrinter : Expr.IVisitor<string>
    {
        public string PrintExpr(Expr expr) => expr.Accept(this);

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Operator._lexme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("Group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Operator._lexme, expr.Right);
        }

        private string Parenthesize(string name, params Expr[] expressions)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in expressions)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.Operator._lexme, expr.Left, expr.Right);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return $"{expr.Name._lexme} = {PrintExpr(expr.value)}";
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return expr.Name._lexme;
        }
        public string VisitCallExpr(Expr.Call expr)
        {
            string arguments = string.Join(", ", expr.Arguments.Select(e => PrintExpr(e)));

            return $"{PrintExpr(expr.Callee)}({arguments})";
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            return $"{PrintExpr(expr.Object)}.{expr.Name._lexme}";
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            return $"{PrintExpr(expr.Object)}.{expr.Name._lexme}";
        }

        public string VisitThisExpr(Expr.This expr)
        {
            return expr.Keyword._lexme;
        }

        public string VisitSuperExpr(Super expr)
        {
            return "super";
        }
    }
}
