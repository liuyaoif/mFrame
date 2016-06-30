using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Threading;

public class AStarPathFinder
{
    public delegate void OnPathFound(List<Point2I> path);

    private Point2I m_size;
    private Dictionary<int, ASNode> m_allNodesDict;
    private Dictionary<int, ASNode> m_closeList;
    private FastPriorityQueue<ASNode> m_openList;
    private List<Point2I> m_neiborTempList = null;

    //多线程处理
    private const string CALCULATE_THREAD_NAME = "AStarThread";
    private Queue<PathFindingTask> m_newTaskQueue = new Queue<PathFindingTask>();
    private List<PathFindingTask> m_doneTaskList = new List<PathFindingTask>();
    private Thread m_calculateThread;
    private bool m_isRunning = true;

    public AStarPathFinder(Point2I size, List<Point2I> cells)
    {
        m_size = size;
        m_allNodesDict = new Dictionary<int, ASNode>(cells.Count);
        foreach (var kvp in cells)
        {
            ASNode node = new ASNode(kvp);
            m_allNodesDict.Add(node.pos.guid, node);
        }

        m_openList = new FastPriorityQueue<ASNode>(cells.Count);
        m_closeList = new Dictionary<int, ASNode>(cells.Count);
        m_neiborTempList = new List<Point2I>(8);

        m_calculateThread = new Thread(CalculatePathThread);
        m_calculateThread.Name = CALCULATE_THREAD_NAME;
        m_calculateThread.Start();
    }

    public void Dispose()
    {
        m_isRunning = false;
        if (m_calculateThread != null)
        {
            m_calculateThread.Abort();
            m_calculateThread.Join();
        }
    }

    private void CalculatePathThread()
    {
        while (m_isRunning)
        {
            lock (m_newTaskQueue)
            {
                lock (m_doneTaskList)
                {
                    while (m_newTaskQueue.Count > 0)
                    {
                        PathFindingTask task = m_newTaskQueue.Dequeue();
                        task.m_path = FindPath(task.m_fromPos, task.m_toPos);
                        m_doneTaskList.Add(task);
                    }
                }
            }
        }
    }

    public void Update()
    {
        if (m_doneTaskList.Count != 0)
        {
            lock (m_doneTaskList)
            {
                for (int i = 0; i < m_doneTaskList.Count; i++)
                {
                    m_doneTaskList[i].m_callBack(m_doneTaskList[i].m_path);
                }
                m_doneTaskList.Clear();
            }
        }
    }

    public void AddPathFindingTask(Point2I from, Point2I to, OnPathFound callBack)
    {
        lock (m_newTaskQueue)
        {
            m_newTaskQueue.Enqueue(new PathFindingTask(from, to, callBack));
        }
    }

    private List<Point2I> FindPath(Point2I start, Point2I end)
    {
        ASNode finalNode;

        ASNode startNode = m_allNodesDict[start.guid];
        startNode.G = CalcG(startNode, startNode);
        m_openList.Enqueue(startNode, startNode.F);
        while (m_openList.Count != 0)
        {
            //找出F值最小的点
            ASNode tempStart = m_openList.Dequeue();
            m_closeList.Add(tempStart.pos.guid, tempStart);

            //找出它相邻的点
            GetNeiborPoints(tempStart);
            for (int i = 0; i < m_neiborTempList.Count; i++)
            {
                Point2I point = m_neiborTempList[i];

                if (m_openList.Contains(m_allNodesDict[point.guid]))
                {
                    ASNode node = m_allNodesDict[point.guid];
                    //计算G值, 如果比原来的大, 就什么都不做, 否则设置它的父节点为当前点,并更新G和F
                    FoundPoint(tempStart, node);
                }
                else
                {
                    //如果它们不在开始列表里, 就加入, 并设置父节点,并计算GHF
                    NotFoundPoint(tempStart, end, point);
                }
            }
            m_neiborTempList.Clear();
            if (m_openList.Contains(m_allNodesDict[end.guid]))
            {
                finalNode = m_allNodesDict[end.guid];
                break;
            }
        }
        finalNode = m_allNodesDict[end.guid];

        List<Point2I> ret = new List<Point2I>();
        while (finalNode != null)
        {
            ret.Add(finalNode.pos);

            if (m_allNodesDict.ContainsKey(finalNode.parent))
            {
                finalNode = m_allNodesDict[finalNode.parent];
            }
            else
            {
                finalNode = null;
            }
        }

        Clear();
        return ret;
    }

    private void FoundPoint(ASNode tempStart, ASNode point)
    {
        var G = CalcG(tempStart, point);
        if (G < point.G)
        {
            point.parent = tempStart.pos.guid;
            point.G = G;
        }
    }

    private void NotFoundPoint(ASNode tempStart, Point2I end, Point2I point)
    {
        ASNode node = m_allNodesDict[point.guid];
        node.parent = tempStart.pos.guid;
        node.G = CalcG(tempStart, node);
        node.H = CalcH(end, node);
        m_openList.Enqueue(node, node.F);
    }

    private int CalcG(ASNode start, ASNode point)
    {
        //int G = (point.m_cell == null) ? Cell.MAX_WEIGHT : point.weight;
        int G = point.weight;

        int parentG = 0;
        if (point.parent == -1)
        {
            parentG = 0;
        }
        else
        {
            parentG = m_allNodesDict[point.parent].G;
        }

        return G + parentG;
    }

    private int CalcH(Point2I end, ASNode node)
    {
        const int straightWeight = 10;
        const int diagonalWeight = 14;
        int h_diagonal = Math.Min(Math.Abs(node.pos.x - end.x), Math.Abs(node.pos.y - end.y));
        int h_straight = Math.Abs(node.pos.x - end.x) + Math.Abs(node.pos.y - end.y);
        int h = diagonalWeight * h_diagonal + straightWeight * (h_straight - 2 * h_diagonal);
        return h;
    }

    //获取某个点周围可以到达的点
    private void GetNeiborPoints(ASNode point)
    {
        for (int x = point.pos.x - 1; x <= point.pos.x + 1; x++)
        {
            for (int y = point.pos.y - 1; y <= point.pos.y + 1; y++)
            {
                if (IsInRange(x, y) && !m_closeList.ContainsKey(Point2I.CalcSingleValue(x, y)))
                {
                    m_neiborTempList.Add(new Point2I(x, y));
                }
            }
        }
    }

    public bool IsInRange(int x, int y)
    {
        if (x >= 0 && x < m_size.x && y >= 0 && y < m_size.y)
        {
            return true;
        }
        return false;
    }

    private void Clear()
    {
        m_openList.Clear();
        m_closeList.Clear();

        foreach (var kvp in m_allNodesDict)
        {
            kvp.Value.G = -1;
            kvp.Value.H = -1;
            kvp.Value.parent = -1;
        }
    }

    private class ASNode : FastPriorityQueueNode
    {
        public int parent = -1;
        public int F { get { return G + H; } }  //F=G+H
        public int G { get; set; }
        public int H { get; set; }

        //         public Cell m_cell;
        //
        //         public ASNode(Cell cell)
        //         {
        //             m_cell = cell;
        //         }

        public Point2I m_pos;

        public ASNode(Point2I pos)
        {
            m_pos = pos;
        }
        public Point2I pos
        {
            get { return m_pos; }
        }

        public int weight
        {
            //get { return m_cell.m_weight; }
            get { return 1; }
        }

        public override string ToString()
        {
            return pos.ToString() + "_G: " + G + "_H: " + H + "_F: " + F;
        }
    }

    private class PathFindingTask
    {
        public Point2I m_fromPos = Point2I.minusOne;
        public Point2I m_toPos = Point2I.minusOne;
        public OnPathFound m_callBack = null;
        public List<Point2I> m_path = null;

        public PathFindingTask(Point2I from, Point2I to, OnPathFound callBack)
        {
            m_fromPos = from;
            m_toPos = to;
            m_callBack = callBack;
        }
    }
}

