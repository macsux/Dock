using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolChromeHeaderActionsTests
{
    [AvaloniaFact]
    public void Header_Shows_The_Active_Tools_Own_Buttons_Only_When_Given_A_Template()
    {
        var factory = new Factory();
        var dock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>(), Factory = factory };
        var tool = new Tool { Title = "Tool" };
        factory.AddDockable(dock, tool);
        dock.ActiveDockable = tool;
        var chrome = new ToolChromeControl { DataContext = dock };
        var window = new Window { Width = 400, Height = 300, Content = chrome };
        window.Styles.Add(new DockFluentTheme());
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var actions = chrome.GetVisualDescendants().OfType<ContentControl>().Single(c => c.Name == "PART_HeaderActions");

        // By default nothing: the dockable isn't shown through the application's data templates.
        Assert.Same(tool, actions.Content);
        Assert.Null(actions.Presenter!.Child);

        chrome.HeaderActionsTemplate = new FuncDataTemplate<Tool>((t, _) => new Button { Content = t.Title });
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("Tool", Assert.IsType<Button>(actions.Presenter!.Child).Content);
        window.Close();
    }
}
