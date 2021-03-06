using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

using JetBrains.Platform.RdFramework;
using JetBrains.Platform.RdFramework.Base;
using JetBrains.Platform.RdFramework.Impl;
using JetBrains.Platform.RdFramework.Tasks;
using JetBrains.Platform.RdFramework.Util;
using JetBrains.Platform.RdFramework.Text;

using JetBrains.Util.Collections;
using JetBrains.Util.Logging;
using JetBrains.Util.PersistentMap;
using Lifetime = JetBrains.DataFlow.Lifetime;

// ReSharper disable RedundantEmptyObjectCreationArgumentList
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantOverflowCheckingContext


namespace JetBrains.Rider.Model
{
  
  
  public class NukeModel : RdExtBase {
    //fields
    //public fields
    [NotNull] public IRdSignal<BuildInvocation> Build { get { return _Build; }}
    
    //private fields
    [NotNull] private readonly RdSignal<BuildInvocation> _Build;
    
    //primary constructor
    private NukeModel(
      [NotNull] RdSignal<BuildInvocation> build
    )
    {
      if (build == null) throw new ArgumentNullException("build");
      
      _Build = build;
      BindableChildren.Add(new KeyValuePair<string, object>("build", _Build));
    }
    //secondary constructor
    internal NukeModel (
    ) : this (
      new RdSignal<BuildInvocation>(BuildInvocation.Read, BuildInvocation.Write)
    ) {}
    //statics
    
    
    
    protected override long SerializationHash => -4251989644293807542L;
    
    protected override Action<ISerializers> Register => RegisterDeclaredTypesSerializers;
    public static void RegisterDeclaredTypesSerializers(ISerializers serializers)
    {
      serializers.Register(BuildInvocation.Read, BuildInvocation.Write);
      
      serializers.RegisterToplevelOnce(typeof(IdeRoot), IdeRoot.RegisterDeclaredTypesSerializers);
    }
    
    //custom body
    //equals trait
    //hash code trait
    //pretty print
    public override void Print(PrettyPrinter printer)
    {
      printer.Println("NukeModel (");
      using (printer.IndentCookie()) {
        printer.Print("build = "); _Build.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
  public static class SolutionNukeModelEx
   {
    public static NukeModel GetNukeModel(this Solution solution)
    {
      return solution.GetOrCreateExtension("nukeModel", () => new NukeModel());
    }
  }
  
  
  public class BuildInvocation : IPrintable, IEquatable<BuildInvocation> {
    //fields
    //public fields
    [NotNull] public string ProjectFile {get; private set;}
    [NotNull] public string Target {get; private set;}
    public bool DebugMode {get; private set;}
    public bool SkipDependencies {get; private set;}
    
    //private fields
    //primary constructor
    public BuildInvocation(
      [NotNull] string projectFile,
      [NotNull] string target,
      bool debugMode,
      bool skipDependencies
    )
    {
      if (projectFile == null) throw new ArgumentNullException("projectFile");
      if (target == null) throw new ArgumentNullException("target");
      
      ProjectFile = projectFile;
      Target = target;
      DebugMode = debugMode;
      SkipDependencies = skipDependencies;
    }
    //secondary constructor
    //statics
    
    public static CtxReadDelegate<BuildInvocation> Read = (ctx, reader) => 
    {
      var projectFile = reader.ReadString();
      var target = reader.ReadString();
      var debugMode = reader.ReadBool();
      var skipDependencies = reader.ReadBool();
      return new BuildInvocation(projectFile, target, debugMode, skipDependencies);
    };
    
    public static CtxWriteDelegate<BuildInvocation> Write = (ctx, writer, value) => 
    {
      writer.Write(value.ProjectFile);
      writer.Write(value.Target);
      writer.Write(value.DebugMode);
      writer.Write(value.SkipDependencies);
    };
    //custom body
    //equals trait
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((BuildInvocation) obj);
    }
    public bool Equals(BuildInvocation other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ProjectFile == other.ProjectFile && Target == other.Target && DebugMode == other.DebugMode && SkipDependencies == other.SkipDependencies;
    }
    //hash code trait
    public override int GetHashCode()
    {
      unchecked {
        var hash = 0;
        hash = hash * 31 + ProjectFile.GetHashCode();
        hash = hash * 31 + Target.GetHashCode();
        hash = hash * 31 + DebugMode.GetHashCode();
        hash = hash * 31 + SkipDependencies.GetHashCode();
        return hash;
      }
    }
    //pretty print
    public void Print(PrettyPrinter printer)
    {
      printer.Println("BuildInvocation (");
      using (printer.IndentCookie()) {
        printer.Print("projectFile = "); ProjectFile.PrintEx(printer); printer.Println();
        printer.Print("target = "); Target.PrintEx(printer); printer.Println();
        printer.Print("debugMode = "); DebugMode.PrintEx(printer); printer.Println();
        printer.Print("skipDependencies = "); SkipDependencies.PrintEx(printer); printer.Println();
      }
      printer.Print(")");
    }
    //toString
    public override string ToString()
    {
      var printer = new SingleLinePrettyPrinter();
      Print(printer);
      return printer.ToString();
    }
  }
}
