using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LimDB.Benchmarks.Objects;
using LimDB.lib.Json;
using Microsoft.VSDiagnostics;

namespace LimDB.Benchmarks
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [CPUUsageDiagnoser]
    public class JsonSerializationPathBenchmarks
    {
        private List<BenchmarkPost> _posts = null!;
        private JsonTypeInfo<List<BenchmarkPost>>? _jsonTypeInfo;
        private JsonSerializerOptions _reflectionOptions = null!;
        private JsonWriterOptions _writerOptions;
        [Params(100, 1000, 10000)]
        public int DatasetSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Create test data
            _posts = new List<BenchmarkPost>();
            for (int i = 0; i < DatasetSize; i++)
            {
                _posts.Add(new BenchmarkPost { Id = i, Active = true, Created = DateTime.UtcNow, Modified = DateTime.UtcNow, Title = $"Post {i}", Body = $"This is the body of post {i} with some content to serialize", Category = $"Category{i % 10}", URL = $"https://example.com/post/{i}", PostDate = DateTime.UtcNow.AddDays(-i) });
            }

            // Try to get source-generated type info
            var typeInfo = LimDbJsonContext.Default.GetTypeInfo(typeof(List<BenchmarkPost>));
            if (typeInfo is JsonTypeInfo<List<BenchmarkPost>> jsonTypeInfo)
            {
                _jsonTypeInfo = jsonTypeInfo;
            }

            // Setup reflection-based options
            _reflectionOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
            // Setup writer options once
            _writerOptions = new JsonWriterOptions
            {
                Indented = false
            };
        }

        [Benchmark(Baseline = true, Description = "BEFORE: Reflection-based (not AOT-compatible)")]
        public int Before_ReflectionSerialization()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, _writerOptions))
                {
                    JsonSerializer.Serialize(writer, _posts, _reflectionOptions);
                }

                return bufferWriter.WrittenMemory.Length;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        [Benchmark(Description = "AFTER: Source Generation (AOT-friendly)")]
        public int After_SourceGenerationSerialization()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, _writerOptions))
                {
                    if (_jsonTypeInfo != null)
                    {
                        JsonSerializer.Serialize(writer, _posts, _jsonTypeInfo);
                    }
                    else
                    {
                        // Fallback shouldn't happen in this benchmark
                        JsonSerializer.Serialize(writer, _posts, _reflectionOptions);
                    }
                }

                return bufferWriter.WrittenMemory.Length;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        private sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
        {
            private byte[] _buffer;
            private int _index;
            private const int InitialCapacity = 256 * 1024;
            private const int DefaultSizeHint = 4096;
            public ArrayPoolBufferWriter()
            {
                _buffer = ArrayPool<byte>.Shared.Rent(InitialCapacity);
                _index = 0;
            }

            public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _index);

            public void Advance(int count)
            {
                _index += count;
            }

            public Memory<byte> GetMemory(int sizeHint = 0)
            {
                CheckAndResizeBuffer(sizeHint);
                return _buffer.AsMemory(_index);
            }

            public Span<byte> GetSpan(int sizeHint = 0)
            {
                CheckAndResizeBuffer(sizeHint);
                return _buffer.AsSpan(_index);
            }

            private void CheckAndResizeBuffer(int sizeHint)
            {
                if (sizeHint == 0)
                {
                    sizeHint = DefaultSizeHint;
                }

                if (_index + sizeHint > _buffer.Length)
                {
                    var newSize = Math.Max(_buffer.Length * 2, _index + sizeHint);
                    var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
                    _buffer.AsSpan(0, _index).CopyTo(newBuffer);
                    ArrayPool<byte>.Shared.Return(_buffer);
                    _buffer = newBuffer;
                }
            }

            public void Dispose()
            {
                if (_buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                    _buffer = null!;
                }
            }
        }
    }
}