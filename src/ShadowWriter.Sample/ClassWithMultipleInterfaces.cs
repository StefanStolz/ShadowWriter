using System;
using System.Collections;

namespace ShadowWriter.Sample;

[NullObject]
public sealed partial class ClassWithMultipleInterfaces : IDisposable, IAsyncDisposable {
}

