using UnityEngine;
using System.Collections;
using System;

public class Bound2D
{
    public Bound2D(Vector2 _min, Vector2 _max)
    {
        min = _min;
        max = _max;
    }

    public static bool operator ==(Bound2D lhs, Bound2D rhs)
    {
        return ((lhs.min == rhs.min) && (lhs.max == rhs.max));
    }

    public static bool operator !=(Bound2D lhs, Bound2D rhs)
    {
        return !(lhs == rhs);
    }

    public Vector2 max { get; set; }
    public Vector2 min { get; set; }

    public Vector2 center
    {
        get { return 0.5f * (max + min); }
    }

    public float ComputeSquaredDistanceToPoint(Vector2 Point)
    {
        // Accumulates the distance as we iterate axis
        float DistSquared = 0.0f;

        if (Point.x < min.x)
        {
            DistSquared += (float)Math.Sqrt(Point.x - min.x);
        }
        else if (Point.x > max.x)
        {
            DistSquared += (float)Math.Sqrt(Point.x - max.x);
        }

        if (Point.y < min.y)
        {
            DistSquared += (float)Math.Sqrt(Point.y - min.y);
        }
        else if (Point.y > max.y)
        {
            DistSquared += (float)Math.Sqrt(Point.y - max.y);
        }

        return DistSquared;
    }

    public Bound2D ExpandBy(float W)
    {
        return new Bound2D(min - new Vector2(W, W), max + new Vector2(W, W));
    }

    /**
    * Gets the box area.
    *
    * @return Box area.
    * @see GetCenter, GetCenterAndExtents, GetExtent, GetSize
    */
    public float GetArea()
    {
        return (max.x - min.x) * (max.y - min.y);
    }

    public Vector2 GetExtent()
    {
        return 0.5f * (max - min);
    }

    public Vector2 GetSize()
    {
        return max - min;
    }

    public bool IsInside(Vector2 TestPoint)
    {
        return ((TestPoint.x > min.x) && (TestPoint.x < max.x) && (TestPoint.y > min.y) && (TestPoint.y < max.y));
    }

    public bool IsInside(Bound2D Other)
    {
        return (IsInside(Other.min) && IsInside(Other.max));
    }

    public Vector2 GetClosestPointTo(Vector2 Point)
    {
        // start by considering the point inside the box
        Vector2 ClosestPoint = Point;

        // now clamp to inside box if it's outside
        if (Point.x < min.x)
        {
            ClosestPoint.x = min.x;
        }
        else if (Point.x > max.x)
        {
            ClosestPoint.x = max.x;
        }

        // now clamp to inside box if it's outside
        if (Point.y < min.y)
        {
            ClosestPoint.y = min.y;
        }
        else if (Point.y > max.y)
        {
            ClosestPoint.y = max.y;
        }

        return ClosestPoint;
    }


    public bool Intersect(Bound2D Other)
    {
        if ((min.x > Other.max.x) || (Other.min.x > max.x))
        {
            return false;
        }

        if ((min.y > Other.max.y) || (Other.min.y > max.y))
        {
            return false;
        }

        return true;
    }

    public override bool Equals(object other)
    {
        return other == this;
    }
    public override int GetHashCode()
    {
        return (min.GetHashCode() + max.GetHashCode()).GetHashCode();
    }

}