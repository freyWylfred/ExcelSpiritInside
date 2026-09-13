using System.Text.Json;
using System.Text.Json.Serialization;
using LLama.Common;
using LLama.Sampling;

namespace ExcelSpiritInside
{
    public sealed class LlmSettings
    {
        [JsonPropertyName("n_gpu_layers")] public int GpuLayers { get; set; } = 0;
        [JsonPropertyName("main_gpu")] public int MainGpu { get; set; } = 0;
        [JsonPropertyName("split_mode")] public string SplitMode { get; set; } = "none";
        [JsonPropertyName("use_mlock")] public bool UseMlock { get; set; } = false;
        [JsonPropertyName("use_mmap")] public bool UseMmap { get; set; } = true;
        [JsonPropertyName("n_ctx")] public uint ContextSize { get; set; } = 4096;
        [JsonPropertyName("n_batch")] public uint BatchSize { get; set; } = 512;
        [JsonPropertyName("n_ubatch")] public uint UBatchSize { get; set; } = 512;
        [JsonPropertyName("n_threads")] public int Threads { get; set; } = 0;
        [JsonPropertyName("n_threads_batch")] public int BatchThreads { get; set; } = 0;
        [JsonPropertyName("flash_attention")] public bool FlashAttention { get; set; } = false;
        [JsonPropertyName("seed")] public uint Seed { get; set; } = 1337;
        [JsonPropertyName("rope_freq_base")] public float? RopeFrequencyBase { get; set; }
        [JsonPropertyName("rope_freq_scale")] public float? RopeFrequencyScale { get; set; }

        [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; } = 512;
        [JsonPropertyName("temperature")] public float Temperature { get; set; } = 0.7f;
        [JsonPropertyName("top_k")] public int TopK { get; set; } = 40;
        [JsonPropertyName("top_p")] public float TopP { get; set; } = 0.9f;
        [JsonPropertyName("min_p")] public float MinP { get; set; } = 0.05f;
        [JsonPropertyName("repeat_penalty")] public float RepeatPenalty { get; set; } = 1.1f;
        [JsonPropertyName("repeat_last_n")] public int RepeatLastN { get; set; } = 64;
        [JsonPropertyName("frequency_penalty")] public float FrequencyPenalty { get; set; } = 0f;
        [JsonPropertyName("presence_penalty")] public float PresencePenalty { get; set; } = 0f;
        [JsonPropertyName("anti_prompts")] public List<string> AntiPrompts { get; set; } = new() { "\nUser:", "User:" };
        [JsonPropertyName("system_prompt")] public string SystemPrompt { get; set; } = "You are a helpful assistant for Excel data.";

        public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "llm-settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static LlmSettings Load()
        {
            var path = SettingsPath;
            try
            {
                if (!File.Exists(path))
                {
                    var defaults = new LlmSettings();
                    File.WriteAllText(path, JsonSerializer.Serialize(defaults, JsonOptions));
                    Program.Log($"[Settings] Created default settings at {path}");
                    return defaults;
                }

                var loaded = JsonSerializer.Deserialize<LlmSettings>(File.ReadAllText(path), JsonOptions) ?? new LlmSettings();
                Program.Log($"[Settings] Loaded {path}");
                return loaded;
            }
            catch (Exception ex)
            {
                Program.Log($"[Settings] Failed to load {path}: {ex.Message}. Using defaults.");
                return new LlmSettings();
            }
        }

        public ModelParams ToModelParams(string modelPath)
        {
            var threads = Threads > 0 ? Threads : Math.Max(1, Environment.ProcessorCount / 2);
            var p = new ModelParams(modelPath)
            {
                ContextSize = ContextSize,
                BatchSize = BatchSize,
                UBatchSize = UBatchSize,
                GpuLayerCount = GpuLayers,
                MainGpu = MainGpu,
                SplitMode = ParseSplitMode(SplitMode),
                UseMemorymap = UseMmap,
                UseMemoryLock = UseMlock,
                Threads = threads,
                BatchThreads = BatchThreads > 0 ? BatchThreads : threads,
                FlashAttention = FlashAttention
            };
            if (RopeFrequencyBase.HasValue) p.RopeFrequencyBase = RopeFrequencyBase;
            if (RopeFrequencyScale.HasValue) p.RopeFrequencyScale = RopeFrequencyScale;
            return p;
        }

        public InferenceParams ToInferenceParams()
        {
            return new InferenceParams
            {
                MaxTokens = MaxTokens,
                AntiPrompts = AntiPrompts,
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Seed = Seed,
                    Temperature = Temperature,
                    TopK = TopK,
                    TopP = TopP,
                    MinP = MinP,
                    RepeatPenalty = RepeatPenalty,
                    PenaltyCount = RepeatLastN,
                    FrequencyPenalty = FrequencyPenalty,
                    PresencePenalty = PresencePenalty
                }
            };
        }

        private static LLama.Native.GPUSplitMode ParseSplitMode(string value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "layer" => LLama.Native.GPUSplitMode.Layer,
                "row" => LLama.Native.GPUSplitMode.Row,
                _ => LLama.Native.GPUSplitMode.None
            };
        }
    }
}
