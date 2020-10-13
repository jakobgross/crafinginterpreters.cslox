using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private Environment environment = new Environment();

        public class RuntimeError : Exception
        {
            public readonly Token _token;
            public RuntimeError(Token token, string message) : base(message)
            {
                _token = token;
            }
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError e)
            {
                Program.DisplayRuntimeError(e);
            }
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        private string Stringify(object obj)
        {
            if (obj is null) return "nil";
            if (obj is double) return ((double)obj).ToString();
            return obj.ToString();
        }

        public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;
        public object VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expression);

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.Right);
            switch (expr.Operator._type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }
            return null;
        }



        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);
            switch (expr.Operator._type)
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    if ((double)right == 0)
                    {
                        throw new RuntimeError(expr.Operator, "Division by Zero is not allowed");
                    }
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");


                case TokenType.GREATER:
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            }
            return null;
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private bool IsEqual(object a, object b)
        {
            if (a is null && b is null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private bool IsTruthy(object obj)
        {
            if (obj is null) return false;
            if (obj is bool boolObj) return boolObj;
            return true;
        }

        private object Evaluate(Expr expr) => expr.Accept(this);

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            environment.Assign(expr.Name, value);
            return value;
        }
        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);
            if(expr.Operator._type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }
            return Evaluate(expr.Right);
        }
        public object VisitCallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }
        public object VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }
        public object VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }
        public object VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }
        public object VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }
        public object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.Name);
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            throw new NotImplementedException();
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            throw new NotImplementedException();
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if(stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            throw new NotImplementedException();
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if(stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name._lexme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }
    }
}
