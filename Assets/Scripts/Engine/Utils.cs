using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;


[Serializable]
public struct Vector3Serializable {
    public Vector3Serializable(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public float x;
    public float y;
    public float z;
    public static implicit operator Vector3(Vector3Serializable v) {
        return new Vector3(v.x, v.y, v.z);
    }
    public static implicit operator Vector2(Vector3Serializable v) {
        return new Vector2(v.x, v.y);
    }
    public static implicit operator Vector3Serializable(Vector3 v) {
        return new Vector3Serializable(v.x, v.y, v.z);
    }
}

[Serializable]
public struct Vector2Serializable {
    public Vector2Serializable(float x, float y) {
        this.x = x;
        this.y = y;
    }
    public float x;
    public float y;
    public static implicit operator Vector2(Vector2Serializable v) {
        return new Vector2(v.x, v.y);
    }
    public static implicit operator Vector2Serializable(Vector2 v) {
        return new Vector2Serializable(v.x, v.y);
    }
    public static implicit operator Vector3(Vector2Serializable v) {
        return new Vector3(v.x, v.y, 0.0f);
    }
    
}

public class Utils {
    public class IdGen {
        public static int nextId {
            get {
                if (m_id == 0) {
                    m_id = 10000;
                }

                return ++m_id;
            }
        }

        public static void ResetId(int id) {
            m_id = id;
        }

        static int m_id = 0;
    }

    public static int ToInt(double v) {
        return (int)((v > 0.0 ? 0.5 : -0.5) + v);
    }

    public static double RandomValue(double base_, double rangeRate) {
        return ((s_rnd.NextDouble() - 0.5) * rangeRate + 1) * base_;
    }
    public static System.Random Random {
        get {
            return s_rnd;
        }
    }
    static System.Random s_rnd = new System.Random();

    public static float perPixel {
        get {
            return Camera.main.orthographicSize / Screen.height;
        }
    }

    public static Vector2 HalfCameraSize {
        get {
            float aspectRatio = Screen.width * 1.0f / Screen.height;
            float hy = Camera.main.orthographicSize;
            float hx = hy * aspectRatio;
            return new Vector2(hx, hy);
        }
    }

    public static Vector2 GetDirectionPoint(Vector2 from, float radian, float distance) {
        return new Vector2(from.x + Mathf.Cos(-radian) * distance, from.y + Mathf.Sin(radian) * distance);
    }

    public static Vector2 GetForwardPoint(Vector2 from, Vector2 to, float distance) {
        float a = Mathf.Atan2(to.y - from.y, to.x - from.x);
        return GetDirectionPoint(from, a, distance);
    }

    public static float GetAngle(Vector2 from, Vector2 to) {
        return Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
    }

    public static float GetAngle(Vector2 detal) {
        return Mathf.Atan2(detal.y, detal.x) * Mathf.Rad2Deg;
    }
#if false
    public static byte[] Serialize(object obj)
    {
        IFormatter fmt = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        fmt.Serialize(stream, obj);
        byte[] data = new byte[stream.Length];
        Array.Copy(stream.GetBuffer(), data, data.Length);
        return data;
    }
#endif
    public static byte[][] Serialize(object obj, out int total, int seg = 1024) {
        Debug.Assert(seg > 1);
        IFormatter fmt = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        fmt.Serialize(stream, obj);
        stream.Position = 0;
        total = (int)stream.Length;
        //Debug.LogFormat("ToSend: {0}B", total);
        byte[][] data = new byte[(total + (seg - 1)) / seg][];
        int index = 0;
        while (total > 0) {
            int size = total > seg ? seg : total;
            data[index] = new byte[size];
            stream.Read(data[index], 0, size);
            total -= size;
            ++index;
        }
        return data;
    }

    public static object Deserialize(byte[] data) {
        MemoryStream stream = new MemoryStream(data);
        IFormatter fmt = new BinaryFormatter();
        object obj = fmt.Deserialize(stream);
        return obj;
    }
}
