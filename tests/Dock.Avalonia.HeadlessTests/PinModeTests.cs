using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinModeTests
{
    private static (Factory Factory, RootDock Root, ToolDock Pane, Tool Tool) PinnedTool()
    {
        var factory = new Factory();
        var root = (RootDock)factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>();
        var pane = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        var tool = new Tool { Title = "Tool", CanPin = true };
        factory.AddDockable(root, pane);
        factory.AddDockable(pane, tool);
        factory.InitLayout(root);
        factory.PinDockable(tool);
        Assert.True(factory.IsDockablePinned(tool, root));
        return (factory, root, pane, tool);
    }

    private static Button TabButton(ToolPinItemControl item, out Window window)
    {
        window = new Window { Width = 400, Height = 400, Content = item };
        window.Styles.Add(new DockFluentTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return item.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_TabButton");
    }

    [AvaloniaFact]
    public void AutoHide_Tab_Shows_The_Tool_Over_The_Layout()
    {
        var (_, root, pane, tool) = PinnedTool();
        var item = new ToolPinItemControl { DataContext = tool };
        var button = TabButton(item, out var window);

        Assert.DoesNotContain(":hide", item.Classes);
        button.Command!.Execute(button.CommandParameter);

        Assert.Same(tool, root.PinnedDock?.ActiveDockable);
        Assert.DoesNotContain(tool, pane.VisibleDockables!);
        window.Close();
    }

    [AvaloniaFact]
    public void Hide_Tab_Docks_The_Tool_Back_Where_It_Was()
    {
        DockSettings.PinMode = DockPinMode.Hide;
        try
        {
            var (factory, root, pane, tool) = PinnedTool();
            var item = new ToolPinItemControl { DataContext = tool };
            var button = TabButton(item, out var window);

            Assert.Contains(":hide", item.Classes);
            button.Command!.Execute(button.CommandParameter);

            Assert.False(factory.IsDockablePinned(tool, root));
            Assert.Contains(tool, pane.VisibleDockables!);
            Assert.Same(pane, tool.Owner);
            Assert.Null(root.PinnedDock?.ActiveDockable);
            window.Close();
        }
        finally
        {
            DockSettings.PinMode = DockPinMode.AutoHide;
        }
    }
}
