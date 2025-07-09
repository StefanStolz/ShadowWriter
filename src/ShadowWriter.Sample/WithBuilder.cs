// Copyright © Stefan Stolz, 2025

using System.IO;

namespace ShadowWriter.Sample;

[Builder]
public partial record WithBuilder(int Number);


[Builder]
public partial record WithBuilderNullableInt(int? Number);

[Builder]
public partial record WithBuilderMultiple(int Number, int Number2, bool Enabled);

[Builder]
public partial record WithBuilderWithNonNullableString(string Text, Stream Stream);

