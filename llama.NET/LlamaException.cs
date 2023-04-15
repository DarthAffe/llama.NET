namespace llama.NET;

public class LlamaException : Exception
{
    #region Constructors

    public LlamaException()
    { }

    public LlamaException(string? message)
        : base(message)
    { }

    public LlamaException(string? message, Exception? innerException)
        : base(message, innerException)
    { }

    #endregion
}