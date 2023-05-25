using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ApiApplication.Controllers.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ConsecutiveInts : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is IEnumerable<short> numbers)
            {
                var sortedNumbers = numbers.OrderBy(n => n).ToList();

                for (int i = 1; i < sortedNumbers.Count; i++)
                {
                    if (sortedNumbers[i] != sortedNumbers[i - 1] + 1)
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
