using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LimDB.Benchmarks.Json;
using LimDB.Benchmarks.Objects;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LimDB.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class CollectionTypeSerializationBenchmarks
    {
        private List<BenchmarkPost> _listData = null!;
        private BenchmarkPost[] _arrayData = null!;
        private IReadOnlyList<BenchmarkPost> _readOnlyListData = null!;

        private JsonTypeInfo<List<BenchmarkPost>> _listTypeInfo = null!;
        private JsonTypeInfo<BenchmarkPost[]> _arrayTypeInfo = null!;
        private JsonTypeInfo<IReadOnlyList<BenchmarkPost>> _readOnlyListTypeInfo = null!;

        [Params(100, 1000, 10000)]
        public int ItemCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var posts = new List<BenchmarkPost>();
            for (int i = 1; i <= ItemCount; i++)
            {
                posts.Add(new BenchmarkPost
                {
                    Id = i,
                    Title = $"Post Title {i}",
                    Body = $"This is the body of post {i}. It contains some sample text to make the data more realistic.",
                    Category = $"Category {i % 10}",
                    URL = $"https://example.com/post/{i}",
                    PostDate = DateTime.UtcNow.AddDays(-i),
                    Active = true,
                    Created = DateTime.UtcNow.AddDays(-i),
                    Modified = DateTime.UtcNow.AddDays(-i)
                });
            }

            _listData = posts;
            _arrayData = posts.ToArray();
            _readOnlyListData = posts;

            _listTypeInfo = BenchmarkJsonContext.Default.ListBenchmarkPost;
            _arrayTypeInfo = BenchmarkJsonContext.Default.BenchmarkPostArray;
            _readOnlyListTypeInfo = BenchmarkJsonContext.Default.IReadOnlyListBenchmarkPost;
        }

        [Benchmark(Baseline = true)]
        public int SerializeList()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    JsonSerializer.Serialize(writer, _listData, _listTypeInfo);
                }
                return bufferWriter.WrittenCount;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        [Benchmark]
        public int SerializeArray()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    JsonSerializer.Serialize(writer, _arrayData, _arrayTypeInfo);
                }
                return bufferWriter.WrittenCount;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        [Benchmark]
        public int SerializeIReadOnlyList()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    JsonSerializer.Serialize(writer, _readOnlyListData, _readOnlyListTypeInfo);
                }
                return bufferWriter.WrittenCount;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        [Benchmark]
        public int SerializeListAsIReadOnlyList()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                IReadOnlyList<BenchmarkPost> data = _listData;
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    JsonSerializer.Serialize(writer, data, _readOnlyListTypeInfo);
                }
                return bufferWriter.WrittenCount;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        [Benchmark]
        public int SerializeArrayAsIReadOnlyList()
        {
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                IReadOnlyList<BenchmarkPost> data = _arrayData;
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    JsonSerializer.Serialize(writer, data, _readOnlyListTypeInfo);
                }
                return bufferWriter.WrittenCount;
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        private sealed class ArrayPoolBufferWriter(int initialCapacity = 4096) : IBufferWriter<byte>, IDisposable
        {
            private byte[] _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            private int _index = 0;

            public int WrittenCount => _index;

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
                    sizeHint = 1;
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
