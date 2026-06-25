using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Behaviours;

public class TextBoxBehaviour
{
    private static readonly Dictionary<TextBox, Capture> Associations = new();

    public static bool GetScrollOnTextChanged(DependencyObject dependencyObject) =>
        (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);

    public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value) =>
        dependencyObject.SetValue(ScrollOnTextChangedProperty, value);

    public static readonly DependencyProperty ScrollOnTextChangedProperty =
        DependencyProperty.RegisterAttached(
            name: "ScrollOnTextChanged",
            propertyType: typeof(bool),
            ownerType: typeof(TextBoxBehaviour),
            defaultMetadata: new UIPropertyMetadata(false, OnScrollOnTextChanged));

    private static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not TextBox textBox)
            return;

        bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
        if (newValue == oldValue)
            return;

        if (newValue)
        {
            textBox.Loaded += TextBoxLoaded;
            textBox.Unloaded += TextBoxUnloaded;
        }
        else
        {
            textBox.Loaded -= TextBoxLoaded;
            textBox.Unloaded -= TextBoxUnloaded;

            if (Associations.TryGetValue(textBox, out var association))
                association.Dispose();
        }
    }

    private static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
    {
        var textBox = (TextBox)sender;
        Associations[textBox].Dispose();
        textBox.Unloaded -= TextBoxUnloaded;
    }

    private static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        var textBox = (TextBox)sender;
        textBox.Loaded -= TextBoxLoaded;
        Associations[textBox] = new Capture(textBox);
    }

    private class Capture : IDisposable
    {
        private TextBox TextBox { get; }

        public Capture(TextBox textBox)
        {
            TextBox = textBox;
            TextBox.TextChanged += OnTextBoxOnTextChanged;
        }

        private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
        {
            TextBox.ScrollToEnd();
        }

        public void Dispose()
        {
            TextBox.TextChanged -= OnTextBoxOnTextChanged;
        }
    }
}
