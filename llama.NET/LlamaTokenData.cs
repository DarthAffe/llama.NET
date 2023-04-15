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
    /// llama.h: p
    /// probability of the token
    /// </summary>
    public float Probability;

    /// <summary>
    /// llama.h: plog
    /// log probability of the token
    /// </summary>
    public float ProbabilityLog;

    #endregion
}