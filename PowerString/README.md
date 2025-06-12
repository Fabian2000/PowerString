# PowerString

A memory-efficient, deterministic string implementation for .NET that uses native memory allocation instead of the managed heap.

## Overview

PowerString provides deterministic memory management for string operations, eliminating garbage collection pressure while maintaining a familiar string-like API. It's designed for scenarios where memory control is more important than raw performance.

## Key Features

- **Deterministic Memory Control**: Memory is freed immediately when disposed, not when GC decides
- **Zero Garbage Collection Pressure**: String operations don't impact GC performance  
- **Full Span Integration**: Complete support for `ReadOnlySpan<char>` and `Span<char>` operations
- **Comprehensive String Operations**: Full set of string manipulation methods (Replace, Contains, Append, etc.)
- **Safe Memory Management**: Automatic cleanup with `using` statements, no memory leaks

## When to Use PowerString

### ✅ Use PowerString when:
- **Immediate memory cleanup** is required (can't wait for GC)
- Working with many strings that must be freed immediately after use
- Memory-constrained environments where GC pressure is problematic
- Real-time systems where GC pauses are unacceptable
- You need deterministic memory management over performance

### ❌ Use regular String when:
- **Performance** is the primary concern (String is faster for most operations)
- Working with small to medium strings
- Standard applications where GC pressure is acceptable
- Simple string operations without complex memory requirements

## Installation

```bash
# Install via NuGet (example)
dotnet add package PowerStrings
```

No special configuration required! PowerString handles all unsafe operations internally while providing a completely safe public API.

## Usage

### Basic Operations

```csharp
using PowerStrings;

// Create PowerString instances
using var ps1 = PowerString.From("Hello");
using var ps2 = PowerString.From("World");

// Or use implicit conversion
PowerString ps3 = "Hello World";

// String operations
ps1.Append(" ");
ps1.Append(ps2);
ps1.ToUpper();
ps1.Replace("HELLO", "Hi");

Console.WriteLine(ps1.ToString()); // "Hi WORLD"
```

### Advanced String Processing

```csharp
// Process an array of emails with zero GC pressure
string[] emails = GetThousandsOfEmails();
var results = new List<string>();

foreach (var email in emails)
{
    using var ps = PowerString.From(email);
    ps.ToLower();                           // In-place modification
    ps.Replace("@gmail.com", "@google.com"); // Native memory reallocation
    
    var atIndex = ps.IndexOf('@');
    using var username = PowerString.From(ps.AsSpan()[..atIndex]);
    username.Append("_processed");
    
    results.Add(username.ToString());      // Only final allocation
    // ps and username memory freed immediately
}
```

### File Processing Example

```csharp
string content = File.ReadAllText("large-file.txt");

using var processor = PowerString.From(content);
processor.Replace("old-text", "new-text");
processor.ToLower();
processor.Replace("\r\n", "\n");

File.WriteAllText("processed-file.txt", processor.ToString());
// Memory freed immediately, no waiting for GC
```

## API Reference

### Creation Methods

```csharp
PowerString.From(string str)           // From string
PowerString.From(char[] array)         // From char array  
PowerString.From(ReadOnlySpan<char>)   // From span
PowerString.Empty()                    // Empty instance

// Implicit conversions
PowerString ps = "Hello";              // From string literal
PowerString ps = charArray;            // From char array
```

### String Operations

```csharp
// Modification (in-place when possible)
ps.Append(string text)
ps.Append(PowerString other)
ps.Append(char[] array)
ps.Append(ReadOnlySpan<char> span)

ps.Prepend(string text)
ps.Prepend(PowerString other)
ps.Prepend(char[] array)
ps.Prepend(ReadOnlySpan<char> span)

ps.Insert(int index, string text)
ps.Insert(int index, PowerString other)
ps.Insert(int index, char[] array)
ps.Insert(int index, ReadOnlySpan<char> span)

ps.Replace(PowerString oldValue, PowerString newValue)
ps.Replace(char oldChar, char newChar)
ps.ToLower()
ps.ToUpper()

// Search Operations
ps.Contains(PowerString text)
ps.Contains(char character)
ps.StartsWith(PowerString text)
ps.StartsWith(char character)
ps.EndsWith(PowerString text)
ps.EndsWith(char character)
ps.IndexOf(PowerString text)
ps.IndexOf(char character)
ps.LastIndexOf(PowerString text)
ps.LastIndexOf(char character)

// All search methods also accept StringComparison parameter

// Conversion
ps.ToString()                          // Convert to string
ps.ToCharArray()                       // Convert to char array
ps.AsSpan()                           // Get ReadOnlySpan<char> view
```

### Properties

```csharp
ps.Length                             // String length
ps.IsEmpty                            // True if empty
ps[int index]                         // Character access (get/set)
```

## Performance Characteristics

### Memory Usage
- **String**: Allocates on managed heap, GC cleanup later
- **PowerString**: Native memory allocation, immediate cleanup

### Performance Trade-offs
- **String**: Faster for most operations (highly optimized .NET runtime)
- **PowerString**: Better memory control and predictable cleanup timing

## Resource Management

PowerString provides deterministic resource cleanup:

- **RAII Pattern**: Automatic cleanup with `using` statements
- **Exception Safety**: Memory is properly freed even when exceptions occur
- **No Resource Leaks**: All native memory allocations are tracked and released
- **Explicit Control**: Call `Dispose()` manually when needed

```csharp
using var ps = PowerString.From("Hello");
ps.Append(" World");
// Memory automatically freed at end of scope
```

## Thread Safety

PowerString instances are **not thread-safe**. Each thread should use its own PowerString instances or implement external synchronization.

## Limitations

1. **Manual disposal required** - Must use `using` or call `Dispose()` explicitly
2. **Not suitable for simple operations** - Regular strings are faster for basic tasks
3. **No automatic interning** - Unlike string literals

## Contributing

Contributions are welcome! Please ensure that:
- All memory allocations are properly cleaned up
- Exception safety is maintained
- Performance implications are documented
- Unit tests cover new functionality

## License

[Your License Here]

## Acknowledgments

Built with modern .NET features including:
- `System.Runtime.InteropServices.NativeMemory`
- `ReadOnlySpan<T>` and `Span<T>`
- Unsafe C# for native memory access