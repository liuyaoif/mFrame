using LitJson;
using System;

namespace mFrame.Utility
{
    [Serializable]
    public struct Vector2I
    {
        private const int MAX_VALUE = 65536;

        //Point2I的x,y的绝对值不能大于MAX_ABSOLUTE, 否则SingleValue计算不准确
        private const int MAX_ABSOLUTE = 10000;

        public int x;
        public int y;

        public Vector2I(int posX, int posY)
        {
            x = posX;
            y = posY;
        }

        public static bool operator ==(Vector2I left, Vector2I right)
        {
            if (left.x == right.x && left.y == right.y)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Vector2I left, Vector2I right)
        {
            if (left.x == right.x && left.y == right.y)
            {
                return false;
            }
            return true;
        }

        public static Vector2I operator +(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.x + right.x, left.y + right.y);
        }

        public static Vector2I operator -(Vector2I left, Vector2I right)
        {
            return new Vector2I(left.x - right.x, left.y - right.y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2I)
            {
                Vector2I right = (Vector2I)obj;
                if (x == right.x && y == right.y)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public static float Distance(Vector2I left, Vector2I right)
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

        public static Vector2I GuidToPoint(int value)
        {
            Vector2I ret = new Vector2I();
            ret.guid = value;
            return ret;
        }

        public static Vector2I zero
        {
            get { return new Vector2I(0, 0); }
        }

        public static Vector2I one
        {
            get { return new Vector2I(1, 1); }
        }

        public static Vector2I minusOne
        {
            get { return new Vector2I(-1, -1); }
        }

        public static Vector2I invalid
        {
            get { return new Vector2I(int.MaxValue, int.MaxValue); }
        }

        public static Vector2I ParseFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return invalid;
            }

            string[] arr = str.Split("~".ToCharArray());
            if (UtilTools.IsArrayValid(arr))
            {
                return new Vector2I(int.Parse(arr[0]), int.Parse(arr[1]));
            }
            return invalid;
        }

        public static JsonData ToJson(Vector2I point)
        {
            JsonData ret = new JsonData();
            ret["x"] = point.x;
            ret["y"] = point.y;
            return ret;
        }

        public static Vector2I FromJson(JsonData json)
        {
            Vector2I newPoint = new Vector2I();
            newPoint.x = (int)json["x"];
            newPoint.y = (int)json["y"];
            return newPoint;
        }
    }
}