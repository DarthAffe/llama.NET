namespace llama.NET;

/// <summary>
/// llama.h: llama_token_data
/// </summary>
public struct LlamaTokenData
{
    #region Properties & Fields

    /// <summary>
    /// llama.h: id
    /// token id
    /// </summary>
    public int Id;

    /// <summary>
    /// llama.h: logit
    /// log-odds of the token
    /// </summary>
    public float Logit;

    /// <summary>
    /// llama.h: p
    /// probability of the token
    /// </summary>
    public float Probability;

    #endregion

    #region Constructors
    
    public LlamaTokenData(LlamaToken token, float logit, float probability)
        : this(token.Token, logit, probability) { }

    public LlamaTokenData(int id, float logit, float probability)
    {
        this.Id = id;
        this.Logit = logit;
        this.Probability = probability;
    }

    #endregion
}