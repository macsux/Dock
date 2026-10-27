// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dock.Model.Core;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Dock.Serializer.SystemTextJson;

/// <summary>
/// A class that implements the <see cref="IDockSerializer"/> interface using JSON serialization.
/// </summary>
public sealed class DockSerializer : IDockSerializer
{
    private readonly JsonSerializerOptions _options;

    private DockSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    [RequiresUnreferencedCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#endif
    public DockSerializer(Type listType)
        : this(DockSerializerOptionsFactory.Create(listType))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class with the specified list type and type info resolver.
    /// </summary>
    /// <param name="listType">The type of list to use in the serialization process.</param>
    /// <param name="typeInfoResolver">The resolver to use for generated metadata.</param>
    [RequiresUnreferencedCode("Lists of a custom list type are created through reflection; DockSerializer(IJsonTypeInfoResolver) needs none.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Lists of a custom list type are created through MakeGenericType; DockSerializer(IJsonTypeInfoResolver) needs none.")]
#endif
    public DockSerializer(Type listType, IJsonTypeInfoResolver typeInfoResolver)
        : this(DockSerializerOptionsFactory.Create(listType, typeInfoResolver))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type and the specified type info resolver.
    /// Trim- and NativeAOT-safe with a source-generated resolver.
    /// </summary>
    /// <param name="typeInfoResolver">The resolver to use for generated metadata.</param>
    public DockSerializer(IJsonTypeInfoResolver typeInfoResolver)
        : this(DockSerializerOptionsFactory.Create(typeInfoResolver))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSerializer"/> class using <see cref="ObservableCollection{T}"/> as the list type.
    /// </summary>
    [RequiresUnreferencedCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("The Dock model's types are discovered and serialized through reflection; use a source-generated resolver (DockJsonSourceGeneration) instead.")]
#endif
    public DockSerializer() : this(typeof(ObservableCollection<>))
    {
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, TypeInfo<T>());
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize(text, TypeInfo<T>());
    }

    /// <summary>The metadata the options' resolver (reflection-based or source-generated) has for <typeparamref name="T"/>.</summary>
    private JsonTypeInfo<T> TypeInfo<T>() => (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T));

    /// <inheritdoc/>
    public T? Load<T>(Stream stream)
    {
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var text = streamReader.ReadToEnd();
        return Deserialize<T>(text);
    }

    /// <inheritdoc/>
    public void Save<T>(Stream stream, T value)
    {
        var text = Serialize(value);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
        streamWriter.Write(text);
    }
}
