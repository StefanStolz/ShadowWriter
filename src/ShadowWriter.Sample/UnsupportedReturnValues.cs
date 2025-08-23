using System;

namespace ShadowWriter.Sample;

public interface IUnsupportedReturnValues
{
    IDisposable SomeMethod();
}

[NullObject]
public partial class UnsupportedReturnValues : IUnsupportedReturnValues
{
    public IDisposable SomeMethod()
    {
        return null!;
    }
}