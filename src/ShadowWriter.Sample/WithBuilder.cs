// Copyright © Stefan Stolz, 2025

using System.Collections.Generic;
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


[Builder]
public partial record WithBuilderAndNullableArgs(string Text, string? OtherText);


[Builder(RequireStrings = true, RequireValueTypes = true)]
public partial record WithBuilderRequired(string Text, int Number, bool Enabled);


[Builder]
public partial record WithBuilderCollections(int[] Tags, List<string> Names, IEnumerable<int> Ids);

