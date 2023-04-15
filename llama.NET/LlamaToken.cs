namespace llama.NET;

public readonly struct LlamaToken
{
    #region Properties & Fields

    public readonly int Token;

    #endregion

    #region Constructors

    public LlamaToken(int token)
    {
        this.Token = token;
    }

    #endregion
}