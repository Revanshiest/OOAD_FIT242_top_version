namespace SpaceBattle.Lib;

public class Vector
{
    public int[] vector;

    public Vector(params int[] nums)
    {
        if (nums.Length == 0) throw new ArgumentException();
        vector = nums.ToArray();
    }

    public int Size => vector.Length;

    public int this[int index]
    {
        get => vector[index];
        set => vector[index] = value;
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size) throw new ArgumentException();
        return new Vector(v1.vector.Zip(v2.vector, (a, b) => a + b).ToArray());
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size) throw new ArgumentException();
        return new Vector(v1.vector.Zip(v2.vector, (a, b) => a - b).ToArray());
    }

    public static Vector operator *(int alfa, Vector v1)
    {
        return new Vector(v1.vector.Select(x => alfa * x).ToArray());
    }

    public static bool operator ==(Vector v1, Vector v2)
    {
        if (ReferenceEquals(v1, v2)) return true;
        if (v1 is null || v2 is null) return false;
        return v1.vector.SequenceEqual(v2.vector);
    }

    public static bool operator !=(Vector v1, Vector v2) => !(v1 == v2);

    public override bool Equals(object? obj) => obj is Vector v && vector.SequenceEqual(v.vector);

    public override int GetHashCode()
    {
        return string.Join("", vector.Select(x => x.ToString())).GetHashCode();
    }
}
