using System;
using System.Collections.Generic;

namespace ShadowWriter.Sample;

public interface IHaveProperties
{
    string Text { get; }
    string TextWithSetter { get; set; }
    int Number { get; }
    int NumberWithSetter { get; set; }
    bool Bool { get; }
    bool BoolWithSetter { get; set; }
    IEnumerable<string> AnEnumerable { get; }
    SomeStruct Struct { get; }

   IDisposable Ab { get; }
}

public readonly struct SomeStruct
{
    public SomeStruct(int value, string text)
    {
        this.Value = value;
        this.Text = text;
    }

    public int Value { get; }
    public string Text { get; }
}

[NullObject]
public partial class NullHaveProperties : IHaveProperties
{
    public IDisposable Ab { get; } = null!;
}