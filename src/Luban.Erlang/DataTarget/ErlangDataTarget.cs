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

using Luban.DataTarget;
using Luban.Datas;
using Luban.Defs;
using Luban.Erlang.DataVisitors;
using System.Text;

namespace Luban.Erlang.DataTarget;

[DataTarget("erlang")]
public class ErlangDataTarget : DataTargetBase
{
    public void ExportTableSingleton(DefTable t, Record record, StringBuilder s)
    {
        s.Append($"-module({GetModuleName(t)}).").AppendLine();
        s.Append("-export([get_data/0]).").AppendLine();
        s.AppendLine();
        s.Append("get_data() ->").AppendLine();
        s.Append('\t').Append(record.Data.Apply(ToErlangLiteralVisitor.Ins)).Append('.');
    }

    public void ExportTableMap(DefTable t, List<Record> records, StringBuilder s)
    {
        s.Append($"-module({GetModuleName(t)}).").AppendLine();
        s.Append("-export([get_data_map/0, get_key_list/0]).").AppendLine();
        s.AppendLine();

        s.Append("get_data_map() -> #{" ).AppendLine();
        bool first = true;
        foreach (Record r in records)
        {
            if (!first)
            {
                s.Append(',').AppendLine();
            }
            first = false;
            DBean d = r.Data;
            s.Append($"\t{d.GetField(t.Index).Apply(ToErlangLiteralVisitor.Ins)} => ");
            s.Append(d.Apply(ToErlangLiteralVisitor.Ins));
        }
        s.AppendLine();
        s.Append("}.").AppendLine();
        s.AppendLine();

        s.Append("get_key_list() ->").AppendLine();
        s.Append("\t[").Append(string.Join(", ", records.Select(r => r.Data.GetField(t.Index).Apply(ToErlangLiteralVisitor.Ins)))).Append("].");
    }

    public void ExportTableList(DefTable t, List<Record> records, StringBuilder s)
    {
        s.Append($"-module({GetModuleName(t)}).").AppendLine();
        s.Append("-export([get_data_list/0]).").AppendLine();
        s.AppendLine();

        s.Append("get_data_list() ->").AppendLine();
        s.Append("\t[").AppendLine();
        bool first = true;
        foreach (Record r in records)
        {
            if (!first)
            {
                s.Append(',').AppendLine();
            }
            first = false;
            s.Append('\t').Append(r.Data.Apply(ToErlangLiteralVisitor.Ins));
        }
        s.AppendLine();
        s.Append("\t].");
    }

    protected override string DefaultOutputFileExt => "erl";

    // Erlang requires the module name to match the erl file name, which comes from OutputDataFile.
    private static string GetModuleName(DefTable t)
    {
        return t.OutputDataFile.ToLowerInvariant();
    }

    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        var s = new StringBuilder();
        if (table.IsMapTable)
        {
            ExportTableMap(table, records, s);
        }
        else if (table.IsSingletonTable)
        {
            ExportTableSingleton(table, records[0], s);
        }
        else
        {
            ExportTableList(table, records, s);
        }
        s.AppendLine();
        return CreateOutputFile($"{table.OutputDataFile}.{OutputFileExt}", s.ToString());
    }
}
