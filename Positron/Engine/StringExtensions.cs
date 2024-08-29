using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroSpeech.Positron;

public static partial class StringExtensions
{

    public static bool EqualsIgnoreCase(this string text, string compare) {
        if (string.IsNullOrEmpty(text))
            return string.IsNullOrEmpty(compare);
        return text.Equals(compare, StringComparison.OrdinalIgnoreCase);
    }

    public static string ToCamelCase(this string text)
    {
        StringBuilder sb = new StringBuilder(text.Length);
        var ce = text.GetEnumerator();
        while (ce.MoveNext()){
            var ch = ce.Current;
            if (Char.IsUpper(ch))
            {
                sb.Append(Char.ToLowerInvariant(ch));
                continue;
            }
            sb.Append(ch);
            break;
        }
        while (ce.MoveNext())
        {
            sb.Append(ce.Current);
        }
        return sb.ToString();            
    }
}
