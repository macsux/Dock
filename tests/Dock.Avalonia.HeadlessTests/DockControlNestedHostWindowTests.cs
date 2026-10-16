using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// A dock control inside a floating window's content (a layout nested in a dockable that was floated) leaves the window
/// to the layout it already shows.
/// </summary>
public class DockControlNestedHostWindowTests
{
    [AvaloniaFact]
    public void Nested_DockControl_Does_Not_Take_Over_The_Host_Window()
    {
        var outerFactory = new Factory();
        var outer = outerFactory.CreateLayout();
        outer.Factory = outerFactory;
        var outerWindow = outerFactory.CreateWindowFrom(outer)!;
        outerWindow.Layout = outer;
        var host = new TestHostWindow();
        outerFactory.InitDockWindow(outerWindow, outer, host);

        var innerFactory = new Factory();
        var inner = innerFactory.CreateLayout();
        inner.Factory = innerFactory;
        host.Content = new DockControl { Factory = innerFactory, Layout = inner };
        host.Show();

        Assert.Same(outerWindow, host.Window);
        Assert.Same(host, outerWindow.Host);
        Assert.NotNull(inner.Window);
        Assert.Null(inner.Window!.Host);
        host.Close();
    }

    [AvaloniaFact]
    public void DockControl_Binds_A_Host_Window_That_Has_No_Layout_Yet()
    {
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;
        var host = new TestHostWindow { Content = new DockControl { Factory = factory, Layout = layout } };
        host.Show();

        Assert.NotNull(layout.Window);
        Assert.Same(host, layout.Window!.Host);
        Assert.Same(layout.Window, host.Window);
        host.Close();
    }
}
