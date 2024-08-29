using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron;
public static partial class StringExtensions
{

    public static string Left(this string @this, int maxLength)
    {
        if (@this.Length <= maxLength)
        {
            return @this;
        }
        return @this.Substring(0, maxLength);
    }
    public static string SafeFileName(this string @this, string padLeft = "", int maxLength = 60)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(@this).Left(maxLength) + padLeft;
        var ext = System.IO.Path.GetExtension(@this);
        var sb = new StringBuilder(maxLength + ext.Length);
        for (int i = 0; i < name.Length; i++)
        {
            char ch = name[i];
            ch = char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' ? ch : '-';
            sb.Append(ch);
        }
        sb.Append(ext);
        return sb.ToString();
    }

    //public static ReadOnlySpan<char> Left(this ReadOnlySpan<char> @this, int maxLength)
    //{
    //    if (@this.Length <= maxLength)
    //    {
    //        return @this;
    //    }
    //    return @this.Slice(0, maxLength);
    //}

    //public static string SafeFileName(this ReadOnlySpan<char> @this, int maxLength = 80)
    //{
    //    var name = System.IO.Path.GetFileNameWithoutExtension(@this).Left(maxLength);
    //    var ext = System.IO.Path.GetExtension(@this);
    //    Span<char> newName = stackalloc char[80 + ext.Length];
    //    for (int i = 0; i < name.Length; i++)
    //    {
    //        char ch = name[i];
    //        ch = char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' ? ch : '-';
    //        newName[i] = ch;
    //    }
    //    ext.CopyTo(newName.Slice(name.Length));
    //    return new string(newName., name.Length + ext.Length);
    //}

}