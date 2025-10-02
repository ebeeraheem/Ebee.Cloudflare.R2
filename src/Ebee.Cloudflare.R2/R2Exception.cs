namespace Ebee.Cloudflare.R2;

/// <summary>
/// Exception thrown by R2 client operations.
/// </summary>
public class R2Exception : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="R2Exception"/> class.
    /// </summary>
    public R2Exception()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="R2Exception"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public R2Exception(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="R2Exception"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public R2Exception(string message, Exception innerException) : base(message, innerException)
    {
    }
}