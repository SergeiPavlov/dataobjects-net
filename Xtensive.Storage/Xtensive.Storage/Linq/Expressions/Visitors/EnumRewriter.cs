﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ExpressionVisitor = Xtensive.Core.Linq.ExpressionVisitor;

namespace Xtensive.Storage.Linq.Expressions.Visitors
{
  public class EnumRewriter : ExpressionVisitor
  {
    protected override Expression VisitUnknown(Expression e)
    {
      return ConvertEnum(e);
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
      return ConvertEnum(c);
    }

    /// <inheritdoc/>
    public new Expression Visit(Expression e)
    {
      return base.Visit(e);
    }

    private Expression ConvertEnum(Expression expression)
    {
      return expression.Type.IsEnum 
        ? Expression.Convert(expression, Enum.GetUnderlyingType(expression.Type)) 
        : expression;
    }
  }
}