using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ChampUI.Services;

namespace ChampUI.Tests.Infrastructure;

/// <summary>A node of the rendered tree (element or text) that tests can query and interact with.</summary>
public sealed class Node
{
    public string Tag { get; init; } = "";
    public string? Text { get; init; }
    public Dictionary<string, string> Attributes { get; } = new();
    public Dictionary<string, ulong> Handlers { get; } = new();
    public List<Node> Children { get; } = new();

    public string InnerText => Text ?? string.Concat(Children.Select(c => c.InnerText));
    public string? Attr(string name) => Attributes.TryGetValue(name, out var v) ? v : null;
    public bool HasClass(string cls) => (Attr("class") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cls);

    public IEnumerable<Node> Descendants()
    {
        foreach (var c in Children)
        {
            yield return c;
            foreach (var d in c.Descendants()) yield return d;
        }
    }
}

#pragma warning disable BL0006 // TestHost intentionally inspects RenderTree frames to dispatch component events.
/// <summary>
/// A tiny component test host built on Blazor's public <see cref="Renderer"/>: renders a component,
/// exposes the resulting element tree, and dispatches DOM-style events (click, input, keydown) to it.
/// </summary>
public sealed class TestHost : Renderer
{
    private int _rootId;
    private IComponent? _component;

    public TestHost(Action<ChampUiOptions>? configure = null, Microsoft.JSInterop.IJSRuntime? js = null)
        : this(BuildProvider(configure, js)) { }

    private TestHost(ServiceProvider services) : base(services, NullLoggerFactory.Instance) => Services = services;

    public ServiceProvider Services { get; }

    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

    private static ServiceProvider BuildProvider(Action<ChampUiOptions>? configure, Microsoft.JSInterop.IJSRuntime? js)
    {
        var services = new ServiceCollection();
        services.AddChampUI(configure);
        // Register a no-op IJSRuntime so components that inject it (e.g. UiDataGrid for export) can be tested.
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(js ?? new NullJSRuntime());
        // Register null logger for components that inject ILogger
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        return services.BuildServiceProvider();
    }

    protected override void HandleException(Exception exception) => throw new InvalidOperationException("Component threw during rendering.", exception);
    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;

    /// <summary>Renders <typeparamref name="TComponent"/> with the given parameters and returns the root of its tree.</summary>
    public async Task<Node> RenderAsync<TComponent>(IDictionary<string, object?>? parameters = null) where TComponent : IComponent
    {
        var component = InstantiateComponent(typeof(TComponent)); // goes through DI so [Inject] properties are populated
        _component = component;
        return await RenderAsync(component, parameters);
    }

    /// <summary>Returns the last rendered component instance cast to <typeparamref name="T"/>.</summary>
    public T GetComponent<T>() where T : class, IComponent
        => _component as T ?? throw new InvalidOperationException($"Last rendered component is not {typeof(T).Name}.");

    /// <summary>Renders an arbitrary <see cref="RenderFragment"/> (useful for nesting parent/child components).</summary>
    public Task<Node> RenderFragmentAsync(RenderFragment fragment) =>
        RenderAsync<FragmentHost>(new Dictionary<string, object?> { ["Fragment"] = fragment });

    private async Task<Node> RenderAsync(IComponent component, IDictionary<string, object?>? parameters)
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            _rootId = AssignRootComponentId(component);
            await RenderRootComponentAsync(_rootId, parameters is null ? ParameterView.Empty : ParameterView.FromDictionary(parameters!));
        });
        return Snapshot();
    }

    /// <summary>Re-reads the current tree after interactions.</summary>
    public Node Snapshot()
    {
        var root = new Node { Tag = "#root" };
        Build(_rootId, root);
        return root;
    }

    private void Build(int componentId, Node parent)
    {
        var frames = GetCurrentRenderTreeFrames(componentId);
        BuildRange(frames.Array, 0, frames.Count, parent);
    }

    private void BuildRange(RenderTreeFrame[] frames, int start, int end, Node parent)
    {
        var i = start;
        while (i < end)
        {
            ref var f = ref frames[i];
            switch (f.FrameType)
            {
                case RenderTreeFrameType.Element:
                {
                    var el = new Node { Tag = f.ElementName };
                    var subtreeEnd = i + f.ElementSubtreeLength;
                    var j = i + 1;
                    while (j < subtreeEnd && frames[j].FrameType == RenderTreeFrameType.Attribute)
                    {
                        ref var a = ref frames[j];
                        if (a.AttributeEventHandlerId != 0) el.Handlers[a.AttributeName] = a.AttributeEventHandlerId;
                        else if (a.AttributeValue is not null && a.AttributeValue is not bool b2) el.Attributes[a.AttributeName] = a.AttributeValue.ToString()!;
                        else if (a.AttributeValue is true) el.Attributes[a.AttributeName] = "";
                        j++;
                    }
                    BuildRange(frames, j, subtreeEnd, el);
                    parent.Children.Add(el);
                    i = subtreeEnd;
                    break;
                }
                case RenderTreeFrameType.Text:
                    parent.Children.Add(new Node { Tag = "#text", Text = f.TextContent });
                    i++;
                    break;
                case RenderTreeFrameType.Markup:
                    parent.Children.Add(new Node { Tag = "#markup", Text = f.MarkupContent });
                    i++;
                    break;
                case RenderTreeFrameType.Component:
                    Build(f.ComponentId, parent);
                    i += f.ComponentSubtreeLength;
                    break;
                case RenderTreeFrameType.Region:
                    BuildRange(frames, i + 1, i + f.RegionSubtreeLength, parent);
                    i += f.RegionSubtreeLength;
                    break;
                default:
                    i++;
                    break;
            }
        }
    }

    // ----- interaction

    public Task ClickAsync(Node element) => DispatchAsync(element, "onclick", new MouseEventArgs());
    public Task InputAsync(Node element, string value) => DispatchAsync(element, "oninput", new ChangeEventArgs { Value = value });
    public Task ChangeAsync(Node element, object? value) => DispatchAsync(element, "onchange", new ChangeEventArgs { Value = value });
    public Task KeyDownAsync(Node element, string key) => DispatchAsync(element, "onkeydown", new KeyboardEventArgs { Key = key });

    private async Task DispatchAsync(Node element, string eventName, EventArgs args)
    {
        if (!element.Handlers.TryGetValue(eventName, out var id))
            throw new InvalidOperationException($"<{element.Tag}> has no '{eventName}' handler.");
        await Dispatcher.InvokeAsync(() => DispatchEventAsync(id, null, args));
    }

    private sealed class FragmentHost : ComponentBase
    {
        [Parameter] public RenderFragment? Fragment { get; set; }
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) => builder.AddContent(0, Fragment);
    }
}

#pragma warning restore BL0006

public static class NodeQuery
{
    public static IEnumerable<Node> Elements(this Node root, string tag) => root.Descendants().Where(n => n.Tag == tag);
    public static IEnumerable<Node> WithClass(this Node root, string cls) => root.Descendants().Where(n => n.Tag != "#text" && n.HasClass(cls));
    public static Node First(this Node root, string cls) =>
        root.WithClass(cls).FirstOrDefault() ?? throw new InvalidOperationException($"No element with class '{cls}' found.");
    public static string Html(this Node n)
    {
        var sb = new StringBuilder();
        void W(Node x)
        {
            if (x.Text is not null) { sb.Append(x.Text); return; }
            sb.Append('<').Append(x.Tag);
            foreach (var (k, v) in x.Attributes) sb.Append(' ').Append(k).Append("=\"").Append(v).Append('"');
            sb.Append('>');
            foreach (var c in x.Children) W(c);
            sb.Append("</").Append(x.Tag).Append('>');
        }
        W(n);
        return sb.ToString();
    }
}

/// <summary>No-op IJSRuntime for tests — all JS calls are silently swallowed.</summary>
internal sealed class NullJSRuntime : Microsoft.JSInterop.IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => ValueTask.FromResult(default(TValue)!);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, System.Threading.CancellationToken cancellationToken, object?[]? args)
        => ValueTask.FromResult(default(TValue)!);
}
