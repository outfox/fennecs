﻿// ReSharper disable file RedundantUsingDirective
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

    private  string ClassHeader(int width)
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
        return $$"""
                 }


                 """;
    }


    private  string FileHeader()
    { 
        return 
            """
            // <auto-generated/>
            using System.Runtime.CompilerServices;
            using fennecs.pools;
            using fennecs.storage;
            
            #pragma warning disable CS0414 // Field is assigned but its value is never used
            // ReSharper disable file IdentifierTypo
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
                /// <include file='../_docs.xml' path='members/member[@name="T:{{jobName}}"]'/>
                [OverloadResolutionPriority(0b_{{(!entity ? 1 << width : 0)&255:b8}}_{{bits&255:b8}})]
                public void Job{{(uniform ? "<U>(U uniform, " : "(")}}Action<{{actionParams}}> action)
                {
                  AssertNoWildcards();

                  using var worldLock = World.Lock();
                  var chunkSize = Math.Max(1, Count / Concurrency);

                  Countdown = Countdown ?? new (0); 
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
                              job.MemoryE = table.GetStorage<Identity>(default).AsReadOnlyMemory(start, length);
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
