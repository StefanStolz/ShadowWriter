# NullObject Feature Documentation

## Overview
The NullObject feature is a powerful source generator that automatically creates null object implementations for interfaces and classes. This implementation follows the [Null Object Pattern](https://en.wikipedia.org/wiki/Null_object_pattern), providing default "do nothing" implementations that help avoid null reference exceptions and simplify code.

## Quick Start

```csharp
[NullObject]
public partial class MyNullImplementation : IMyInterface
{
}
```

## Features

### Automatic Implementation
- Generates implementations for all interface methods and properties
- Creates thread-safe and immutable implementations
- Uses partial classes to keep your code clean

### Default Behaviors
- Methods return default values for their return types
- Void methods have empty implementations
- Properties return default values for their types
- Collections return empty collections instead of null

## Usage Guidelines

### Basic Implementation
1. Add the `[NullObject]` attribute to your class
2. Declare the class as `partial`
3. Specify the interface to implement

### Example with Interface

```csharp
public interface ILogger
{
    void Log(string message);
    LogLevel CurrentLevel { get; set; }
    bool IsEnabled { get; }
}

[NullObject]
public partial class NullLogger : ILogger
{
    // All interface members will be automatically implemented
    // No manual implementation required
}
```

### Generated Implementation
The source generator will create implementations like this:

```csharp
public partial class NullLogger
{
    public void Log(string message) { }
    public LogLevel CurrentLevel { get; set; }
    public bool IsEnabled => false;
}
```

## Best Practices

### Do's
✅ Use meaningful names for your null implementations  
✅ Keep null objects simple and focused  
✅ Make classes partial when using the `[NullObject]` attribute  
✅ Use null objects for optional dependencies  

### Don'ts
❌ Don't add custom logic to null objects  
❌ Don't use null objects when real implementation is required  
❌ Don't forget to make the class partial  

## Benefits
1. **Reduced Boilerplate**: No need to manually implement "do nothing" behavior
2. **Type Safety**: Compile-time checking for interface implementation
3. **Clean Code**: Separation of generated code using partial classes
4. **Null Safety**: Helps prevent null reference exceptions
5. **Maintainability**: Generated code updates automatically when interfaces change

## Configuration
The NullObject feature works out of the box with no additional configuration required. Simply add the attribute to your class and the source generator will handle the rest.

## Technical Details

### Generated Code Location
- The source generator creates a partial class file at compile time
- Generated files are not visible in your solution but are included in compilation
- You can view generated files in your IDE's source generator output

### Performance Impact
- Compilation time impact is minimal
- Runtime performance is optimal as implementations are simple
- Memory usage is negligible

## Troubleshooting

### Common Issues
1. **Class must be partial**
   ```csharp
   // Error: Class must be partial
   [NullObject]
   public class MyClass : IMyInterface {} // Won't work

   // Correct:
   [NullObject]
   public partial class MyClass : IMyInterface {}
   ```

2. **Multiple Interface Implementation**
   ```csharp
   // Supported: Multiple interfaces
   [NullObject]
   public partial class MyNullObject : IInterface1, IInterface2 {}
   ```

## Version Compatibility
- Supports .NET Standard 2.0 and above
- Works with C# 8.0 and newer
- Compatible with all modern .NET implementations

## See Also
- [Project README](README.md)
- [Null Object Pattern](https://en.wikipedia.org/wiki/Null_object_pattern)
