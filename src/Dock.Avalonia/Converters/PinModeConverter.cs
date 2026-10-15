// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Settings;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converter that tells whether <see cref="DockSettings.PinMode"/> is the given mode (the bound value is ignored): for
/// menu items that only apply to one pin mode.
/// </summary>
public class PinModeConverter : IValueConverter
{
    /// <summary>
    /// True when pinned tools auto hide.
    /// </summary>
    public static readonly PinModeConverter AutoHide = new(DockPinMode.AutoHide);

    /// <summary>
    /// True when pinned tools hide (their tab docks them back).
    /// </summary>
    public static readonly PinModeConverter Hide = new(DockPinMode.Hide);

    private readonly DockPinMode _mode;

    private PinModeConverter(DockPinMode mode)
    {
        _mode = mode;
    }

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DockSettings.PinMode == _mode;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
