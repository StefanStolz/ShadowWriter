using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowWriter.Sample;

[NullObject]
public interface IHaveMethods {
    int Value { get; }

    void Method1(int value) {}

    IEnumerable<string> AnEnumerable();

    public int Method2();

    public Task MethodAsync();

    public ValueTask Method2Async();
}

public partial class NullHaveMethods {
    // public partial IEnumerable<string> AnEnumerable() {
    //     var x = this.Method2();
    //
    //     yield break;
    // }

    public int Value => 0;
}

public static class UseNullHaveMethods {

    public static void Execute() {
        // var item = NullHaveMethods.Instance;
        //
        // item.Method1(0);
        //
        // item.AnEnumerable();
    }
}