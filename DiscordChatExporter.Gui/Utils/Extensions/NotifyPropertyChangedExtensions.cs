﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace DiscordChatExporter.Gui.Utils.Extensions;

internal static class NotifyPropertyChangedExtensions
{
    public static IDisposable WatchProperty<TOwner, TProperty>(
        this TOwner owner,
        Expression<Func<TOwner, TProperty>> propertyExpression,
        Action callback,
        bool watchInitialValue = true
    )
        where TOwner : INotifyPropertyChanged
    {
        var memberExpression =
            propertyExpression.Body as MemberExpression
            // Property value might be boxed inside a conversion expression, if the types don't match
            ?? (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression;

        if (memberExpression?.Member is not PropertyInfo property)
            throw new ArgumentException("Provided expression must reference a property.");

        void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (
                string.IsNullOrWhiteSpace(args.PropertyName)
                || string.Equals(args.PropertyName, property.Name, StringComparison.Ordinal)
            )
            {
                callback();
            }
        }

        owner.PropertyChanged += OnPropertyChanged;

        if (watchInitialValue)
            callback();

        return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
    }

    public static IDisposable WatchAllProperties<TOwner>(
        this TOwner owner,
        Action callback,
        bool watchInitialValues = true
    )
        where TOwner : INotifyPropertyChanged
    {
        void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) => callback();
        owner.PropertyChanged += OnPropertyChanged;

        if (watchInitialValues)
            callback();

        return Disposable.Create(() => owner.PropertyChanged -= OnPropertyChanged);
    }
}
