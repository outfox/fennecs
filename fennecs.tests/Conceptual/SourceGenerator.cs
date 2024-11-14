using System.Text;

namespace fennecs.tests.Conceptual;

public class SourceGeneratorExperiment(ITestOutputHelper output)
{

    private static string EntityUniform(bool entity, bool uniform)
    {
        return $"{(uniform ? "U uniform, " : "")}{(entity ? "Entity" : "")}";
    }

    private static string ActionParams(int width,  bool entity, bool uniform, string pattern)
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

    private static string LoadEntity(bool entity)
    {
        //language=C#
        return entity ? "\n                  var entity = table[i];" : "";
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

    private string GenerateFor(bool entity, bool uniform, int width, int bits)
    {
        var pattern = $"{bits:b16}"[(16 - width)..16].Replace("0", "W").Replace("1", "R");

        //output.WriteLine($"{bits:3}:{bits:b16}:{entityRW}");

        //language=C#
        var code = $$"""
                        /// <include file='XMLdoc.xml' path='members/member[@name="T:For"]'/>
                        [OverloadResolutionPriority(0b_{{(entity ? 1 : 0)}}_{{bits:b8}})]
                        public void For{{(uniform ? "<U>(U uniform, " : "(")}}Action<{{ActionParams(width, entity, uniform, pattern)}}> action)
                        {
                           using var worldLock = World.Lock();

                           foreach (var table in Filtered)
                           {
                               var count = table.Count;
                               using var join = table.CrossJoin<{{TypeParams(width)}}>(_streamTypes.AsSpan());
                               if (join.Empty) continue;
                               do
                               {
                                   var {{Select(width)}} = join.Select;
                                   {{Deconstruct(width, pattern)}}
                                   for (var i = 0; i < count; i++)
                                   {   {{LoadEntity(entity)}}
                                       action({{Parameters(entity, uniform, pattern)}}); 
                                   }
                               } while (join.Iterate());
                           }
                        }
                        
                        
                     """;

        return code;
    }


    [Theory]
    [InlineData(3)]
    private void TestFor(int width)
    {
        var top = (1 << width) - 1;
        for (var bits = top; bits >= 0; bits--)
        {
            output.WriteLine(GenerateFor(true, true, width, bits));
        }
        //return code;
    }

}
