/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/27/2020         EPPlus Software AB       Initial release EPPlus 5
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml.FormulaParsing.Excel.Operators;
using OfficeOpenXml.FormulaParsing.ExpressionGraph;
using OfficeOpenXml.Utils;

namespace OfficeOpenXml.FormulaParsing.ExcelUtilities
{
    public class ExpressionEvaluator
    {
        private readonly WildCardValueMatcher _wildCardValueMatcher;
        private readonly CompileResultFactory _compileResultFactory;

        public ExpressionEvaluator()
            : this(new WildCardValueMatcher(), new CompileResultFactory())
        {

        }

        public ExpressionEvaluator(WildCardValueMatcher wildCardValueMatcher, CompileResultFactory compileResultFactory)
        {
            _wildCardValueMatcher = wildCardValueMatcher;
            _compileResultFactory = compileResultFactory;
        }

        private string GetNonAlphanumericStartChars(string expr)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(expr))
            {
                expr = expr.Trim();
                if (expr.Length > 1 && !char.IsLetterOrDigit(expr[0]) && !char.IsLetterOrDigit(expr[1]))
                {
                    if (char.IsWhiteSpace(expr[1]))
                        result = expr.Substring(0, 1);
                    else
                        result = expr.Substring(0, 2);
                }
                else if (expr.Length > 0 && !char.IsLetterOrDigit(expr[0]))
                {
                    result = expr.Substring(0, 1);
                }
            }
            
            return result;
        }

        private bool EvaluateOperator(object left, object right, IOperator op)
        {
            var leftResult = _compileResultFactory.Create(left);
            var rightResult = _compileResultFactory.Create(right);
            var result = op.Apply(leftResult, rightResult);
            if (result.DataType != DataType.Boolean)
            {
                throw new ArgumentException("Illegal operator in expression");
            }
            return (bool)result.Result;
        }

        public bool TryConvertToDouble(object op, out double d)
        {
            if (op is double || op is int)
            {
                d = Convert.ToDouble(op);
                return true;
            }
            else if (op is DateTime)
            {
                d = ((DateTime) op).ToOADate();
                return true;
            }
            else if (op != null)
            {
                if (double.TryParse(op.ToString(), out d))
                {
                    return true;
                }
            }
            d = 0;
            return false;
        }

        /// <summary>
        /// Returns true if any of the supplied expressions evaluates to true
        /// </summary>
        /// <param name="left">The object to evaluate</param>
        /// <param name="expressions">The expressions to evaluate the object against</param>
        /// <returns>True if any of the supplied expressions evaluates to true</returns>
        public bool Evaluate(object left, IEnumerable<string> expressions)
        {
            if (expressions == null || !expressions.Any()) return false;
            foreach(var expression in expressions)
            {
                if(Evaluate(left, expression))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the supplied expression evaluates to true
        /// </summary>
        /// <param name="left">The object to evaluate</param>
        /// <param name="expression">The expressions to evaluate the object against</param>
        /// <returns></returns>
        public bool Evaluate(object left, string expression)
        {
            if (expression == string.Empty)
            {
                return left == null;
            }
            var operatorCandidate = GetNonAlphanumericStartChars(expression);
            // ignore the wildcard operator *
            if (!string.IsNullOrEmpty(operatorCandidate) && operatorCandidate != "*")
            {
                IOperator op;
                if (OperatorsDict.Instance.TryGetValue(operatorCandidate, out op))
                {
                    var right = expression.Replace(operatorCandidate, string.Empty);
                    if (left == null && string.IsNullOrEmpty(right))
                    {
                        return op.Operator == Operators.Equals;
                    }
                    if (left == null ^ string.IsNullOrEmpty(right))
                    {
                        return op.Operator == Operators.NotEqualTo;
                    }
                    double leftNum, rightNum;
                    DateTime date;
                    bool leftIsNumeric = TryConvertToDouble(left, out leftNum);
                    bool rightIsNumeric = double.TryParse(right, out rightNum);
                    bool rightIsDate = DateTime.TryParse(right, out date);
                    if(rightIsNumeric && op.Operator == Operators.Minus)
                    {
                        rightNum *= -1;
                        op = OperatorsDict.Instance["="];
                    }
                    if (leftIsNumeric && rightIsNumeric)
                    {
                         return EvaluateOperator(leftNum, rightNum, op);
                    }
                    if (leftIsNumeric && rightIsDate)
                    {
                        return EvaluateOperator(leftNum, date.ToOADate(), op);
                    }
                    if (leftIsNumeric != rightIsNumeric)
                    {
                        return op.Operator == Operators.NotEqualTo;
                    }
                    return EvaluateOperator(left, right, op);
                }
            }
            return _wildCardValueMatcher.IsMatch(expression, left) == 0;
        }
    }
}
