using Godot;
using FixMath.NET;
using System.Text.RegularExpressions;

[GlobalClass]
public partial class Fix64GD : RefCounted
{
    public Fix64 value;

    // Constants
    public static Fix64GD zero()
    {
        Fix64GD ret = new();
        ret.value = Fix64.Zero;
        return ret;
    }

    public static Fix64GD one()
    {
        Fix64GD ret = new();
        ret.value = Fix64.One;
        return ret;
    }

    public static Fix64GD minus_one()
    {
        Fix64GD ret = new();
        ret.value = Fix64.NegativeOne;
        return ret;
    }

    public static Fix64GD two()
    {
        Fix64GD ret = new();
        ret.value = Fix64.Two;
        return ret;
    }

    public static Fix64GD epsilon()
    {
        Fix64GD ret = new();
        ret.value = Fix64.Epsilon;
        return ret;
    }

    public static Fix64GD pi()
    {
        Fix64GD ret = new();
        ret.value = Fix64.Pi;
        return ret;
    }

    public static Fix64GD min_value()
    {
        Fix64GD ret = new();
        ret.value = Fix64.MinValue;
        return ret;
    }

    public static Fix64GD max_value()
    {
        Fix64GD ret = new();
        ret.value = Fix64.MaxValue;
        return ret;
    }

    // Creation
    public void set_raw_value(long raw_value)
    {
        value = Fix64.FromRaw(raw_value);
    }

    public long get_raw_value()
    {
        return value.RawValue;
    }

    public void from_int(long new_value)
    {
        value = (Fix64)new_value;
    }

    public void from_float(double new_value)
    {
        value = (Fix64)new_value;
    }

    public double to_float()
    {
        return (double)value;
    }

    // Comparisons
    public bool equal(Fix64GD compare)
    {
        return value == compare.value;
    }

    public bool greater(Fix64GD compare)
    {
        return value > compare.value;
    }

    public bool less(Fix64GD compare)
    {
        return value < compare.value;
    }

    public bool greater_equal(Fix64GD compare)
    {
        return value >= compare.value;
    }

    public bool less_equal(Fix64GD compare)
    {
        return value <= compare.value;
    }

    // Operators
    public void add(Fix64GD other)
    {
        value += other.value;
    }

    public void sub(Fix64GD other)
    {
        value -= other.value;
    }

    public void mul(Fix64GD other)
    {
        value *= other.value;
    }

    public void div(Fix64GD other)
    {
        value /= other.value;
    }

    public void mod(Fix64GD other)
    {
        value = value % other.value;
    }

    public void add_int(int other)
    {
        value += (Fix64)other;
    }

    public void sub_int(int other)
    {
        value -= (Fix64)other;
    }

    public void mul_int(int other)
    {
        value *= (Fix64)other;
    }

    public void div_int(int other)
    {
        value /= (Fix64)other;
    }

    public void abs()
    {
        value = Fix64.Abs(value);
    }

    public int sign()
    {
        return Fix64.Sign(value);
    }

    public void floor()
    {
        value = Fix64.Floor(value);
    }

    public void ceil()
    {
        value = Fix64.Ceiling(value);
    }

    public void round()
    {
        value = Fix64.Round(value);
    }

    // Math Functions
    public void sqrt()
    {
        value = Fix64.Sqrt(value);
    }

    public void deg2rad()
    {
        value *= Fix64.DegToRad;
    }

    public void rad2deg()
    {
        value *= Fix64.RadToDeg;
    }

    public static Fix64GD min(Fix64GD a, Fix64GD b)
    {
        Fix64GD ret = new();
        ret.value = Fix64.Min(a.value, b.value);
        return ret;
    }

    public static Fix64GD max(Fix64GD a, Fix64GD b)
    {
        Fix64GD ret = new();
        ret.value = Fix64.Max(a.value, b.value);
        return ret;
    }

    public void clamp(Fix64GD a, Fix64GD b)
    {
        value = Fix64.Clamp(value, a.value, b.value);
    }

    public void clamp01()
    {
        value = Fix64.Clamp01(value);
    }

    public void sin()
    {
        value = Fix64.Sin(value);
    }

    public void cos()
    {
        value = Fix64.Cos(value);
    }

    public void tan()
    {
        value = Fix64.Tan(value);
    }

    public void acos()
    {
        value = Fix64.Acos(value);
    }

    public void atan()
    {
        value = Fix64.Atan(value);
    }

    public static Fix64GD atan2(Fix64GD y, Fix64GD x)
    {
        Fix64GD ret = new();
        ret.value = Fix64.Atan2(y.value, x.value);
        return ret;
    }

    public void pow(Fix64GD exp)
    {
        value = Fix64.Pow(value, exp.value);
    }

    public void pow2()
    {
        value = Fix64.Pow2(value);
    }

    public void ln()
    {
        value = Fix64.Ln(value);
    }

    public void log2()
    {
        value = Fix64.Log2(value);
    }
}
