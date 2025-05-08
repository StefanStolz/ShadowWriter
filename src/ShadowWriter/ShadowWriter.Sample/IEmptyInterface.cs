namespace ShadowWriter.Sample;

[NullObject]
public interface IEmptyInterface {

}

public static class UseNullEmptyInterface {
    public static void Execute() {
        var item = NullEmptyInterface.Instance;


    }
}