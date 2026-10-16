using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>A dock control nested in a dockable's content owns its drag and drop areas: the outer one doesn't pick them up.</summary>
public class DockHelpersNestedDockControlTests
{
    private static (IRootDock Root, DocumentDock Documents) Layout(Factory factory, string title, object? content)
    {
        var document = new Document { Id = title, Title = title, Content = content };
        var documents = new DocumentDock { Id = title + "Dock", VisibleDockables = factory.CreateList<IDockable>(document), ActiveDockable = document };
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(documents);
        root.ActiveDockable = documents;
        root.DefaultDockable = documents;
        return (root, documents);
    }

    [AvaloniaFact]
    public void Outer_DockControl_Skips_The_Drag_And_Drop_Areas_Of_A_Nested_One()
    {
        var innerFactory = new Factory();
        var (innerRoot, _) = Layout(innerFactory, "Inner", new TextBlock { Text = "inner content" });
        var inner = new DockControl { Factory = innerFactory, Layout = innerRoot, InitializeLayout = true, InitializeFactory = true };
        var outerFactory = new Factory();
        var (outerRoot, _) = Layout(outerFactory, "Outer", inner);
        var outer = new DockControl { Factory = outerFactory, Layout = outerRoot, InitializeLayout = true, InitializeFactory = true };
        var window = new Window { Width = 600, Height = 400, Content = outer };
        try
        {
            window.Show();
            for (var pass = 0; pass < 5; pass++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }
            var innerTab = inner.GetVisualDescendants().OfType<DocumentTabStripItem>().Single();
            var point = innerTab.TranslatePoint(new Point(innerTab.Bounds.Width / 2, innerTab.Bounds.Height / 2), outer)!.Value;

            window.MouseMove(outer.TranslatePoint(point, window)!.Value);

            var innerDrag = DockHelpers.GetControl(inner, outer.TranslatePoint(point, inner)!.Value, DockProperties.IsDragAreaProperty);
            Assert.NotNull(innerDrag);
            Assert.True(inner.IsVisualAncestorOf(innerDrag!));

            var outerDrag = DockHelpers.GetControl(outer, point, DockProperties.IsDragAreaProperty);
            Assert.False(outerDrag is not null && inner.IsVisualAncestorOf(outerDrag));
            var outerDrop = DockHelpers.GetControl(outer, point, DockProperties.IsDropAreaProperty);
            Assert.False(outerDrop is not null && inner.IsVisualAncestorOf(outerDrop));
        }
        finally { window.Close(); }
    }
}
