namespace llama.NET;

public readonly struct LlamaToken : IEquatable<LlamaToken>
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

    #region Methods

    public bool Equals(LlamaToken other) => Token == other.Token;
    public override bool Equals(object? obj) => obj is LlamaToken other && Equals(other);
    public override int GetHashCode() => Token;

    #endregion

    #region Operators

    public static bool operator ==(LlamaToken token1, LlamaToken token2) => token1.Equals(token2);
    public static bool operator !=(LlamaToken token1, LlamaToken token2) => !(token1 == token2);

    public static implicit operator int(LlamaToken token) => token.Token;

    #endregion
}