using System.Dynamic;

namespace ShadowWriter.Sample;

[NullObject]
public interface IEmptyInterface {
}

public static class UseNullEmptyInterface {

    public static int XX {
        get => default;
        set => _ = value;
    }

    public static void Execute() {
        var item = NullEmptyInterface.Instance;
    }
}