namespace ShadowWriter.Sample;

public interface IDerivedInterface : IHaveMethods
{
    void DoSomething();
}

[NullObject]
public partial class NullDerivedInterface : IDerivedInterface
{

}

public class UseNullDerivedInterface(NullDerivedInterface item)
{
    public void Execute()
    {
        item.DoSomething();
        item.Method1(23);
    }
}