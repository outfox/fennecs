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
file class JobsGenerator
{
    private FormattableString Memory(bool write, int index)
    {
        var readOnly = !write ? "ReadOnly" : "";
        return $"{readOnly}Memory<C{index}> Memory{index} = null!;";
    }
    
    // ReSharper disable once UnusedMember.Local
    public void Main(ICodegenContext context)
    {
        string jobType = "JobR<C0>";
        string constraints = "where C0 : notnull";
        string deconstruction = "var span0 = Memory0.Span";
        string actionParams = "...";
        string memories = "...";
        
        //language=C#
        context[$"generated/Jobs.g.cs"].Write(
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
                
            """
            );
    }
}