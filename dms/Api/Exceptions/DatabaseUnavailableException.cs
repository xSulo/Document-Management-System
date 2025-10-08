using System;

namespace dms.Api.Exceptions;

public class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
