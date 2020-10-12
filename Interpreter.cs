﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox
{
    public class Interpreter : Expr.IVisitor<object>
    {
        public class RuntimeError : Exception
        {
            public readonly Token _token;
            public RuntimeError(Token token, string message): base(message)
            {
                _token = token;
            }
        }

        public void interpret(Expr expression)
        {
            try
            {
                object value = evaluate(expression);
                Console.WriteLine(stringify(value));
            }
            catch(RuntimeError e)
            {
                Program.runtimeError(e);
            }
        }

        private string stringify(object obj)
        {
            if(obj is null) return "nil";
            if (obj is double) return ((double)obj).ToString();
            return obj.ToString();
        }

        public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;
        public object VisitGroupingExpr(Expr.Grouping expr) => evaluate(expr.Expression);

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = evaluate(expr.Right);
            switch (expr.Operator._type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !isTruthy(right);
            }
            return null;
        }

        

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = evaluate(expr.Left);
            object right = evaluate(expr.Right);
            switch (expr.Operator._type)
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator,left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    if((double) right == 0)
                    {
                        throw new RuntimeError(expr.Operator, "Division by Zero is not allowed");
                    }
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if(left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }
                    if(left is double && right is double)
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
                case TokenType.BANG_EQUAL: return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL: return isEqual(left, right);
            }
            return null;
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private bool isEqual(object a, object b)
        {
            if (a is null && b is null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private bool isTruthy(object obj)
        {
            if (obj is null) return false;
            if (obj is bool boolObj) return boolObj;
            return true;
        }

        private object evaluate(Expr expr) => expr.Accept(this);

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op,"Operand must be a number.");
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            throw new NotImplementedException();
        }
        public object VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}