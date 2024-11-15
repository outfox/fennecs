using System;
using System.Collections.Generic;
using System.Text;
using CodegenCS;

namespace fennecs.generators;

// ReSharper disable once UnusedType.Local
/// <summary>
/// Generator class for CodegenCS https://github.com/Drizin/CodegenCS
/// </summary>
/// <remarks>
/// This is parsed as a CSX template in build target <b>"GenerateCode"</b>
/// </remarks>
file class StreamsForGenerator
{
    private readonly Dictionary<string, int> _types = new()
    {
        //{ "Stream.1", typeof(Stream<>) },
        //{ "Stream.2", typeof(Stream<,>) },
        //{ "Stream.3", typeof(Stream<,,>) },
        //{ "Stream.4", typeof(Stream<,,,>) },
        //{ "Stream.5", typeof(Stream<,,,,>) },

        { "Stream.1", 1 },
        { "Stream.2", 2 },
        { "Stream.3", 3 },
        { "Stream.4", 4 }, 
        { "Stream.5", 5 },
    };
 
    //public  FormattableString Main() => $$"""{{{Class}}}""";

    public void Main(ICodegenContext context)
    {
        var source = new StringBuilder();
        
        source.AppendLine(FileHeader());
        
        foreach (var pair in _types)
        {
            Console.WriteLine($"Processing {pair.Key}");
            
            //TODO: Load types at generator runtime
            //var width = type.GetGenericArguments().Length;
            
            var (name, width) = (pair.Key, pair.Value);
            
            var file =  name + ".generated.cs";

            source.AppendLine(ClassHeader(width));

            var top = (1 << width) - 1;
            for (var bits = top; bits >= 0; bits--)
            {
                source.AppendLine(GenerateFor(false, false, width, bits));
                source.AppendLine(GenerateFor(true, false, width, bits));
                source.AppendLine(GenerateFor(false, true, width, bits));
                source.AppendLine(GenerateFor(true, true, width, bits));
            }

            source.AppendLine(ClassFooter());                        
        }                           
        context[$"Streams.For.g.cs"].Write($$"""{{source}}""");
    }


    private static string ActionParams(int width, bool entity, bool uniform, string pattern)
    {
        var typeParams = new StringBuilder();

        //language=C#
        if (entity) typeParams.Append($"EntityRef, ");

        //language=C#
        if (uniform) typeParams.Append($"U, ");

        //language=C#
        for (var i = 0; i < width; i++)
        {
            var rw = pattern[i] == 'W' ? "RW" : "R";
            typeParams.Append($"{rw}<C{i}>");
            if (i < width - 1) typeParams.Append(", ");
        }
        return typeParams.ToString();
    }

    private static string TypeParams(int width)
    {
        var typeParams = new StringBuilder();

        //language=C#
        for (var i = 0; i < width; i++)
        {
            typeParams.Append($"C{i}");
            if (i < width - 1) typeParams.Append(", ");
        }
        return typeParams.ToString();
    }

    private static string Select(int width)
    {
        var select = new StringBuilder();
        if (width > 1) select.Append("(");
        //language=C#
        for (var i = 0; i < width; i++)
        {
            select.Append($"s{i}");
            if (i < width - 1) select.Append(", ");
        }
        if (width > 1) select.Append(")");
        return select.ToString();
    }

    private static string Deconstruct(int width, string pattern)
    {
        var deconstruct = new StringBuilder();
        //language=C#
        for (var i = 0; i < width; i++)
        {
            deconstruct.Append($"var span{i} = s{i}.Span; ");
            if (pattern[i] == 'W') deconstruct.Append($"var type{i} = s{i}.Expression; ");
        }
        return deconstruct.ToString();
    }

    private static string Parameters(bool entity, bool uniform, string pattern)
    {
        var parameters = new StringBuilder();

        //language=C#
        if (entity) parameters.Append("new(in entity), ");

        //language=C#
        if (uniform) parameters.Append("uniform, ");

        var index = 0;
        foreach (var p in pattern)
        {
            if (index != 0) parameters.Append(", ");
            parameters.Append(
                //language=C#
                p switch
                {
                    'W' => $"new(ref span{index}[i], in entity, in type{index})",
                    'R' => $"new(ref span{index}[i])",
                    _ => throw new NotImplementedException(),
                }
            );
            index++;
        }
        return parameters.ToString();
    }

    private static string ClassHeader(int width)
    {
        //language=C#
        return $$"""               
               public partial record Stream<{{TypeParams(width)}}>
               {
               """;
    }
    private static string ClassFooter()
    {
        //language=C#
        return "}";
    }
    
    private static string FileHeader()
    {
        return 
            """
            // <auto-generated/>
            using System.Runtime.CompilerServices;
            using fennecs.pools;
            using fennecs.storage;
            
            namespace fennecs;
            """;
}
    
    private static string GenerateFor(bool entity, bool uniform, int width, int bits)
    {
        var pattern = $"{bits:b16}".Substring(16 - width).Replace("0", "W").Replace("1", "R");
        
        return //Language=C#
            $$"""        
              
                      /// <include file='../XMLdoc.xml' path='members/member[@name="T:For{{(entity ? "E" : "")}}{{(uniform ? "U" : "")}}"]'/>
                      [OverloadResolutionPriority(0b_{{(entity ? 1 << width : 0)&255:b8}}_{{bits:b8}})]
                      public void For{{(uniform ? "<U>(U uniform, " : "(")}}Action<{{ActionParams(width, entity, uniform, pattern)}}> action)
                      {
                         using var worldLock = World.Lock();
              
                         foreach (var table in Filtered)
                         {
                             using var join = table.CrossJoin<{{TypeParams(width)}}>(_streamTypes.AsSpan());
                             if (join.Empty) continue;

                             var count = table.Count;
                             do
                             {
                                 var {{Select(width)}} = join.Select;
                                 {{Deconstruct(width, pattern)}}
                                 for (var i = 0; i < count; i++)
                                 {   
                                     var entity = table[i];
                                     action({{Parameters(entity, uniform, pattern)}}); 
                                 }
                             } while (join.Iterate());
                         }
                      }
                      
                      
              """;
    }

}
