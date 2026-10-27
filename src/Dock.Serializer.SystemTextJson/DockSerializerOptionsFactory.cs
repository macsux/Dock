// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

internal static class DockSerializerOptionsFactory
{
    [RequiresUnreferencedCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#endif
    public static JsonSerializerOptions Create(Type listType)
    {
        if (listType is null)
        {
            throw new ArgumentNullException(nameof(listType));
        }

        return Create(listType, new DockModelPolymorphicTypeResolver());
    }

    [RequiresUnreferencedCode("Lists of a custom list type are created through reflection.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Lists of a custom list type are created through MakeGenericType.")]
#endif
    public static JsonSerializerOptions Create(Type listType, IJsonTypeInfoResolver typeInfoResolver)
    {
        if (listType is null)
        {
            throw new ArgumentNullException(nameof(listType));
        }

        return Build(typeInfoResolver, typeInfo => DockListTypeInfoModifier.ApplyWithReflection(typeInfo, listType));
    }

    /// <summary>Options over a source-generated resolver, lists as <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>: no reflection.</summary>
    public static JsonSerializerOptions Create(IJsonTypeInfoResolver typeInfoResolver)
    {
        var listType = typeof(System.Collections.ObjectModel.ObservableCollection<>);
        return Build(typeInfoResolver, typeInfo => DockListTypeInfoModifier.Apply(typeInfo, listType));
    }

    private static JsonSerializerOptions Build(IJsonTypeInfoResolver typeInfoResolver, Action<JsonTypeInfo> listModifier)
    {
        if (typeInfoResolver is null)
        {
            throw new ArgumentNullException(nameof(typeInfoResolver));
        }

        return new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            TypeInfoResolver = typeInfoResolver.WithAddedModifier(listModifier)
        };
    }
}
