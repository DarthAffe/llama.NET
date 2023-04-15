namespace llama.NET;

/// <summary>
/// llama.h: llama_context_params
/// </summary>
public struct LlamaContextParams
{
    #region Properties & Fields

    /// <summary>
    /// llama.h: n_ctx
    /// text context
    /// </summary>
    public int ContextSize;

    /// <summary>
    /// llama.h: n_parts
    /// -1 for default
    /// </summary>
    public int PartCount;

    /// <summary>
    /// llama.h: seed
    /// RNG seed, 0 for random
    /// </summary>
    public int Seed;

    /// <summary>
    /// llama.h: f16_kv
    /// use fp16 for KV cache
    /// </summary>
    public bool IsKVf16;

    /// <summary>
    /// llama.h: logits_all
    /// the llama_eval() call computes all logits, not just the last one
    /// </summary>
    public bool IsComputingAllLogits;

    /// <summary>
    /// llama.h: vocab_only
    /// only load the vocabulary, no weights
    /// </summary>
    public bool IsVocabOnly;

    /// <summary>
    /// llama.h: use_mmap
    /// use mmap if possible
    /// </summary>
    public bool UseMMap;

    /// <summary>
    /// llama.h: use_mlock
    /// force system to keep model in RAM
    /// </summary>
    public bool UseMLock;

    /// <summary>
    /// llama.h: embedding
    /// embedding mode only
    /// </summary>
    public bool IsEmbeddingMode;

    //TODO DarthAffe 15.04.2023: Is this really needed? Keep private for now.
    /// <summary>
    /// llama.h: progress_callback
    /// called with a progress value between 0 and 1, pass NULL to disable
    /// </summary>
    private nint ProgressCallback;

    /// <summary>
    /// llama.h: progress_callback_user_data
    /// context pointer passed to the progress callback
    /// </summary>
    private nint ProgressCallbackUserData;

    #endregion
}