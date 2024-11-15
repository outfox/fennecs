// ReSharper disable file RedundantUsingDirective
using System;
using System.Collections.Generic;
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
file class StreamsJobGenerator
{
    private readonly Dictionary<string, int> _types = new()
    {
        //{ "Stream.1", typeof(Stream<>) },
        //{ "Stream.2", typeof(Stream<,>) },
        //{ "Stream.3", typeof(Stream<,,>) },
        //{ "Stream.4", typeof(Stream<,,,>) },
        //{ "Stream.5", typeof(Stream<,,,,>) },

        { "Stream.1", 1 },
        //{ "Stream.2", 2 },
        //{ "Stream.3", 3 },
        //{ "Stream.4", 4 }, 
        //{ "Stream.5", 5 },
    };
 
    //public  FormattableString Main() => $$"""{{{Class}}}""";

    // ReSharper disable once UnusedMember.Local
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
        context[$"Streams.Job.g.cs"].Write($$"""{{source}}""");
    }


    private  string ActionParams(int width, bool entity, bool uniform, string pattern)
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

    private  string TypeParams(int width)
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
    
   private  string Select(int width)
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

    private  string Deconstruct(int width, string accessors, bool uniform)
    {
        var deconstruct = new StringBuilder();

        //language=C#
        if (uniform) deconstruct.Append($"job.Uniform = uniform;");
        
        //language=C#
        for (var i = 0; i < width; i++)
        {
            var access = accessors[i] == 'R' ? "ReadOnly" : ""; 
            deconstruct.Append($"job.Memory{i} = s{i}.As{access}Memory(start, length);");
            deconstruct.Append($"job.Type{i} = s{i}.Expression;");
        }
        return deconstruct.ToString();
    }

    private  string Parameters(bool entity, bool uniform, string pattern)
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

    private  string ClassHeader(int width)
    {
        //language=C#
        return $$"""               
               public partial record Stream<{{TypeParams(width)}}>
               {
               """; 
    }
    private  string ClassFooter()
    {
        //language=C#
        return "}";
    }
    
    private  string FileHeader()
    { 
        return 
            """
            // <auto-generated/>
            using System.Runtime.CompilerServices;
            using fennecs.pools;
            using fennecs.storage;
            
            // ReSharper disable InconsistentNaming
            
            namespace fennecs;
            """;
}
    
    private  string GenerateFor(bool entity, bool uniform, int width, int bits)
    {
        var accessors = $"{bits:b16}".Substring(16 - width).Replace("0", "W").Replace("1", "R");
        var typeParams = TypeParams(width);
        var jobParams = JobParams(width, uniform);
        var actionParams = ActionParams(width, entity, uniform, accessors);

        var jobName = $"Job{(entity ? "E" : "")}{(uniform ? "U" : "")}{accessors}";
        var jobType = $"{jobName}<{jobParams}>";

        //language=C#
        return
            $$"""        
                /// <include file='../XMLdoc.xml' path='members/member[@name="T:{{jobName}}"]'/>
                [OverloadResolutionPriority(0b_{{(entity ? 1 << width : 0)&255:b8}}_{{bits:b8}})]
                public void Job{{(uniform ? "<U>(U uniform, " : "(")}}Action<{{actionParams}}> action)
                {
                  AssertNoWildcards();

                  using var worldLock = World.Lock();
                  var chunkSize = Math.Max(1, Count / Concurrency);

                  Countdown.Reset();

                  using var jobs = PooledList<{{jobType}}>.Rent();

                  foreach (var table in Filtered)
                  {
                      using var join = table.CrossJoin<{{typeParams}}>(_streamTypes.AsSpan());
                      if (join.Empty) continue;

                      var count = table.Count; // storage.Length is the capacity, not the count.
                      var partitions = count / chunkSize + Math.Sign(count % chunkSize);
                      do
                      {
                          for (var chunk = 0; chunk < partitions; chunk++)
                          {
                              Countdown.AddCount();

                              var start = chunk * chunkSize;
                              var length = Math.Min(chunkSize, count - start);

                              var {{Select(width)}} = join.Select;

                              var job = JobPool<{{jobType}}>.Rent();

                              {{Deconstruct(width, accessors, uniform)}}

                              job.World = table.World;
                              job.MemoryE = table.GetStorage<Identity>(default).AsMemory(start, length);
                              job.Action = action;
                              job.CountDown = Countdown;
                              jobs.Add(job);

                              ThreadPool.UnsafeQueueUserWorkItem(job, true);
                          }
                      } while (join.Iterate());
                  }

                  Countdown.Signal();
                  Countdown.Wait();

                  JobPool<{{jobType}}>.Return(jobs);
                }
            """;
    }

}
