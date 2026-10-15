// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Settings;

/// <summary>
/// What pinning a tool does. Either way the tool collapses to a tab on its side of the layout; the mode decides what
/// clicking that tab does.
/// </summary>
public enum DockPinMode
{
    /// <summary>
    /// Auto hide: clicking the tab shows the tool over the layout, until it loses focus.
    /// </summary>
    AutoHide,

    /// <summary>
    /// Hide: clicking the tab docks the tool back where it was. The pin button reads "Hide".
    /// </summary>
    Hide
}
