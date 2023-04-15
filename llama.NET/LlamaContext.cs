namespace llama.NET;

public readonly struct LlamaContext
{
    #region Properties & Fields

    private readonly nint _context;

    #endregion

    #region Constructors

    internal LlamaContext(nint context)
    {
        this._context = context;
    }

    #endregion

    #region Operators

    public static implicit operator nint(LlamaContext context) => context._context;

    #endregion
}
