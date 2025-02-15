﻿using System;
using System.IO;
using System.Threading.Tasks;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
    public class MsgPackObjectSerializer : ISerializer
    {
        private readonly System.Text.Encoding encoding;

        public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null, System.Text.Encoding encoding = null)
        {
            customSerializerRegistrar?.Invoke(SerializationContext.Default.Serializers);

            if (encoding == null)
            {
                this.encoding = System.Text.Encoding.UTF8;
            }
        }

        public Task<object> DeserializeAsync(byte[] serializedObject)
        {
            return DeserializeAsync<object>(serializedObject);
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(encoding.GetString(serializedObject), typeof(T));
            }

            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream(serializedObject))
            {
                return serializer.Unpack(byteStream);
            }
        }

        public Task<T> DeserializeAsync<T>(byte[] serializedObject)
        {
            if (typeof(T) == typeof(string))
            {
                return Task.FromResult((T)Convert.ChangeType(encoding.GetString(serializedObject), typeof(T)));
            }

            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream(serializedObject))
            {
                return serializer.UnpackAsync(byteStream);
            }
        }

        public byte[] Serialize(object item)
        {
            if (item is string)
            {
                return encoding.GetBytes(item.ToString());
            }

            var serializer = MessagePackSerializer.Get(item.GetType());

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);

                return byteStream.ToArray();
            }
        }

        public Task<byte[]> SerializeAsync(object item)
        {
            if (item is string)
            {
                return Task.FromResult(encoding.GetBytes(item.ToString()));
            }

            var serializer = MessagePackSerializer.Get(item.GetType());

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);

                return Task.FromResult(byteStream.ToArray());
            }
        }

        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }
    }
}
