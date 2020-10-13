﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace crafinginterpreters.cslox
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitClassStmt(Class stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitFunctionStmt(Function stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitReturnStmt(Return stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
        }
        public abstract R Accept<R>(IVisitor<R> visitor);

        public class Block : Stmt
        {
            public readonly List<Stmt> statements;

            public Block(List<Stmt> statements)
            {
                this.statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

        }

        public class Class : Stmt
        {
            public readonly Token name;
            public readonly Expr.Variable superclass;
            public readonly List<Stmt.Function> methods;

            public Class(Token name, Expr.Variable superclass, List<Stmt.Function> methods)
            {
                this.name = name;
                this.superclass = superclass;
                this.methods = methods;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitClassStmt(this);
            }
        }

        public class Expression : Stmt
        {
            public readonly Expr expression;
            public Expression(Expr expression)
            {
                this.expression = expression;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Function : Stmt
        {
            public readonly Token name;
            public readonly List<Token> parameters;
            public readonly List<Stmt> body;

            public Function(Token name, List<Token> parameters, List<Stmt> body)
            {
                this.name = name;
                this.parameters = parameters;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }
        }

        public class If : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;

            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Print : Stmt
        {
            public readonly Expr expression;
            public Print(Expr expression)
            {
                this.expression = expression;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Return : Stmt
        {
            public readonly Token name;
            public readonly Expr initializer;

            public Return(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }
        }

        public class Var : Stmt
        {
            public readonly Token name;
            public readonly Expr initializer;

            public Var(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        public class While : Stmt
        {
            public readonly Expr condition;
            public readonly Stmt body;

            public While(Expr condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;
            }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }
        }

    }
}
