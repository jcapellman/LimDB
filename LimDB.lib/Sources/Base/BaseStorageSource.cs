using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LimDB.lib.Common;

namespace LimDB.lib.Sources.Base
{
    public abstract class BaseStorageSource(string dbFileName = LibConstants.DefaultDbFileName)
    {
        protected readonly string DbFileName = dbFileName;

        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public abstract Task<string> GetDbAsync();

        protected abstract Task<bool> WriteAsync(ReadOnlyMemory<byte> jsonBytes);

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Fallback to reflection-based JSON serialization when source generation is not available")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Fallback to reflection-based JSON serialization when source generation is not available")]
        public async Task<bool> WriteDbAsync<T>(List<T> objects, JsonTypeInfo<List<T>>? jsonTypeInfo)
        {
            // Use ArrayPool to reduce allocations for large JSON output
            var bufferWriter = new ArrayPoolBufferWriter();
            try
            {
                using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
                {
                    if (jsonTypeInfo != null)
                    {
                        JsonSerializer.Serialize(writer, objects, jsonTypeInfo);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, objects, DefaultJsonSerializerOptions);
                    }
                }

                return await WriteAsync(bufferWriter.WrittenMemory);
            }
            finally
            {
                bufferWriter.Dispose();
            }
        }

        private sealed class ArrayPoolBufferWriter(int initialCapacity = LibConstants.JsonBufferInitialCapacity) : IBufferWriter<byte>, IDisposable
        {
            private byte[] _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            private int _index = 0;

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
                    sizeHint = LibConstants.JsonBufferDefaultSizeHint;
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

