namespace llama.NET;

internal unsafe struct _LlamaTokenDataArray
{
    #region Properties & Fields

    public LlamaTokenData* Data;

    public uint Size;

    public bool Sorted;

    #endregion

    #region Constructors
    
    public _LlamaTokenDataArray(LlamaTokenData* data, uint size, bool sorted)
    {
        this.Data = data;
        this.Size = size;
        this.Sorted = sorted;
    }

    #endregion
}

public ref struct LlamaTokenDataArray
{
    #region Properties & Fields

    public Span<LlamaTokenData> Data;

    public bool Sorted;

    #endregion

    #region Constructors

    public LlamaTokenDataArray(Span<LlamaTokenData> data, bool sorted = false)
    {
        this.Data = data;
        this.Sorted = sorted;
    }

    #endregion
}