using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ember.DataSchemaManager.SharedFuncctions;

public class Shared
{
    public static Boolean IsNumeric(String input)
    {
        return Regex.IsMatch(input, @"^-?\d*\.?\d+$");
    }
}