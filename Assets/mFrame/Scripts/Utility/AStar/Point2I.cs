using LitJson;
using System;
using Utility;

[Serializable]
public struct Point2I
{
    private const int MAX_VALUE = 65536;

    //Point2I的x,y的绝对值不能大于MAX_ABSOLUTE, 否则SingleValue计算不准确
    private const int MAX_ABSOLUTE = 10000;

    public int x;
    public int y;

    public Point2I(int posX, int posY)
    {
        x = posX;
        y = posY;
    }

    public static bool operator ==(Point2I left, Point2I right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return true;
        }
        return false;
    }

    public static bool operator !=(Point2I left, Point2I right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return false;
        }
        return true;
    }

    public static Point2I operator +(Point2I left, Point2I right)
    {
        return new Point2I(left.x + right.x, left.y + right.y);
    }

    public static Point2I operator -(Point2I left, Point2I right)
    {
        return new Point2I(left.x - right.x, left.y - right.y);
    }

    public override bool Equals(object obj)
    {
        if (obj is Point2I)
        {
            Point2I right = (Point2I)obj;
            if (x == right.x && y == right.y)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public static float Distance(Point2I left, Point2I right)
    {
        int dist = (left.x - right.x) * (left.x - right.x) + (left.y - right.y) * (left.y - right.y);
        return (float)Math.Sqrt(dist);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return x + "_" + y;
    }

    public int guid
    {
        get
        {
            return (x + MAX_ABSOLUTE) * MAX_VALUE + y + MAX_ABSOLUTE;
        }

        set
        {
            int singleValue = value;
            y = singleValue % MAX_VALUE - MAX_ABSOLUTE;
            x = (singleValue - y) / MAX_VALUE - MAX_ABSOLUTE;
        }
    }

    public static int CalcSingleValue(int x, int y)
    {
        return (x + MAX_ABSOLUTE) * MAX_VALUE + y + MAX_ABSOLUTE;
    }

    public static Point2I GuidToPoint(int value)
    {
        Point2I ret = new Point2I();
        ret.guid = value;
        return ret;
    }

    public static Point2I zero
    {
        get { return new Point2I(0, 0); }
    }

    public static Point2I one
    {
        get { return new Point2I(1, 1); }
    }

    public static Point2I minusOne
    {
        get { return new Point2I(-1, -1); }
    }

    public static Point2I invalid
    {
        get { return new Point2I(int.MaxValue, int.MaxValue); }
    }

    public static Point2I ParseFromString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return invalid;
        }

        string[] arr = str.Split("~".ToCharArray());
        if (UtilTools.IsArrayValid(arr))
        {
            return new Point2I(int.Parse(arr[0]), int.Parse(arr[1]));
        }
        return invalid;
    }

    public static JsonData ToJson(Point2I point)
    {
        JsonData ret = new JsonData();
        ret["x"] = point.x;
        ret["y"] = point.y;
        return ret;
    }

    public static Point2I FromJson(JsonData json)
    {
        Point2I newPoint = new Point2I();
        newPoint.x = (int)json["x"];
        newPoint.y = (int)json["y"];
        return newPoint;
    }
}
