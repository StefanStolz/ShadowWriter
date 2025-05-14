using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowWriter.Sample;

public interface IMyInterface {
    void VoidMethod();

    Task TaskMethod();

    IEnumerable<int> GeneratorFunction();

    
}