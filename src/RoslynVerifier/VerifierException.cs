using System;

namespace RoslynVerifier;

public sealed class VerifierException : Exception
{
    public VerifierException(string message)
        : base(message)
    {
    }

    public VerifierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}