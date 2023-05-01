using System.Runtime.InteropServices;

namespace llama.NET;

public static unsafe class Llama
{
    #region Properties & Fields

    public static List<string> NativeSearchPaths { get; } = new()
    {
        "llama.dll",
        "llama.so",
        "llama.dylib",
    };

    private static nint _libHandle = 0;

    #endregion

    #region Methods

    public static void Load()
    {
        if (_libHandle != 0) return;

        List<string> possiblePathList = GetPossibleLibraryPaths().ToList();

        string? libPath = possiblePathList.FirstOrDefault(File.Exists);
        if (libPath == null) throw new LlamaException($"Can't find the Llama native library at one of the expected locations:\r\n '{string.Join("\r\n", possiblePathList.Select(Path.GetFullPath))}'");

        if (!NativeLibrary.TryLoad(libPath, out _libHandle))
            throw new LlamaException($"Llama LoadLibrary failed with error code {Marshal.GetLastPInvokeError()}");

        _llamaContextDefaultParams = (delegate* unmanaged[Cdecl]<LlamaContextParams>)LoadFunction("llama_context_default_params");
        _llamaMMapSupported = (delegate* unmanaged[Cdecl]<bool>)LoadFunction("llama_mmap_supported");
        _llamaMMockSupported = (delegate* unmanaged[Cdecl]<bool>)LoadFunction("llama_mlock_supported");
        _llamaInitFromFile = (delegate* unmanaged[Cdecl]<string, LlamaContextParams, nint>)LoadFunction("llama_init_from_file");
        _llamaFree = (delegate* unmanaged[Cdecl]<nint, void>)LoadFunction("llama_free");
        _llamaModelQuantize = (delegate* unmanaged[Cdecl]<string, string, LlamaFType, int, int>)LoadFunction("llama_model_quantize");
        _llamaApplyLoraFromFile = (delegate* unmanaged[Cdecl]<nint, string, string, int, int>)LoadFunction("llama_apply_lora_from_file");
        _llamaGetKvCacheTokenCount = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_get_kv_cache_token_count");
        _llamaSetRngSeed = (delegate* unmanaged[Cdecl]<nint, int, void>)LoadFunction("llama_set_rng_seed");
        _llamaGetStateSize = (delegate* unmanaged[Cdecl]<nint, uint>)LoadFunction("llama_get_state_size");
        _llamaCopyStateData = (delegate* unmanaged[Cdecl]<nint, ref byte, uint>)LoadFunction("llama_copy_state_data");
        _llamaLoadSessionFile = (delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, uint, out uint, bool>)LoadFunction("llama_load_session_file");
        _llamaSaveSessionFile = (delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, uint, bool>)LoadFunction("llama_save_session_file");
        _llamaSetStateData = (delegate* unmanaged[Cdecl]<nint, in byte, uint>)LoadFunction("llama_set_state_data");
        _llamaEval = (delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, int, int>)LoadFunction("llama_eval");
        _llamaTokenize = (delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, int, bool, int>)LoadFunction("llama_tokenize");
        _llamaNVocab = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_vocab");
        _llamaNCtx = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_ctx");
        _llamaNEmbd = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_embd");
        _llamaGetLogits = (delegate* unmanaged[Cdecl]<nint, float*>)LoadFunction("llama_get_logits");
        _llamaGetEmbeddings = (delegate* unmanaged[Cdecl]<nint, float*>)LoadFunction("llama_get_embeddings");
        _llamaTokenToStr = (delegate* unmanaged[Cdecl]<nint, LlamaToken, nint>)LoadFunction("llama_token_to_str");
        _llamaTokenBos = (delegate* unmanaged[Cdecl]<int>)LoadFunction("llama_token_bos");
        _llamaTokenEos = (delegate* unmanaged[Cdecl]<int>)LoadFunction("llama_token_eos");
        _llamaTokenN1 = (delegate* unmanaged[Cdecl]<int>)LoadFunction("llama_token_nl");
        _llamaSampleRepetitionPenalty = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, LlamaToken*, uint, float, void>)LoadFunction("llama_sample_repetition_penalty");
        _llamaSampleFrequencyAndPresencePenalties = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, LlamaToken*, uint, float, float, void>)LoadFunction("llama_sample_frequency_and_presence_penalties");
        _llamaSampleSoftmax = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, void>)LoadFunction("llama_sample_softmax");
        _llamaSampleTopK = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int, uint, void>)LoadFunction("llama_sample_top_k");
        _llamaSampleTopP = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void>)LoadFunction("llama_sample_top_p");
        _llamaSampleTailFree = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void>)LoadFunction("llama_sample_tail_free");
        _llamaSampleTypical = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void>)LoadFunction("llama_sample_typical");
        _llamaSampleTemperature = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, void>)LoadFunction("llama_sample_temperature");
        _llamaSampleTokenMirostat = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, float, int, ref float, int>)LoadFunction("llama_sample_token_mirostat");
        _llamaSampleTokenMirostatV2 = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, float, ref float, int>)LoadFunction("llama_sample_token_mirostat_v2");
        _llamaSampleTokenGreedy = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int>)LoadFunction("llama_sample_token_greedy");
        _llamaSampleToken = (delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int>)LoadFunction("llama_sample_token");
        _llamaPrintTimings = (delegate* unmanaged[Cdecl]<nint, void>)LoadFunction("llama_print_timings");
        _llamaResetTimings = (delegate* unmanaged[Cdecl]<nint, void>)LoadFunction("llama_reset_timings");
        _llamaPrintSystemInfo = (delegate* unmanaged[Cdecl]<nint>)LoadFunction("llama_print_system_info");
    }

    private static nint LoadFunction(string function)
    {
        if (!NativeLibrary.TryGetExport(_libHandle, function, out nint ptr)) throw new LlamaException($"Failed to load Llama function '{function}'");
        return ptr;
    }

    public static void Unload()
    {
        if (_libHandle == 0) return;

        _llamaContextDefaultParams = null;
        _llamaMMapSupported = null;
        _llamaMMockSupported = null;
        _llamaInitFromFile = null;
        _llamaFree = null;
        _llamaModelQuantize = null;
        _llamaApplyLoraFromFile = null;
        _llamaGetKvCacheTokenCount = null;
        _llamaSetRngSeed = null;
        _llamaGetStateSize = null;
        _llamaCopyStateData = null;
        _llamaLoadSessionFile = null;
        _llamaSaveSessionFile = null;
        _llamaSetStateData = null;
        _llamaEval = null;
        _llamaTokenize = null;
        _llamaNVocab = null;
        _llamaNCtx = null;
        _llamaNEmbd = null;
        _llamaGetLogits = null;
        _llamaGetEmbeddings = null;
        _llamaTokenToStr = null;
        _llamaTokenBos = null;
        _llamaTokenEos = null;
        _llamaTokenN1 = null;
        _llamaSampleRepetitionPenalty = null;
        _llamaSampleFrequencyAndPresencePenalties = null;
        _llamaSampleSoftmax = null;
        _llamaSampleTopK = null;
        _llamaSampleTopP = null;
        _llamaSampleTailFree = null;
        _llamaSampleTypical = null;
        _llamaSampleTemperature = null;
        _llamaSampleTokenMirostat = null;
        _llamaSampleTokenMirostatV2 = null;
        _llamaSampleTokenGreedy = null;
        _llamaSampleToken = null;
        _llamaPrintTimings = null;
        _llamaResetTimings = null;
        _llamaPrintSystemInfo = null;

        NativeLibrary.Free(_libHandle);
        _libHandle = 0;
    }

    private static IEnumerable<string> GetPossibleLibraryPaths()
    {
        IEnumerable<string> possibleLibraryPaths;
        if (OperatingSystem.IsWindows())
            possibleLibraryPaths = NativeSearchPaths.Where(x => string.Equals(Path.GetExtension(x), ".dll", StringComparison.OrdinalIgnoreCase));
        else if (OperatingSystem.IsMacOS())
            possibleLibraryPaths = NativeSearchPaths.Where(x => string.Equals(Path.GetExtension(x), ".dylib", StringComparison.OrdinalIgnoreCase));
        else
            possibleLibraryPaths = NativeSearchPaths.Where(x => string.Equals(Path.GetExtension(x), ".so", StringComparison.OrdinalIgnoreCase));

        return possibleLibraryPaths.Select(Environment.ExpandEnvironmentVariables);
    }

    #endregion

    #region Pointers

    private static delegate* unmanaged[Cdecl]<LlamaContextParams> _llamaContextDefaultParams;
    private static delegate* unmanaged[Cdecl]<bool> _llamaMMapSupported;
    private static delegate* unmanaged[Cdecl]<bool> _llamaMMockSupported;
    private static delegate* unmanaged[Cdecl]<string, LlamaContextParams, nint> _llamaInitFromFile;
    private static delegate* unmanaged[Cdecl]<nint, void> _llamaFree;
    private static delegate* unmanaged[Cdecl]<string, string, LlamaFType, int, int> _llamaModelQuantize;
    private static delegate* unmanaged[Cdecl]<nint, string, string, int, int> _llamaApplyLoraFromFile;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaGetKvCacheTokenCount;
    private static delegate* unmanaged[Cdecl]<nint, int, void> _llamaSetRngSeed;
    private static delegate* unmanaged[Cdecl]<nint, uint> _llamaGetStateSize;
    private static delegate* unmanaged[Cdecl]<nint, ref byte, uint> _llamaCopyStateData;
    private static delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, uint, out uint, bool> _llamaLoadSessionFile;
    private static delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, uint, bool> _llamaSaveSessionFile;
    private static delegate* unmanaged[Cdecl]<nint, in byte, uint> _llamaSetStateData;
    private static delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, int, int> _llamaEval;
    private static delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, int, bool, int> _llamaTokenize;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNVocab;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNCtx;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNEmbd;
    private static delegate* unmanaged[Cdecl]<nint, float*> _llamaGetLogits;
    private static delegate* unmanaged[Cdecl]<nint, float*> _llamaGetEmbeddings;
    private static delegate* unmanaged[Cdecl]<nint, LlamaToken, nint> _llamaTokenToStr;
    private static delegate* unmanaged[Cdecl]<int> _llamaTokenBos;
    private static delegate* unmanaged[Cdecl]<int> _llamaTokenEos;
    private static delegate* unmanaged[Cdecl]<int> _llamaTokenN1;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, LlamaToken*, uint, float, void> _llamaSampleRepetitionPenalty;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, LlamaToken*, uint, float, float, void> _llamaSampleFrequencyAndPresencePenalties;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, void> _llamaSampleSoftmax;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int, uint, void> _llamaSampleTopK;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void> _llamaSampleTopP;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void> _llamaSampleTailFree;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, uint, void> _llamaSampleTypical;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, void> _llamaSampleTemperature;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, float, int, ref float, int> _llamaSampleTokenMirostat;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, float, float, ref float, int> _llamaSampleTokenMirostatV2;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int> _llamaSampleTokenGreedy;
    private static delegate* unmanaged[Cdecl]<nint, _LlamaTokenDataArray, int> _llamaSampleToken;
    private static delegate* unmanaged[Cdecl]<nint, void> _llamaPrintTimings;
    private static delegate* unmanaged[Cdecl]<nint, void> _llamaResetTimings;
    private static delegate* unmanaged[Cdecl]<nint> _llamaPrintSystemInfo;

    #endregion

    #region Lib-Methods

    public static LlamaContextParams GetContextDefaultParams()
    {
        if (_llamaContextDefaultParams == null) throw new LlamaException("The library is not loaded.");
        return _llamaContextDefaultParams();
    }

    public static bool IsMMapSupported()
    {
        if (_llamaMMapSupported == null) throw new LlamaException("The library is not loaded.");
        return _llamaMMapSupported();
    }

    public static bool IsMMockSupported()
    {
        if (_llamaMMockSupported == null) throw new LlamaException("The library is not loaded.");
        return _llamaMMockSupported();
    }

    /// <summary>
    /// Various functions for loading a ggml llama model.
    /// Allocate (almost) all memory needed for the model.
    /// Return NULL on failure.
    /// </summary>
    public static LlamaContext InitFromFile(string modelPath, LlamaContextParams contextParams)
    {
        if (_llamaInitFromFile == null) throw new LlamaException("The library is not loaded.");
        return new LlamaContext(_llamaInitFromFile(modelPath, contextParams));
    }

    /// <summary>
    /// Frees all allocated memory
    /// </summary>
    public static void Free(LlamaContext context)
    {
        if (_llamaFree == null) throw new LlamaException("The library is not loaded.");
        _llamaFree(context);
    }

    /// <param name="threads">how many threads to use. If &lt;=0, will use std::thread::hardware_concurrency(), else the number given.</param>
    /// <returns>Returns 0 on success.</returns>
    public static int ModelQuantize(string inputName, string outputName, LlamaFType type, int threads = 0)
    {
        if (_llamaModelQuantize == null) throw new LlamaException("The library is not loaded.");
        return _llamaModelQuantize(inputName, outputName, type, threads);
    }

    /// <summary>
    /// Apply a LoRA adapter to a loaded model.
    /// path_base_model is the path to a higher quality model to use as a base for.
    /// the layers modified by the adapter. Can be NULL to use the current loaded model.
    /// The model needs to be reloaded before applying a new adapter, otherwise the adapter.
    /// will be applied on top of the previous one.
    /// </summary>
    public static int ApplyLoraFromFile(LlamaContext context, string loraPath, string baseModelPath, int threads = 0)
    {
        if (_llamaApplyLoraFromFile == null) throw new LlamaException("The library is not loaded.");
        return _llamaApplyLoraFromFile(context, loraPath, baseModelPath, threads);
    }

    /// <summary>
    /// Returns the number of tokens in the KV cache.
    /// </summary>
    public static int GetKvCacheTokenCount(LlamaContext context)
    {
        if (_llamaGetKvCacheTokenCount == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetKvCacheTokenCount(context);
    }

    /// <summary>
    /// Sets the current rng seed.
    /// </summary>
    public static void SetRngSeed(LlamaContext context, int seed)
    {
        if (_llamaSetRngSeed == null) throw new LlamaException("The library is not loaded.");
        _llamaSetRngSeed(context, seed);
    }

    /// <summary>
    /// Returns the size in bytes of the state (rng, logits, embedding and kv_cache).
    /// </summary>
    public static uint GetStateSize(LlamaContext context)
    {
        if (_llamaGetStateSize == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetStateSize(context);
    }

    /// <summary>
    /// Copies the state to the specified destination.
    /// Destination needs to have allocated enough memory.
    /// Returns the number of bytes copied.
    /// </summary>
    public static uint CopyStateData(LlamaContext context, in Span<byte> destination)
    {
        if (_llamaCopyStateData == null) throw new LlamaException("The library is not loaded.");
        return _llamaCopyStateData(context, ref MemoryMarshal.GetReference(destination));
    }

    /// <summary>
    /// Set the state reading from the specified address.
    /// Returns the number of bytes read.
    /// </summary>
    public static uint SetStateData(LlamaContext context, in ReadOnlySpan<byte> data)
    {
        if (_llamaSetStateData == null) throw new LlamaException("The library is not loaded.");
        return _llamaSetStateData(context, in MemoryMarshal.GetReference(data));
    }

    /// <summary>
    /// Load session file.
    /// </summary>
    public static bool LoadSessionFile(LlamaContext context, string path, Span<LlamaToken> tokenBuffer, out int tokenCount)
    {
        if (_llamaLoadSessionFile == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = tokenBuffer)
        {
            bool result = _llamaLoadSessionFile(context, path, tokenPtr, (uint)tokenBuffer.Length, out uint tCount);
            tokenCount = (int)tCount;
            return result;
        }
    }

    /// <summary>
    /// Save session file.
    /// </summary>
    public static bool SaveSessionFile(LlamaContext context, string path, ReadOnlySpan<LlamaToken> tokens)
    {
        if (_llamaSaveSessionFile == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = tokens)
            return _llamaSaveSessionFile(context, path, tokenPtr, (uint)tokens.Length);
    }

    /// <summary>
    /// Run the llama inference to obtain the logits and probabilities for the next token.
    /// tokens is the provided batch of new tokens to process.
    /// pastTokenCount is the number of tokens to use from previous eval calls.
    /// Returns 0 on success.
    /// </summary>
    public static bool Eval(LlamaContext context, in ReadOnlySpan<LlamaToken> tokens, int pastTokenCount, int threads)
    {
        if (_llamaEval == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = tokens)
            return _llamaEval(context, tokenPtr, tokens.Length, pastTokenCount, threads) == 0;
    }

    /// <summary>
    /// Convert the provided text into tokens.
    /// Returns the number of tokens on success, no more than maxTokenCount.
    /// Returns a negative number on failure - the number of tokens that would have been returned.
    /// </summary>
    public static Span<LlamaToken> Tokenize(LlamaContext context, string text, int maxTokenCount, bool addBos)
    {
        if (_llamaTokenize == null) throw new LlamaException("The library is not loaded.");

        LlamaToken[] tokens = new LlamaToken[maxTokenCount];
        int count = Tokenize(context, text, tokens, addBos);
        return tokens.AsSpan(0, count);
    }

    /// <summary>
    /// Convert the provided text into tokens.
    /// Returns the number of tokens on success, no more than buffer.Length.
    /// Returns a negative number on failure - the number of tokens that would have been returned.
    /// </summary>
    public static int Tokenize(LlamaContext context, string text, in Span<LlamaToken> buffer, bool addBos)

    {
        if (_llamaTokenize == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = buffer)
            return _llamaTokenize(context, text, tokenPtr, buffer.Length, addBos);
    }

    public static int GetVocabCount(LlamaContext context)
    {
        if (_llamaNVocab == null) throw new LlamaException("The library is not loaded.");
        return _llamaNVocab(context);
    }

    public static int GetContextCount(LlamaContext context)
    {
        if (_llamaNCtx == null) throw new LlamaException("The library is not loaded.");
        return _llamaNCtx(context);
    }

    public static int GetEmbeddingsCount(LlamaContext context)
    {
        if (_llamaNEmbd == null) throw new LlamaException("The library is not loaded.");
        return _llamaNEmbd(context);
    }

    /// <summary>
    /// Token logits obtained from the last call to llama_eval().
    /// The logits for the last token are stored in the last row.
    /// Can be mutated in order to change the probabilities of the next token.
    /// Rows: n_tokens
    /// Cols: n_vocab
    /// </summary>
    public static Span<float> GetLogits(LlamaContext context)
    {
        if (_llamaGetLogits == null) throw new LlamaException("The library is not loaded.");
        //TODO DarthAffe 01.05.2023: I'm still not sure how exactly the size of the logit array and it's layout is determined, but for the current usage asuming a 1D array with the vocab-count as size seems to be fine.
        return new Span<float>(_llamaGetLogits(context), GetVocabCount(context));
    }

    /// <summary>
    /// Get the embeddings for the input.
    /// shape: [n_embd] (1-dimensional)
    /// </summary>
    public static Span<float> GetEmbeddings(LlamaContext context)
    {
        if (_llamaGetEmbeddings == null) throw new LlamaException("The library is not loaded.");
        return new Span<float>(_llamaGetEmbeddings(context), GetEmbeddingsCount(context));
    }

    /// <summary>
    /// Token Id -> String. Uses the vocabulary in the provided context.
    /// </summary>
    public static string TokenToString(LlamaContext context, LlamaToken token)
    {
        if (_llamaTokenToStr == null) throw new LlamaException("The library is not loaded.");
        return Marshal.PtrToStringUTF8(_llamaTokenToStr(context, token)) ?? string.Empty;
    }

    public static LlamaToken TokenBos()
    {
        if (_llamaTokenBos == null) throw new LlamaException("The library is not loaded.");
        return new LlamaToken(_llamaTokenBos());
    }

    public static LlamaToken TokenEos()
    {
        if (_llamaTokenEos == null) throw new LlamaException("The library is not loaded.");
        return new LlamaToken(_llamaTokenEos());
    }

    public static LlamaToken TokenNl()
    {
        if (_llamaTokenN1 == null) throw new LlamaException("The library is not loaded.");
        return new LlamaToken(_llamaTokenN1());
    }

    /// <summary>
    /// Repetition penalty described in CTRL academic paper https://arxiv.org/abs/1909.05858, with negative logit fix.
    /// </summary>
    public static void SampleRepetitionPenalty(LlamaContext context, in LlamaTokenDataArray candidates, in ReadOnlySpan<LlamaToken> lastTokens, float penalty)
    {
        if (_llamaSampleRepetitionPenalty == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = lastTokens)
        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleRepetitionPenalty(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), tokenPtr, (uint)lastTokens.Length, penalty);
    }

    /// <summary>
    /// Frequency and presence penalties described in OpenAI API https://platform.openai.com/docs/api-reference/parameter-details.
    /// </summary>
    public static void SampleFrequencyAndPresencePenalties(LlamaContext context, in LlamaTokenDataArray candidates, in ReadOnlySpan<LlamaToken> lastTokens, float alphaFrequency, float alphaPresance)
    {
        if (_llamaSampleFrequencyAndPresencePenalties == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = lastTokens)
        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleFrequencyAndPresencePenalties(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), tokenPtr, (uint)lastTokens.Length, alphaFrequency, alphaPresance);
    }

    /// <summary>
    /// Sorts candidate tokens by their logits in descending order and calculate probabilities based on logits.
    /// </summary>
    public static void SampleSoftmax(LlamaContext context, in LlamaTokenDataArray candidates)
    {
        if (_llamaSampleSoftmax == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleSoftmax(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted));
    }

    /// <summary>
    /// Top-K sampling described in academic paper "The Curious Case of Neural Text Degeneration" https://arxiv.org/abs/1904.09751.
    /// </summary>
    public static void SampleTopK(LlamaContext context, in LlamaTokenDataArray candidates, int k, uint minKeep = 1)
    {
        if (_llamaSampleTopK == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleTopK(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), k, minKeep);
    }

    /// <summary>
    /// Nucleus sampling described in academic paper "The Curious Case of Neural Text Degeneration" https://arxiv.org/abs/1904.09751.
    /// </summary>
    public static void SampleTopP(LlamaContext context, in LlamaTokenDataArray candidates, float p, uint minKeep = 1)
    {
        if (_llamaSampleTopP == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleTopP(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), p, minKeep);
    }

    /// <summary>
    /// Tail Free Sampling described in https://www.trentonbricken.com/Tail-Free-Sampling/.
    /// </summary>
    public static void SampleTailFree(LlamaContext context, in LlamaTokenDataArray candidates, float z, uint minKeep = 1)
    {
        if (_llamaSampleTailFree == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleTailFree(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), z, minKeep);
    }

    /// <summary>
    /// Locally Typical Sampling implementation described in the paper https://arxiv.org/abs/2202.00666.
    /// </summary>
    public static void SampleTypical(LlamaContext context, in LlamaTokenDataArray candidates, float p, uint minKeep = 1)
    {
        if (_llamaSampleTypical == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleTypical(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), p, minKeep);
    }

    /// <summary>
    /// Locally Typical Sampling implementation described in the paper https://arxiv.org/abs/2202.00666.
    /// </summary>
    public static void SampleTemperature(LlamaContext context, in LlamaTokenDataArray candidates, float temperature)
    {
        if (_llamaSampleTemperature == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            _llamaSampleTemperature(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), temperature);
    }

    /// <summary>
    /// Mirostat 1.0 algorithm described in the paper https://arxiv.org/abs/2007.14966. Uses tokens instead of words.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="candidates">A vector of `llama_token_data` containing the candidate tokens, their probabilities (p), and log-odds (logit) for the current position in the generated text.</param>
    /// <param name="tau">The target cross-entropy (or surprise) value you want to achieve for the generated text. A higher value corresponds to more surprising or less predictable text, while a lower value corresponds to less surprising or more predictable text.</param>
    /// <param name="eta">The learning rate used to update `mu` based on the error between the target and observed surprisal of the sampled word. A larger learning rate will cause `mu` to be updated more quickly, while a smaller learning rate will result in slower updates.</param>
    /// <param name="m">The number of tokens considered in the estimation of `s_hat`. This is an arbitrary value that is used to calculate `s_hat`, which in turn helps to calculate the value of `k`. In the paper, they use `m = 100`, but you can experiment with different values to see how it affects the performance of the algorithm.</param>
    /// <param name="mu">Maximum cross-entropy. This value is initialized to be twice the target cross-entropy (`2 * tau`) and is updated in the algorithm based on the error between the target and observed surprisal.</param>
    public static LlamaToken SampleTokenMirostat(LlamaContext context, in LlamaTokenDataArray candidates, float tau, float eta, int m, ref float mu)
    {
        if (_llamaSampleTokenMirostat == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            return new LlamaToken(_llamaSampleTokenMirostat(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), tau, eta, m, ref mu));
    }

    /// <summary>
    /// Mirostat 2.0 algorithm described in the paper https://arxiv.org/abs/2007.14966. Uses tokens instead of words.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <param name="candidates">candidates A vector of `llama_token_data` containing the candidate tokens, their probabilities (p), and log-odds (logit) for the current position in the generated text.</param>
    /// <param name="tau">The target cross-entropy (or surprise) value you want to achieve for the generated text. A higher value corresponds to more surprising or less predictable text, while a lower value corresponds to less surprising or more predictable text.</param>
    /// <param name="eta">The learning rate used to update `mu` based on the error between the target and observed surprisal of the sampled word. A larger learning rate will cause `mu` to be updated more quickly, while a smaller learning rate will result in slower updates.</param>
    /// <param name="mu">Maximum cross-entropy. This value is initialized to be twice the target cross-entropy (`2 * tau`) and is updated in the algorithm based on the error between the target and observed surprisal.</param>
    public static LlamaToken SampleTokenMirostatV2(LlamaContext context, in LlamaTokenDataArray candidates, float tau, float eta, ref float mu)
    {
        if (_llamaSampleTokenMirostatV2 == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            return new LlamaToken(_llamaSampleTokenMirostatV2(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted), tau, eta, ref mu));
    }

    /// <summary>
    /// Selects the token with the highest probability.
    /// </summary>
    public static LlamaToken SampleTokenGreedy(LlamaContext context, in LlamaTokenDataArray candidates)
    {
        if (_llamaSampleTokenGreedy == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            return new LlamaToken(_llamaSampleTokenGreedy(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted)));
    }

    /// <summary>
    /// Randomly selects a token from the candidates based on their probabilities.
    /// </summary>
    public static LlamaToken SampleToken(LlamaContext context, in LlamaTokenDataArray candidates)
    {
        if (_llamaSampleToken == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaTokenData* tokenData = candidates.Data)
            return new LlamaToken(_llamaSampleToken(context, new _LlamaTokenDataArray(tokenData, (uint)candidates.Data.Length, candidates.Sorted)));
    }

    /// <summary>
    /// Performance information.
    /// </summary>
    public static void PrintTimings(LlamaContext context)
    {
        if (_llamaPrintTimings == null) throw new LlamaException("The library is not loaded.");
        _llamaPrintTimings(context);
    }

    /// <summary>
    /// Performance information.
    /// </summary>
    public static void ResetTimings(LlamaContext context)
    {
        if (_llamaResetTimings == null) throw new LlamaException("The library is not loaded.");
        _llamaResetTimings(context);
    }

    /// <summary>
    /// Print system information.
    /// </summary>
    public static string PrintSystemInfo()
    {
        if (_llamaPrintSystemInfo == null) throw new LlamaException("The library is not loaded.");
        return Marshal.PtrToStringUTF8(_llamaPrintSystemInfo()) ?? string.Empty;
    }

    #endregion
}