using System;

namespace MagneticField
{
    public struct PointD
    {
        public double X;
        public double Y;
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public PointD ToPoint()
        {
            return new PointD(X, Y);
        }
        public static double Distance(PointD p1, PointD p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        public PointD Rotate(double angle)
        {
            return new PointD(X * Math.Cos(angle) - Y * Math.Sin(angle), X * Math.Sin(angle) + Y * Math.Cos(angle));
        }
        public override bool Equals(object obj)
        {
            return obj is PointD d && this == d;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
        public static bool operator ==(PointD a, PointD b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(PointD a, PointD b)
        {
            return !(a == b);
        }
        public static PointD operator +(PointD a, PointD b)
        {
            return new PointD(a.X + b.X, a.Y + b.Y);
        }
        public static PointD operator +(PointD a, double value)
        {
            return new PointD(a.X + value, a.Y + value);
        }
        public static PointD operator -(PointD a, PointD b)
        {
            return new PointD(a.X - b.X, a.Y - b.Y);
        }
        public static PointD operator -(PointD a, double value)
        {
            return new PointD(a.X - value, a.Y - value);
        }
        public static PointD operator *(PointD a, PointD b)
        {
            return new PointD(a.X * b.X, a.Y * b.Y);
        }
        public static PointD operator *(PointD a, double value)
        {
            return new PointD(a.X * value, a.Y * value);
        }
    }
}
