using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace MemoryPack.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class CollectibleAssemblyLoadContextCollection
{
    public const string Name = "MemoryPack collectible AssemblyLoadContext";
}

[Collection(CollectibleAssemblyLoadContextCollection.Name)]
public class CollectibleAssemblyLoadContextTest
{
    const string PluginSource = """
using System;
using System.Collections.Generic;
using MemoryPack;

namespace CollectiblePlugin;

[MemoryPackable]
public partial class PluginChild
{
    public string? Name { get; set; }
    public Type? RuntimeType { get; set; }
}

[MemoryPackable]
public partial class PluginDto
{
    public int Id { get; set; }
    public Type? RuntimeType { get; set; }
    public PluginChild? Child { get; set; }
    public PluginChild?[]? Children { get; set; }
    public List<PluginChild?>? ChildList { get; set; }
    public Stack<PluginChild?>? ChildStack { get; set; }
    public Dictionary<string, PluginChild?>? ChildMap { get; set; }
}

[MemoryPackSerializable(typeof(PluginDto))]
[MemoryPackSerializable(typeof(PluginChild[]))]
[MemoryPackSerializable(typeof(List<PluginChild>))]
[MemoryPackSerializable(typeof(Dictionary<string, PluginChild>))]
public partial class PluginMemoryPackContext : MemoryPackSerializerContext
{
}

public static class PluginEntry
{
    public static bool Run()
    {
        var context = new PluginMemoryPackContext(MemoryPackSerializerOptions.Utf8);
        var value = new PluginDto
        {
            Id = 42,
            RuntimeType = typeof(PluginChild),
            Child = new PluginChild { Name = "nested", RuntimeType = typeof(PluginChild) },
            Children = new[] { new PluginChild { Name = "array" } },
            ChildList = new List<PluginChild?> { new PluginChild { Name = "list" } },
            ChildStack = new Stack<PluginChild?>(new[] { new PluginChild { Name = "stack" } }),
            ChildMap = new Dictionary<string, PluginChild?>
            {
                ["key"] = new PluginChild { Name = "dictionary" }
            }
        };

        var bytes = MemoryPackSerializer.Serialize<PluginDto, PluginMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<PluginDto, PluginMemoryPackContext>(bytes, context);
        var nonGenericBytes = context.Serialize(typeof(PluginDto), value);
        var nonGenericResult = (PluginDto?)context.Deserialize(typeof(PluginDto), nonGenericBytes);
        return result is not null &&
               nonGenericResult?.Id == 42 &&
               result.Id == 42 &&
               result.RuntimeType == typeof(PluginChild) &&
               result.Child?.Name == "nested" &&
               result.Child.RuntimeType == typeof(PluginChild) &&
               result.Children?[0]?.Name == "array" &&
               result.ChildList?[0]?.Name == "list" &&
               result.ChildStack?.Peek()?.Name == "stack" &&
               result.ChildMap?["key"]?.Name == "dictionary";
    }

    public static bool RunDefault()
    {
        var context = PluginMemoryPackContext.Default;
        var value = new PluginDto { Id = 84, Child = new PluginChild { Name = "default" } };
        var bytes = MemoryPackSerializer.Serialize<PluginDto, PluginMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<PluginDto, PluginMemoryPackContext>(bytes, context);
        return result?.Id == 84 && result.Child?.Name == "default";
    }

    public static string RunFailure()
    {
        var context = new PluginMemoryPackContext();
        try
        {
            MemoryPackSerializer.Deserialize<PluginDto, PluginMemoryPackContext>(new byte[] { 1 }, context);
            return "no error";
        }
        catch (Exception exception)
        {
            return exception.GetType().FullName ?? "error";
        }
    }

    public static string RunSerializeFailure()
    {
        var context = new PluginMemoryPackContext();
        try
        {
            MemoryPackSerializer.Serialize<PluginDto, PluginMemoryPackContext>(new PluginDto { RuntimeType = typeof(PluginEntry) }, context);
            return "no error";
        }
        catch (Exception exception)
        {
            return exception.GetType().FullName ?? "error";
        }
    }

    public static bool RunLegacy()
    {
        var value = new PluginDto { Id = 126 };
        var bytes = MemoryPackSerializer.Serialize(value);
        return MemoryPackSerializer.Deserialize<PluginDto>(bytes)?.Id == 126;
    }
}
""";

    const string PluginBSource = """
using MemoryPack;

namespace BridgePluginB;

[MemoryPackable]
public partial class PluginB
{
    public int Value { get; set; }
}

[MemoryPackSerializable(typeof(PluginB))]
public partial class PluginBContext : MemoryPackSerializerContext
{
}

public static class PluginBEntry
{
    public static bool Run()
    {
        var context = new PluginBContext();
        var bytes = MemoryPackSerializer.Serialize<PluginB, PluginBContext>(new PluginB { Value = 9 }, context);
        return MemoryPackSerializer.Deserialize<PluginB, PluginBContext>(bytes, context)?.Value == 9;
    }
}
""";

    const string BridgePluginSource = """
using System.Collections.Generic;
using BridgePluginB;
using MemoryPack;

namespace BridgePluginA;

[MemoryPackable]
public partial class PluginA
{
    public int Id { get; set; }
    public PluginB? Other { get; set; }
}

[MemoryPackSerializable(typeof(PluginA))]
[MemoryPackSerializable(typeof(Dictionary<PluginA, PluginB>))]
public partial class BridgeMemoryPackContext : MemoryPackSerializerContext
{
}

public static class BridgeEntry
{
    public static bool Run()
    {
        var context = new BridgeMemoryPackContext();
        var value = new PluginA { Id = 7, Other = new PluginB { Value = 11 } };
        var bytes = MemoryPackSerializer.Serialize<PluginA, BridgeMemoryPackContext>(value, context);
        var result = MemoryPackSerializer.Deserialize<PluginA, BridgeMemoryPackContext>(bytes, context);
        if (result?.Id != 7 || result.Other?.Value != 11)
        {
            return false;
        }

        var dictionary = new Dictionary<PluginA, PluginB>
        {
            [new PluginA { Id = 13 }] = new PluginB { Value = 17 }
        };
        var dictionaryBytes = MemoryPackSerializer.Serialize<Dictionary<PluginA, PluginB>, BridgeMemoryPackContext>(dictionary, context);
        var dictionaryResult = MemoryPackSerializer.Deserialize<Dictionary<PluginA, PluginB>, BridgeMemoryPackContext>(dictionaryBytes, context);
        foreach (var item in dictionaryResult!)
        {
            return item.Key.Id == 13 && item.Value.Value == 17;
        }
        return false;
    }

    public static object CreateContext()
    {
        return new BridgeMemoryPackContext();
    }
}
""";

    [Fact]
    public void GeneratedContext_DoesNotRootCollectibleAssemblyLoadContext()
    {
        var plugin = CompilePlugin();

        var references = LoadRunAndUnload(plugin);
        ForceUnload(references);

        references.LoadContext.IsAlive.Should().BeFalse("the context owns every formatter closed over plugin types");
        references.Assembly.IsAlive.Should().BeFalse();
        references.PluginType.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void SameAssemblyInTwoContexts_IsIsolatedAndUnloadsIndependently()
    {
        var plugin = CompilePlugin();
        var first = LoadedPlugin.Load(plugin, "plugin-one");
        var second = LoadedPlugin.Load(plugin, "plugin-two");

        first.Run().Should().BeTrue();
        second.Run().Should().BeTrue();

        var firstReferences = first.Unload();
        first = null!;
        ForceUnload(firstReferences);
        firstReferences.LoadContext.IsAlive.Should().BeFalse();
        firstReferences.Assembly.IsAlive.Should().BeFalse();
        firstReferences.PluginType.IsAlive.Should().BeFalse();

        second.Run().Should().BeTrue("unloading one context must not invalidate the other context's formatter graph");
        var secondReferences = second.Unload();
        second = null!;
        ForceUnload(secondReferences);
        secondReferences.LoadContext.IsAlive.Should().BeFalse();
        secondReferences.Assembly.IsAlive.Should().BeFalse();
        secondReferences.PluginType.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void GeneratedDefault_IsOwnedByCollectibleAssembly()
    {
        var references = LoadInvokeAndUnload(CompilePlugin(), "RunDefault", expected: true, expectGlobalRegistration: false);

        ForceUnload(references);

        references.LoadContext.IsAlive.Should().BeFalse("a generated Default is a static field in the collectible assembly itself");
        references.Assembly.IsAlive.Should().BeFalse();
        references.PluginType.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ContextExceptionPath_DoesNotLeaveThreadStaticRoot()
    {
        var references = LoadInvokeAndUnload(CompilePlugin(), "RunFailure", expected: "MemoryPack.MemoryPackSerializationException", expectGlobalRegistration: false);

        ForceUnload(references);

        references.LoadContext.IsAlive.Should().BeFalse();
        references.Assembly.IsAlive.Should().BeFalse();
        references.PluginType.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void ContextSerializeException_DoesNotLeaveThreadStaticRoot()
    {
        var references = LoadInvokeAndUnload(CompilePlugin(), "RunSerializeFailure", expected: "MemoryPack.MemoryPackSerializationException", expectGlobalRegistration: false);

        ForceUnload(references);

        references.LoadContext.IsAlive.Should().BeFalse();
        references.Assembly.IsAlive.Should().BeFalse();
        references.PluginType.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void LegacyStaticApi_ReproducesGlobalProviderRoot()
    {
        var references = LoadInvokeAndUnload(CompilePlugin(), "RunLegacy", expected: true, expectGlobalRegistration: true);

        ForceUnload(references);

        references.LoadContext.IsAlive.Should().BeTrue("the legacy global provider intentionally retains the registered plugin formatter");
        references.Assembly.IsAlive.Should().BeTrue();
        references.PluginType.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void BridgeContext_OwnsCrossAssemblyGraphAndReleasesBothContexts()
    {
        var pluginBImage = CompilePlugin("BridgePluginB", PluginBSource);
        var pluginBReference = MetadataReference.CreateFromImage(pluginBImage);
        var bridgeImage = CompilePlugin("BridgePluginA", BridgePluginSource, pluginBReference);

        var state = LoadBridgeAndBeginUnload(pluginBImage, bridgeImage);

        ForceUnload(state.BridgeReferences);
        ForceUnload(state.PluginBReferences);
        state.BridgeReferences.LoadContext.IsAlive.Should().BeTrue("the bridge context instance is the explicit lifetime owner for PluginA");
        state.PluginBReferences.LoadContext.IsAlive.Should().BeTrue("the bridge formatter graph closes over PluginB");

        state.ReleaseLease();
        ForceUnload(state.BridgeReferences);
        ForceUnload(state.PluginBReferences);

        state.BridgeReferences.LoadContext.IsAlive.Should().BeFalse();
        state.BridgeReferences.Assembly.IsAlive.Should().BeFalse();
        state.BridgeReferences.PluginType.IsAlive.Should().BeFalse();
        state.PluginBReferences.LoadContext.IsAlive.Should().BeFalse();
        state.PluginBReferences.Assembly.IsAlive.Should().BeFalse();
        state.PluginBReferences.PluginType.IsAlive.Should().BeFalse();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static BridgeUnloadState LoadBridgeAndBeginUnload(byte[] pluginBImage, byte[] bridgeImage)
    {
        var pluginB = LoadedPlugin.Load(pluginBImage, "bridge-b", "BridgePluginB.PluginBEntry", "BridgePluginB.PluginB");
        var bridge = LoadedPlugin.Load(bridgeImage, "bridge-a", "BridgePluginA.BridgeEntry", "BridgePluginA.PluginA", pluginB.Assembly);
        pluginB.Run().Should().BeTrue();
        bridge.Run().Should().BeTrue();

        var lease = new BridgeContextLease(bridge.Invoke("CreateContext")!);
        var bridgeReferences = bridge.Unload();
        var pluginBReferences = pluginB.Unload();
        bridge = null!;
        pluginB = null!;
        return new BridgeUnloadState(lease, bridgeReferences, pluginBReferences);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static UnloadReferences LoadRunAndUnload(byte[] plugin)
    {
        var loaded = LoadedPlugin.Load(plugin, "plugin-single");
        loaded.Run().Should().BeTrue();
        ProviderContainsAssembly(loaded.Assembly).Should().BeFalse("context serialization must not write plugin types or closed formatters to the global provider");
        var references = loaded.Unload();
        loaded = null!;
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static UnloadReferences LoadInvokeAndUnload(byte[] plugin, string methodName, object expected, bool expectGlobalRegistration)
    {
        var loaded = LoadedPlugin.Load(plugin, $"plugin-{methodName}");
        loaded.Invoke(methodName).Should().Be(expected);
        ProviderContainsAssembly(loaded.Assembly).Should().Be(expectGlobalRegistration);
        var references = loaded.Unload();
        loaded = null!;
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool ProviderContainsAssembly(Assembly pluginAssembly)
    {
        var providerType = typeof(MemoryPackFormatterProvider);
        foreach (var fieldName in new[] { "formatters", "genericFormatterFactory", "genericCollectionFormatterFactory" })
        {
            var field = providerType.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic)!;
            var dictionary = (IEnumerable)field.GetValue(null)!;
            foreach (var item in dictionary)
            {
                var key = item.GetType().GetProperty("Key")!.GetValue(item);
                var value = item.GetType().GetProperty("Value")!.GetValue(item);
                if (RefersToAssembly(key, pluginAssembly) || RefersToAssembly(value, pluginAssembly))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static bool RefersToAssembly(object? value, Assembly assembly)
    {
        return value switch
        {
            null => false,
            Type type => IsFromAssembly(type, assembly),
            Delegate callback => IsFromAssembly(callback.GetType(), assembly) || callback.Method.DeclaringType?.Assembly == assembly,
            _ => IsFromAssembly(value.GetType(), assembly),
        };
    }

    static bool IsFromAssembly(Type type, Assembly assembly)
    {
        if (type.Assembly == assembly)
        {
            return true;
        }

        if (type.HasElementType && type.GetElementType() is { } element && IsFromAssembly(element, assembly))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericArguments().Any(argument => IsFromAssembly(argument, assembly));
    }

    static byte[] CompilePlugin()
        => CompilePlugin("CollectiblePlugin", PluginSource);

    static byte[] CompilePlugin(string assemblyName, string source, params MetadataReference[] additionalReferences)
    {
        var (compilation, generatorDiagnostics) = CSharpGeneratorRunner.RunGenerator(
            source,
            preprocessorSymbols: ["NET7_0_OR_GREATER", "NET8_0_OR_GREATER"],
            assemblyName: assemblyName,
            additionalReferences: additionalReferences);
        generatorDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        using var peStream = new MemoryStream();
        var result = compilation.Emit(peStream);
        result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        result.Success.Should().BeTrue();
        return peStream.ToArray();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ForceUnload(UnloadReferences references)
    {
        for (var i = 0; i < 10 &&
             (references.LoadContext.IsAlive || references.Assembly.IsAlive || references.PluginType.IsAlive); i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    sealed class PluginLoadContext : AssemblyLoadContext
    {
        readonly Dictionary<string, Assembly> sharedAssemblies = new(StringComparer.OrdinalIgnoreCase);

        public PluginLoadContext(string name)
            : base(name, isCollectible: true)
        {
        }

        public void Share(Assembly assembly)
        {
            sharedAssemblies[assembly.GetName().Name!] = assembly;
        }

        public void ReleaseSharedAssemblies()
        {
            sharedAssemblies.Clear();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == typeof(MemoryPackableAttribute).Assembly.GetName().Name)
            {
                return typeof(MemoryPackableAttribute).Assembly;
            }

            if (assemblyName.Name is not null && sharedAssemblies.TryGetValue(assemblyName.Name, out var assembly))
            {
                return assembly;
            }

            return null;
        }
    }

    sealed class LoadedPlugin
    {
        PluginLoadContext? loadContext;
        Assembly? assembly;
        Type? entryType;
        Type? pluginType;
        MethodInfo? runMethod;

        public Assembly Assembly => assembly!;

        LoadedPlugin(PluginLoadContext loadContext, Assembly assembly, Type entryType, Type pluginType, MethodInfo runMethod)
        {
            this.loadContext = loadContext;
            this.assembly = assembly;
            this.entryType = entryType;
            this.pluginType = pluginType;
            this.runMethod = runMethod;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static LoadedPlugin Load(
            byte[] plugin,
            string name,
            string entryTypeName = "CollectiblePlugin.PluginEntry",
            string pluginTypeName = "CollectiblePlugin.PluginDto",
            params Assembly[] sharedAssemblies)
        {
            var loadContext = new PluginLoadContext(name);
            foreach (var sharedAssembly in sharedAssemblies)
            {
                loadContext.Share(sharedAssembly);
            }
            using var stream = new MemoryStream(plugin, writable: false);
            var assembly = loadContext.LoadFromStream(stream);
            var entryType = assembly.GetType(entryTypeName, throwOnError: true)!;
            var pluginType = assembly.GetType(pluginTypeName, throwOnError: true)!;
            var runMethod = entryType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!;
            return new LoadedPlugin(loadContext, assembly, entryType, pluginType, runMethod);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool Run()
        {
            return (bool)runMethod!.Invoke(null, null)!;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object? Invoke(string methodName)
        {
            var method = entryType!.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;
            var result = method.Invoke(null, null);
            method = null!;
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public UnloadReferences Unload()
        {
            var contextReference = new WeakReference(loadContext!);
            var assemblyReference = new WeakReference(assembly!);
            var typeReference = new WeakReference(pluginType!);
            var context = loadContext!;

            runMethod = null;
            entryType = null;
            pluginType = null;
            assembly = null;
            loadContext = null;
            context.ReleaseSharedAssemblies();
            context.Unload();
            context = null!;
            return new UnloadReferences(contextReference, assemblyReference, typeReference);
        }
    }

    sealed record UnloadReferences(WeakReference LoadContext, WeakReference Assembly, WeakReference PluginType);

    sealed class BridgeContextLease
    {
        object? context;

        public BridgeContextLease(object context)
        {
            this.context = context;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Release()
        {
            context = null;
        }
    }

    sealed class BridgeUnloadState
    {
        BridgeContextLease? lease;

        public UnloadReferences BridgeReferences { get; }
        public UnloadReferences PluginBReferences { get; }

        public BridgeUnloadState(BridgeContextLease lease, UnloadReferences bridgeReferences, UnloadReferences pluginBReferences)
        {
            this.lease = lease;
            BridgeReferences = bridgeReferences;
            PluginBReferences = pluginBReferences;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ReleaseLease()
        {
            lease!.Release();
            lease = null;
        }
    }
}
