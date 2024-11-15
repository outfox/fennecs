// ReSharper disable file RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CodegenCS;
namespace fennecs.generators;

/// <summary>
/// Generator class for CodegenCS https://github.com/Drizin/CodegenCS
/// </summary>
/// <remarks>
/// This is parsed as a CSX template in build target <b>"GenerateCode"</b>
/// </remarks>
// ReSharper disable once UnusedType.Local
file class JobsGenerator
{
    // ReSharper disable once UnusedMember.Local
    public void Main(ICodegenContext context)
    {
        var source = new StringBuilder();
        
        source.AppendLine(FileHeader());
        
        foreach (var width in Enumerable.Range(1, 5))
        {
            source.AppendLine(ClassHeader(width));

            var top = (1 << width) - 1;
            for (var bits = top; bits >= 0; bits--)
            {
                source.AppendLine(GenerateJobs(false, false, width, bits));
                source.AppendLine(GenerateJobs(true, false, width, bits));
                source.AppendLine(GenerateJobs(false, true, width, bits));
                source.AppendLine(GenerateJobs(true, true, width, bits));
            }

            source.AppendLine(ClassFooter());                        
        }                           
        context["Jobs.g.cs"].Write($"{source}");
    }
    private FormattableString Memory(bool write, int index)
    {
        //var sb = 
        var readOnly = !write ? "ReadOnly" : "";
        return $"{readOnly}Memory<C{index}> Memory{index} = null!;";
    }
    
    private FormattableString Memories(int width, string pattern)
    {
        StringBuilder sb = new();
        
        for (var i = 0; i < width; i++)
        {
            sb.AppendLine(Memory(pattern[i] == 'W', i).ToString());
        }
        return FormattableStringFactory.Create(sb.ToString());
    }

    /*
    private string GenerateJobs(bool entity, bool uniform, int width, int bits)
    {
        var accessors = $"{bits:b16}".Substring(16 - width).Replace("0", "W").Replace("1", "R");
        var typeParams = TypeParams(width);
        var jobParams = JobParams(width, uniform);
        var actionParams = ActionParams(width, entity, uniform, accessors);

        var jobName = $"Job{(entity ? "E" : "")}{(uniform ? "U" : "")}{accessors}";
        var jobType = $"{jobName}<{jobParams}>";
    };
    */
    
    
    private string ActionParams(int width, bool entity, bool uniform, string pattern)
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

    private string TypeParams(int width)
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

    private string Select(int width)
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

    private string Deconstruct(int width, string pattern)
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

    private string Parameters(bool entity, bool uniform, string pattern)
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

    private string ClassHeader(int width)
    {
        //language=C#
        return $$"""               
               public partial record Stream<{{TypeParams(width)}}>
               {
               """;
    }
    private string ClassFooter()
    {
        //language=C#
        return $$"""
                 }
                 
                 
                 """;
    }
    
    private string FileHeader()
    {
        return 
            $"""
            // <auto-generated/>
            using System.Runtime.CompilerServices;
            using fennecs.pools;
            using fennecs.storage;
            
            namespace fennecs;
            
            """;
}
    
    private  string JobParams(int width, bool uniform)
    {
        var typeParams = new StringBuilder();
        
        if (uniform) typeParams.Append($"U, ");
        
        //language=C#
        for (var i = 0; i < width; i++)
        {
            typeParams.Append($"C{i}");
            if (i < width - 1) typeParams.Append(", ");
        }
        return typeParams.ToString();
    }

    private  string JobConstraints(int width)
    {
        var contraints = new StringBuilder();
        
        //language=C#
        for (var i = 0; i < width; i++)
        {
            contraints.Append($"where C{i} : notnull ");
        }
        return contraints.ToString();
    }

    
    private string GenerateJobs(bool entity, bool uniform, int width, int bits)
    {
        var accessors = $"{bits:b16}".Substring(16 - width).Replace("0", "W").Replace("1", "R");
        var typeParams = TypeParams(width);
        var jobParams = JobParams(width, uniform);
        var constraints = JobConstraints(width);
        var actionParams = ActionParams(width, entity, uniform, accessors);

        var memories = Memories(width, accessors);
        var deconstruction = Deconstruct(width, accessors);
        var jobName = $"Job{(entity ? "E" : "")}{(uniform ? "U" : "")}{accessors}";
        var jobType = $"{jobName}<{jobParams}>";

        return //Language=C#
            $$"""
                  internal record {{jobType}} : IThreadPoolWorkItem 
                      {{constraints}}
                  {
                      public ReadOnlyMemory<Identity> MemoryE = null!;
                      public World World = null!;
                  
                      // Memories
                      {{memories}}
                  
                      // Types
                      public TypeExpression Type0 = default;
                  
                      public Action<{{actionParams}}> Action = null!;
                      public CountdownEvent CountDown = null!;
                      public void Execute() 
                      {
                          var identities = MemoryE.Span;
                          {{deconstruction}}
                  
                          var count = identities.Length;
                          for (var i = 0; i < count; i++)
                          {
                              var entity = new Entity(World, identities[i]);
                              Action({{actionParams}});
                          }
                          CountDown.Signal();
                      }
                  }
                  
              """;
    }
}