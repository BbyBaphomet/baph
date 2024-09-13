using LiteNetLib.Utils;
using BaphsFika.Plugin.Networking.Packets;
using BaphsFika.Plugin.Models;
using UnityEngine;

public class PlayerStatePacket : BasePacket
{
    public int PlayerId { get; set; }
    public Vector3? Position { get; set; }
    public Quaternion? Rotation { get; set; }
    public Vector3? Velocity { get; set; }
    public float? Health { get; set; }
    public string Stance { get; set; }
    public string CurrentWeaponId { get; set; }
    public bool IsCompressed { get; set; }

    public override void Serialize(NetDataWriter writer)
    {
        base.Serialize(writer);
        writer.Put(PlayerId);
        writer.PutVector3Nullable(Position);
        writer.PutQuaternionNullable(Rotation);
        writer.PutVector3Nullable(Velocity);
        writer.Put(Health.HasValue);
        if (Health.HasValue) writer.Put(Health.Value);
        writer.Put(Stance);
        writer.Put(CurrentWeaponId);
        writer.Put(IsCompressed);
    }

    public override void Deserialize(NetDataReader reader)
    {
        base.Deserialize(reader);
        PlayerId = reader.GetInt();
        Position = reader.GetVector3Nullable();
        Rotation = reader.GetQuaternionNullable();
        Velocity = reader.GetVector3Nullable();
        Health = reader.GetBool() ? reader.GetFloat() : (float?)null;
        Stance = reader.GetString();
        CurrentWeaponId = reader.GetString();
        IsCompressed = reader.GetBool();
    }
}

public static class NetDataWriterExtensions
{
    public static void PutVector3Nullable(this NetDataWriter writer, Vector3? value)
    {
        writer.Put(value.HasValue);
        if (value.HasValue)
        {
            writer.Put(value.Value.x);
            writer.Put(value.Value.y);
            writer.Put(value.Value.z);
        }
    }

    public static void PutQuaternionNullable(this NetDataWriter writer, Quaternion? value)
    {
        writer.Put(value.HasValue);
        if (value.HasValue)
        {
            writer.Put(value.Value.x);
            writer.Put(value.Value.y);
            writer.Put(value.Value.z);
            writer.Put(value.Value.w);
        }
    }
}

public static class NetDataReaderExtensions
{
    public static Vector3? GetVector3Nullable(this NetDataReader reader)
    {
        if (reader.GetBool())
        {
            return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        return null;
    }

    public static Quaternion? GetQuaternionNullable(this NetDataReader reader)
    {
        if (reader.GetBool())
        {
            return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        return null;
    }
}