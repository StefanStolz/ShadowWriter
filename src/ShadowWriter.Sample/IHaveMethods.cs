using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowWriter.Sample;

[NullObject]
public interface IHaveMethods {
    int Value { get; }

    int Other { get; set; }

    void Method1(int value) {}

    IEnumerable<string> AnEnumerable();

    public int Method2();

    public Task MethodAsync();

    public ValueTask Method2Async();
}

public partial class NullHaveMethods {
}

public static class UseNullHaveMethods {
    public static void Execute() {
        var item = NullHaveMethods.Instance;

        //Console.WriteLine(item.Value);
    }
}