// Copyright 2025 Code Philosophy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Luban.Datas;
using Luban.DataVisitors;
using Luban.Defs;
using Luban.Utils;
using System.Globalization;
using System.Text;

namespace Luban.Erlang.DataVisitors;

public class ToErlangLiteralVisitor : ToLiteralVisitorBase
{
    public static ToErlangLiteralVisitor Ins { get; } = new();

    public override string Accept(DBean type)
    {
        var x = new StringBuilder();
        if (type.Type.IsAbstractType)
        {
            x.Append($"#{{'name__' => \"{DataUtil.GetImplTypeName(type)}\"");
            if (type.Fields.Count > 0)
            {
                x.Append(',');
            }
        }
        else
        {
            x.Append("#{");
        }

        bool first = true;
        int index = 0;
        foreach (var f in type.Fields)
        {
            var defField = (DefField)type.ImplType.HierarchyFields[index++];
            if (f == null || !defField.NeedExport())
            {
                continue;
            }
            if (!first)
            {
                x.Append(',');
            }
            first = false;
            x.Append($"'{defField.Name}' => {f.Apply(this)}");
        }
        x.Append('}');
        return x.ToString();
    }

    private void Append(List<DType> datas, StringBuilder x)
    {
        x.Append('[');
        int index = 0;
        foreach (var e in datas)
        {
            if (index++ > 0)
            {
                x.Append(',');
            }
            x.Append(e.Apply(this));
        }
        x.Append(']');
    }

    public override string Accept(DArray type)
    {
        var x = new StringBuilder();
        Append(type.Datas, x);
        return x.ToString();
    }

    public override string Accept(DList type)
    {
        var x = new StringBuilder();
        Append(type.Datas, x);
        return x.ToString();
    }

    public override string Accept(DSet type)
    {
        var x = new StringBuilder();
        Append(type.Datas, x);
        return x.ToString();
    }

    public override string Accept(DFloat type)
    {
        return FormatFloat(type.Value);
    }

    public override string Accept(DDouble type)
    {
        return FormatFloat(type.Value);
    }

    // Erlang float literal requires digits on both sides of the decimal point (1.0e20, not 1e20 or 1E+20).
    private static string FormatFloat(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new NotSupportedException($"erlang literal does not support float value '{value}'");
        }
        var s = value.ToString("R", CultureInfo.InvariantCulture);
        int e = s.IndexOfAny(new[] { 'e', 'E' });
        var mantissa = e < 0 ? s : s[..e];
        var exponent = e < 0 ? "" : s[e..].ToLowerInvariant();
        if (!mantissa.Contains('.'))
        {
            mantissa += ".0";
        }
        return mantissa + exponent;
    }

    public override string Accept(DMap type)
    {
        var x = new StringBuilder();
        x.Append("#{");
        int index = 0;
        foreach (var e in type.DataMap)
        {
            if (index++ > 0)
            {
                x.Append(',');
            }
            x.Append($"{e.Key.Apply(this)} => {e.Value.Apply(this)}");
        }
        x.Append('}');
        return x.ToString();
    }
}
