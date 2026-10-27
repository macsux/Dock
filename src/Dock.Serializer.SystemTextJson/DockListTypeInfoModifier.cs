// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Dock.Model.Core;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// Resolver modifier that makes <see cref="IList{T}"/> properties deserialize into a
/// configured concrete list type without replacing the built-in enumerable path.
/// </summary>
internal static class DockListTypeInfoModifier
{
    /// <summary>
    /// Without reflection: the Dock model's lists (<see cref="IList{IDockable}"/>, <see cref="IList{IDockWindow}"/>) become
    /// <see cref="ObservableCollection{T}"/> or <see cref="List{T}"/>; other lists keep System.Text.Json's default.
    /// </summary>
    public static void Apply(JsonTypeInfo typeInfo, Type listType)
    {
        if (IsList(typeInfo))
        {
            TryAssignAotSafeCreator(typeInfo, typeInfo.Type, listType);
        }
    }

    /// <summary>Like <see cref="Apply"/>, and any other list (or list type) through reflection.</summary>
    [RequiresUnreferencedCode("Lists of a custom list type are created through reflection.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Lists of a custom list type are created through MakeGenericType.")]
#endif
    public static void ApplyWithReflection(JsonTypeInfo typeInfo, Type listType)
    {
        if (!IsList(typeInfo) || TryAssignAotSafeCreator(typeInfo, typeInfo.Type, listType))
        {
            return;
        }

        var elementType = typeInfo.Type.GetGenericArguments()[0];
        var concreteListType = listType.MakeGenericType(elementType);
        typeInfo.CreateObject = () => Activator.CreateInstance(concreteListType)!;
    }

    private static bool IsList(JsonTypeInfo typeInfo) =>
        typeInfo.Kind == JsonTypeInfoKind.Enumerable && typeInfo.Type.IsGenericType && typeInfo.Type.GetGenericTypeDefinition() == typeof(IList<>);

    private static bool TryAssignAotSafeCreator(JsonTypeInfo typeInfo, Type type, Type listType)
    {
        if (listType == typeof(ObservableCollection<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                typeInfo.CreateObject = static () => new ObservableCollection<IDockable>();
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                typeInfo.CreateObject = static () => new ObservableCollection<IDockWindow>();
                return true;
            }
        }

        if (listType == typeof(List<>))
        {
            if (type == typeof(IList<IDockable>))
            {
                typeInfo.CreateObject = static () => new List<IDockable>();
                return true;
            }

            if (type == typeof(IList<IDockWindow>))
            {
                typeInfo.CreateObject = static () => new List<IDockWindow>();
                return true;
            }
        }

        return false;
    }
}
