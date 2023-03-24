//using Axis.Luna.Extensions;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Axis.Ion.Conversion
{
    /// <summary>
    /// 
    /// </summary>
    public struct ConversionContext
    {
        /// <summary>
        /// For tracking objects during serialization so cyclic reference issues do not occur.
        /// </summary>
        private readonly ObjectIDGenerator objectTracker;

        /// <summary>
        /// Object map, used during de-serialization, for de-referencing instances using their ids.
        /// </summary>
        private readonly Dictionary<string, object?> objectMap = new Dictionary<string, object?>();

        /// <summary>
        /// 
        /// </summary>
        public ConversionOptions Options { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// 
        /// </summary>
        public ContextMetadata Metadata { get; }

        /// <summary>
        /// Attempts to START tracking an object.
        /// </summary>
        /// <param name="object">The object to be tracked</param>
        /// <param name="id">The unique id given to the object by the internal tracker</param>
        /// <returns>true if the object is newly tracked, false if the object has previously been tracked</returns>
        public bool TryTrack(object @object, out string id)
        {
            id = objectTracker
                .GetId(@object, out var added)
                .ToString("x");

            return added;
        }

        /// <summary>
        /// Gets the object with the given id, ignoring the producer function, or if the id isn't added,
        /// attempts to invoke the producer to generate the object
        /// </summary>
        /// <param name="objectId">The object's id</param>
        /// <param name="producer">The object producer function</param>
        /// <returns>The object that was added to the internal map</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public object? GetOrAdd(string objectId, Func<string, object?>? producer = null)
        {
            return objectMap.GetOrAdd(objectId, key =>
            {
                if (producer is null)
                    throw new ArgumentNullException($"{nameof(producer)} is null");

                return producer?.Invoke(key);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="options"></param>
        /// <param name="metadata"></param>
        internal ConversionContext(
            ConversionOptions options,
            ContextMetadata? metadata = null)
            : this(-1, options, metadata)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="options"></param>
        /// <param name="metadata"></param>
        private ConversionContext(
            int depth,
            ConversionOptions options,
            ContextMetadata? metadata = null)
        {
            Depth = depth;
            Options = options;
            Metadata = metadata ?? new ContextMetadata();
            objectTracker = new ObjectIDGenerator();
        }

        public ConversionContext Next() => new ConversionContext(Depth + 1, Options, Metadata);

        public ConversionContext Next(int depth) => new ConversionContext(depth, Options, Metadata);

        public ConversionContext Next(int depth, ConversionOptions options) => new ConversionContext(depth, options, Metadata);

        public ConversionContext Next(int depth, ConversionOptions options, ContextMetadata metadata) => new ConversionContext(depth, options, metadata);
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContextMetadata
    {
        private readonly Dictionary<string, Metadata> metadataMap = new Dictionary<string, Metadata>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Keys => metadataMap.Keys;

        /// <summary>
        /// 
        /// </summary>
        public int Count => metadataMap.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAddMetadata(string key, string data, out Guid id)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (metadataMap.TryGetValue(key, out var metadata))
            {
                id = metadata.Id;
                return false;
            }

            metadata = new Metadata(data);
            id = metadata.Id;
            metadataMap[key] = metadata;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryGetMetadata(string key, out string? data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (metadataMap.TryGetValue(key, out var metadata))
            {
                data = metadata.Data;
                return true;
            }

            data = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryRemoveMetadata(string key, Guid id, out string? data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (metadataMap.TryGetValue(key, out var metadata)
                && metadata.Id == id)
            {
                metadataMap.Remove(key);
                data = metadata.Data;
                return true;
            }

            data = null;
            return false;
        }

        private struct Metadata
        {
            public string Data { get; }

            public Guid Id { get; }

            public Metadata(string data)
            {
                Id = Guid.NewGuid();
                Data = data;
            }
        }
    }
}
