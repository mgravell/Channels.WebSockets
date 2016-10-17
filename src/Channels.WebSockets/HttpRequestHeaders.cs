using Channels.Text.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Channels.WebSockets
{
    public struct HttpRequestHeaders : IEnumerable<KeyValuePair<string, ReadableBuffer>>, IDisposable
    {
        private Dictionary<string, PreservedBuffer> headers;
        public void Dispose()
        {
            if (headers != null)
            {
                foreach (var pair in headers)
                    pair.Value.Dispose();
            }
            headers = null;
        }
        internal HttpRequestHeaders(Dictionary<string, PreservedBuffer> headers)
        {
            this.headers = headers;
        }
        public bool ContainsKey(string key) => headers.ContainsKey(key);
        IEnumerator<KeyValuePair<string, ReadableBuffer>> IEnumerable<KeyValuePair<string, ReadableBuffer>>.GetEnumerator() => ((IEnumerable<KeyValuePair<string, ReadableBuffer>>)headers).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)headers).GetEnumerator();
        public Dictionary<string, PreservedBuffer>.Enumerator GetEnumerator() => headers.GetEnumerator();

        public string GetAsciiString(string key)
        {
            PreservedBuffer buffer;
            if (headers.TryGetValue(key, out buffer)) return buffer.Buffer.GetAsciiString();
            return null;
        }
        internal ReadableBuffer GetRaw(string key)
        {
            PreservedBuffer buffer;
            if (headers.TryGetValue(key, out buffer)) return buffer.Buffer;
            return default(ReadableBuffer);
        }

    }
}
