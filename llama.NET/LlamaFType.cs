namespace llama.NET;

/// <summary>
/// llama.h: llama_ftype
/// model file types
/// </summary>
public enum LlamaFType
{
    /// <summary>
    /// llama.h: LLAMA_FTYPE_ALL_F32
    /// </summary>
    AllF32 = 0,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_F16
    /// except 1d tensors
    /// </summary>
    MostlyF16 = 1,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q4_0
    /// except 1d tensors
    /// </summary>
    MostlyQ4_0 = 2,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q4_1
    /// except 1d tensors
    /// </summary>
    MostlyQ4_1 = 3,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q4_1_SOME_F16
    /// tok_embeddings.weight and output.weight are F16
    /// </summary>
    MostlyQ4_1SomeF16 = 4,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q4_2
    /// except 1d tensors
    /// </summary>
    MostlyQ4_2 = 5,

    // LLAMA_FTYPE_MOSTLY_Q4_3 (6) support has been removed

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q8_0
    /// except 1d tensors
    /// </summary>
    MostlyQ8_0 = 7,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q5_0
    /// except 1d tensors
    /// </summary>
    MostlyQ5_0 = 8,

    /// <summary>
    /// llama.h: LLAMA_FTYPE_MOSTLY_Q5_1
    /// except 1d tensors
    /// </summary>
    MostlyQ5_1 = 9,
}