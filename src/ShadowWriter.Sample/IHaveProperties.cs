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
}

[NullObject]
public partial class NullHaveProperties : IHaveProperties{
}