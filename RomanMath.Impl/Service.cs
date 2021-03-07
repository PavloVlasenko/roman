using System;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt.Parsec;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Prim;

namespace RomanMath.Impl
{
    public static class Service
    {
        /// <summary>
        /// Accepts a math expression with Roman digits, evaluates it and returns a result in decimal format
        /// </summary>
        /// <param name="expression">Expression to evaluate</param>
        /// <exception cref="ArgumentNullException">If provided expression string is null or empty</exception>
        /// <exception cref="ParserException">If provided expression is invalid math expression</exception>
        /// <returns>Expression result in decomal format</returns>
        public static int Evaluate(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentNullException(nameof(expression), "String is null or empty");
            var trimmed = expression.Where(t => !char.IsSeparator(t)).ToArray();
            var parseToEnd = from res in ParseExpression()
                from _ in eof
                select res;
            var parseResult = parse(parseToEnd, new string(trimmed));
            return parseResult.ToEither().Match(res => res, err => throw new ParserException(err));
        }

        //Table of operators. * has higher priority than + or -, thus it's in second array
        private static readonly Operator<int>[][] OperatorTable =
        {
            new[]
            {
                Operator.Infix(Assoc.Left,
                    oneOf("+-").Select<char, Func<int, int, int>>(t => t switch
                    {
                        '+' => (a, b) => a + b,
                        '-' => (a, b) => a - b,
                        // this exception will newer be thrown
                        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
                    }))
            },
            new[] {Operator.Infix(Assoc.Left, ch('*').Select<char, Func<int, int, int>>(t => (a, b) => a * b)),},
        };

        private static Parser<int> ParseExpression()
        {
            Parser<int> withOpers(Parser<int> t) =>
                Expr.buildExpressionParser(OperatorTable, t);

            var tryParseRoman = attempt(ParseRoman());
            return withOpers(choice(InScope(lazyp(() => ParseExpression())), tryParseRoman,
                withOpers(tryParseRoman)));
        }

        private static Parser<T> InScope<T>(Parser<T> p) =>
            from _ in ch('(')
            from res in p
            from r in ch(')')
            select res;

        private static Parser<int> ParseRoman() =>
            from thousand in ManyUpToN(ch('M'), 3).Select(t => t.Length() * 1000)
            from hundred in GenericParse(100, 'C', 'D', 'M')
            from ten in GenericParse(10, 'X', 'L', 'C')
            from one in GenericParse(1, 'I', 'V', 'X')
            select thousand + hundred + ten + one;

        /// <summary>
        /// Applies <see cref="n"/> or less times provided <see cref="parser"/>
        /// </summary>
        private static Parser<ImmutableStack<T>> ManyUpToN<T>(Parser<T> parser, int n)
        {
            if (n <= 0) return result(ImmutableStack<T>.Empty);
            var res = from elem in parser
                from otherElems in ManyUpToN(parser, n - 1)
                select otherElems.Push(elem);
            return choice(res, result(ImmutableStack<T>.Empty));
        }

        private static Parser<int> GenericParse(int @base, char low, char mid, char top)
        {
            //Like IX
            var oneUnderTop = str($"{low}{top}").Select(t => 9 * @base);
            //Like VI, VII, VIII
            var overMid = from _ in ch(mid)
                from s in ManyUpToN(ch(low), 3)
                select (s.Length() + 5) * @base;
            //Like IV
            var oneUnderMid = str($"{low}{mid}").Select(t => 4 * @base);
            //Like I, II. III
            var overLow = ManyUpToN(ch(low), 3).Select(s => s.Length() * @base);
            return choice(attempt(oneUnderTop), attempt(overMid), attempt(oneUnderMid), attempt(overLow));
        }
    }
}