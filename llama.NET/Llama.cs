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
        _llamaGetStateSize = (delegate* unmanaged[Cdecl]<nint, uint>)LoadFunction("llama_get_state_size");
        _llamaCopyStateData = (delegate* unmanaged[Cdecl]<nint, ref byte, uint>)LoadFunction("llama_copy_state_data");
        _llamaSetStateData = (delegate* unmanaged[Cdecl]<nint, in byte, uint>)LoadFunction("llama_set_state_data");
        _llamaEval = (delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, int, int>)LoadFunction("llama_eval");
        _llamaTokenize = (delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, int, bool, int>)LoadFunction("llama_tokenize");
        _llamaNVocab = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_vocab");
        _llamaNCtx = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_ctx");
        _llamaNEmbd = (delegate* unmanaged[Cdecl]<nint, int>)LoadFunction("llama_n_embd");
        _llamaGetLogits = (delegate* unmanaged[Cdecl]<nint, nint>)LoadFunction("llama_get_logits");
        _llamaGetEmbeddings = (delegate* unmanaged[Cdecl]<nint, nint>)LoadFunction("llama_get_embeddings");
        _llamaTokenToStr = (delegate* unmanaged[Cdecl]<nint, LlamaToken, nint>)LoadFunction("llama_token_to_str");
        _llamaTokenBos = (delegate* unmanaged[Cdecl]<int>)LoadFunction("llama_token_bos");
        _llamaTokenEos = (delegate* unmanaged[Cdecl]<int>)LoadFunction("llama_token_eos");
        _llamaSampleTopPTopK = (delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, float, float, float, int>)LoadFunction("llama_sample_top_p_top_k");
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
        _llamaGetStateSize = null;
        _llamaCopyStateData = null;
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
        _llamaSampleTopPTopK = null;
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
    private static delegate* unmanaged[Cdecl]<nint, uint> _llamaGetStateSize;
    private static delegate* unmanaged[Cdecl]<nint, ref byte, uint> _llamaCopyStateData;
    private static delegate* unmanaged[Cdecl]<nint, in byte, uint> _llamaSetStateData;
    private static delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, int, int> _llamaEval;
    private static delegate* unmanaged[Cdecl]<nint, string, LlamaToken*, int, bool, int> _llamaTokenize;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNVocab;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNCtx;
    private static delegate* unmanaged[Cdecl]<nint, int> _llamaNEmbd;
    private static delegate* unmanaged[Cdecl]<nint, nint> _llamaGetLogits;
    private static delegate* unmanaged[Cdecl]<nint, nint> _llamaGetEmbeddings;
    private static delegate* unmanaged[Cdecl]<nint, LlamaToken, nint> _llamaTokenToStr;
    private static delegate* unmanaged[Cdecl]<int> _llamaTokenBos;
    private static delegate* unmanaged[Cdecl]<int> _llamaTokenEos;
    private static delegate* unmanaged[Cdecl]<nint, LlamaToken*, int, int, float, float, float, int> _llamaSampleTopPTopK;
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

    public static LlamaContext InitFromFile(string modelPath, LlamaContextParams contextParams)
    {
        if (_llamaInitFromFile == null) throw new LlamaException("The library is not loaded.");
        return new LlamaContext(_llamaInitFromFile(modelPath, contextParams));
    }

    public static void Free(LlamaContext context)
    {
        if (_llamaFree == null) throw new LlamaException("The library is not loaded.");
        _llamaFree(context);
    }

    public static int ModelQuantize(string inputName, string outputName, LlamaFType type, int threads = 0)
    {
        if (_llamaModelQuantize == null) throw new LlamaException("The library is not loaded.");
        return _llamaModelQuantize(inputName, outputName, type, threads);
    }

    public static int GetKvCacheTokenCount(LlamaContext context)
    {
        if (_llamaGetKvCacheTokenCount == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetKvCacheTokenCount(context);
    }

    public static int ApplyLoraFromFile(LlamaContext context, string loraPath, string baseModelPath, int threads = 0)
    {
        if (_llamaApplyLoraFromFile == null) throw new LlamaException("The library is not loaded.");
        return _llamaApplyLoraFromFile(context, loraPath, baseModelPath, threads);
    }

    public static uint GetStateSize(LlamaContext context)
    {
        if (_llamaGetStateSize == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetStateSize(context);
    }

    public static uint CopyStateData(LlamaContext context, in Span<byte> destination)
    {
        if (_llamaCopyStateData == null) throw new LlamaException("The library is not loaded.");
        return _llamaCopyStateData(context, ref MemoryMarshal.GetReference(destination));
    }

    public static uint SetStateData(LlamaContext context, in ReadOnlySpan<byte> destination)
    {
        if (_llamaSetStateData == null) throw new LlamaException("The library is not loaded.");
        return _llamaSetStateData(context, in MemoryMarshal.GetReference(destination));
    }

    public static bool Eval(LlamaContext context, in Span<LlamaToken> tokens, int pastTokenCount, int threads)
    {
        if (_llamaEval == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* tokenPtr = tokens)
            return _llamaEval(context, tokenPtr, tokens.Length, pastTokenCount, threads) == 0;
    }

    public static Span<LlamaToken> Tokenize(LlamaContext context, string text, int maxTokenCount, bool addBos)
    {
        if (_llamaTokenize == null) throw new LlamaException("The library is not loaded.");

        //TODO DarthAffe 15.04.2023: Fix this to not allocate that much if not needed and maybe add an overload to provide a buffer to prevent the allocation.
        LlamaToken[] tokens = new LlamaToken[2048];
        fixed (LlamaToken* tokenPtr = tokens)
        {
            int count = _llamaTokenize(context, text, tokenPtr, maxTokenCount, addBos);
            return tokens.AsSpan(0, count);
        }
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

    //TODO DarthAffe 15.04.2023: What is this returning? Comment sounds like a multidimensional array.
    public static nint GetLogits(LlamaContext context)
    {
        if (_llamaGetLogits == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetLogits(context);
    }

    //TODO DarthAffe 15.04.2023: What is this returning? Comment sounds like a multidimensional array like logits?
    public static nint GetEmbeddings(LlamaContext context)
    {
        if (_llamaGetEmbeddings == null) throw new LlamaException("The library is not loaded.");
        return _llamaGetEmbeddings(context);
    }

    public static string TokenToString(LlamaContext context, LlamaToken token)
    {
        if (_llamaTokenToStr == null) throw new LlamaException("The library is not loaded.");
        return Marshal.PtrToStringUTF8(_llamaTokenToStr(context, token)) ?? string.Empty;
    }

    public static int TokenBos()
    {
        if (_llamaTokenBos == null) throw new LlamaException("The library is not loaded.");
        return _llamaTokenBos();
    }

    public static int TokenEos()
    {
        if (_llamaTokenEos == null) throw new LlamaException("The library is not loaded.");
        return _llamaTokenEos();
    }

    public static LlamaToken SampleTopPTopK(LlamaContext context, in Span<LlamaToken> lastTokens, int topK, float topP, float temperature, float repeatPenalty)
    {
        if (_llamaSampleTopPTopK == null) throw new LlamaException("The library is not loaded.");

        fixed (LlamaToken* lastTokenPtr = lastTokens)
            return new LlamaToken(_llamaSampleTopPTopK(context, lastTokenPtr, lastTokens.Length, topK, topP, temperature, repeatPenalty));
    }

    public static void PrintTimings(LlamaContext context)
    {
        if (_llamaPrintTimings == null) throw new LlamaException("The library is not loaded.");
        _llamaPrintTimings(context);
    }

    public static void ResetTimings(LlamaContext context)
    {
        if (_llamaResetTimings == null) throw new LlamaException("The library is not loaded.");
        _llamaResetTimings(context);
    }

    public static string PrintSystemInfo()
    {
        if (_llamaPrintSystemInfo == null) throw new LlamaException("The library is not loaded.");
        return Marshal.PtrToStringUTF8(_llamaPrintSystemInfo()) ?? string.Empty;
    }

    #endregion
}