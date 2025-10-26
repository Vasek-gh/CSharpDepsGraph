using System;

namespace CSharpDepsGraph;

/// <summary>
/// Exception for business error detection. Stack should not be output for business errors
/// </summary>
public class CSharpDepsGraphException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpDepsGraphException"/> class
    /// </summary>
    public CSharpDepsGraphException(string message)
        : base(message)
    {
    }
}