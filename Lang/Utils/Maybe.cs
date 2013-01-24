using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Utils
{
    public static class Maybe
    {
        public static TInput Or<TInput>(this TInput input, Func<TInput> evaluator)
            where TInput : class
        {
            if (input != null)
            {
                return input;
            }

            return evaluator();
        }
    }
}
