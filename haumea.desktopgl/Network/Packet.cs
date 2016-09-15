using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;


namespace Haumea.Network
{
    [ProtoContract]
    public class Ack : Payload
    {
        [ProtoMember(1)]
        public uint Bitfield { get; set; }
        public Ack() { }
        public Ack(uint field)
        {
            Bitfield = field;
        }
    }
    [ProtoContract]
    public class Handshake : Payload
    {
        [ProtoMember(1)]
        public uint Bitfield { get; set; }
        public Handshake() { }

    }
    [ProtoContract]
    public class Player : Payload
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int Age { get; set; }
        public Player() { }
        public Player(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
    [ProtoContract]
    public abstract class Payload { }
    [ProtoContract]
    public class Packet
    {
        public static void Configure()
        {
            IDictionary<int, Type> payloadTypes = new Dictionary<int, Type>    {
                { 1,typeof(Ack) },
                { 2,typeof(Handshake) },
                { 3,typeof(Player) }
            };
            foreach (var entry in payloadTypes)
            {
                RuntimeTypeModel.Default[typeof(Payload)].AddSubType(entry.Key, entry.Value);
            }
        }
        [ProtoMember(1)]
        public uint Seq { get; set; }
        [ProtoMember(2)]
        public Payload[] Payload { get; private set; }

        public Packet() { }

        public Packet(uint seq, Payload[] payload)
        {
            Seq = seq;
            Payload = payload;
        }
        public Packet(uint seq, List<Payload> payload)
        {
            Seq = seq;
            Payload = payload.ToArray();
        }
        public byte[] Serialize()
        {
            byte[] result;
            using (var stream = new System.IO.MemoryStream())
            {
                Serializer.Serialize(stream, this);
                result = stream.ToArray();
            }
            return result;
        }
        public static Packet Deserialize(byte[] message)
        {
            Packet result = null;
            using (var stream = new System.IO.MemoryStream(message))
            {
                try
                {
                    result = Serializer.Deserialize<Packet>(stream);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return result;
        }
    }
}
