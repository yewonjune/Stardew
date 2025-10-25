using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarGrid2D : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap roadTilemap;            // 걷기 허용
    public Tilemap[] blockTilemaps;        // 벽, 물, 건물 내부 등 (선택)

    [Header("Settings")]
    public LayerMask obstacleMask;         // Collider로 막는 레이어 (나무, 바위, 건물 등)
    public Vector2 cellSize = new Vector2(1f, 1f);
    public float agentRadius = 0.2f;       // 에이전트 반경(좁은 곳 피하려면 키움)

    BoundsInt _bounds;
    Vector3 _origin;
    int _w, _h;

    bool[,] _walkable; // [x,y]

    public void Bake()
    {
        if (!roadTilemap) { Debug.LogError("[AStarGrid2D] roadTilemap is null"); return; }

        _bounds = roadTilemap.cellBounds;
        _w = _bounds.size.x;
        _h = _bounds.size.y;
        _origin = roadTilemap.CellToWorld(_bounds.min);

        _walkable = new bool[_w, _h];

        for (int y = 0; y < _h; y++)
        {
            for (int x = 0; x < _w; x++)
            {
                var cell = new Vector3Int(_bounds.xMin + x, _bounds.yMin + y, 0);
                bool isRoad = roadTilemap.HasTile(cell);

                if (!isRoad) { _walkable[x, y] = false; continue; }

                // 블록 타일맵에 타일이 있으면 막힘
                bool blockedByTilemap = false;
                if (blockTilemaps != null)
                {
                    foreach (var tm in blockTilemaps)
                    {
                        if (tm && tm.HasTile(cell)) { blockedByTilemap = true; break; }
                    }
                }
                if (blockedByTilemap) { _walkable[x, y] = false; continue; }

                // 콜라이더 충돌 체크(원형 캐스트)
                Vector3 world = roadTilemap.GetCellCenterWorld(cell);
                bool hit = Physics2D.OverlapCircle(world, agentRadius, obstacleMask);
                _walkable[x, y] = !hit;
            }
        }
    }

    public bool WorldToGrid(Vector3 world, out int gx, out int gy)
    {
        var cell = roadTilemap.WorldToCell(world);
        gx = cell.x - _bounds.xMin;
        gy = cell.y - _bounds.yMin;
        return gx >= 0 && gy >= 0 && gx < _w && gy < _h;
    }

    public Vector3 GridToWorld(int gx, int gy)
    {
        var cell = new Vector3Int(_bounds.xMin + gx, _bounds.yMin + gy, 0);
        return roadTilemap.GetCellCenterWorld(cell);
    }

    public bool IsWalkable(int gx, int gy)
    {
        if (gx < 0 || gy < 0 || gx >= _w || gy >= _h) return false;
        return _walkable[gx, gy];
    }

    // 8방향 이웃(대각선 포함). 좁은 통로 끼임 방지 원하면 대각선 금지로 바꿔도 됨.
    static readonly (int dx, int dy, int cost)[] Neigh = new (int, int, int)[]
    {
        ( 1, 0, 10),( -1, 0, 10),( 0, 1, 10),( 0,-1, 10),
        ( 1, 1, 14),( 1,-1, 14),(-1, 1, 14),(-1,-1, 14)
    };

    public bool FindPath(Vector3 startWorld, Vector3 targetWorld, List<Vector3> outPath)
    {
        outPath.Clear();
        if (!WorldToGrid(startWorld, out int sx, out int sy)) return false;
        if (!WorldToGrid(targetWorld, out int tx, out int ty)) return false;
        if (!IsWalkable(tx, ty)) return false;

        var open = new SimpleMinHeap();
        var came = new Dictionary<(int, int), (int, int)>();
        var gScore = new Dictionary<(int, int), int>();

        (int, int) start = (sx, sy);
        (int, int) goal = (tx, ty);

        gScore[start] = 0;
        open.Push(start, Heuristic(sx, sy, tx, ty));

        var closed = new HashSet<(int, int)>();

        while (open.Count > 0)
        {
            var current = open.PopMin();
            if (current == goal)
            {
                // reconstruct
                var cur = current;
                var rev = new List<(int, int)>();
                rev.Add(cur);
                while (came.TryGetValue(cur, out var prev))
                {
                    cur = prev;
                    rev.Add(cur);
                }
                rev.Reverse();

                foreach (var (gx, gy) in rev)
                    outPath.Add(GridToWorld(gx, gy));
                return true;
            }

            closed.Add(current);

            foreach (var (dx, dy, stepCost) in Neigh)
            {
                var nb = (current.Item1 + dx, current.Item2 + dy);
                if (closed.Contains(nb)) continue;
                if (!IsWalkable(nb.Item1, nb.Item2)) continue;

                int tentativeG = gScore[current] + stepCost;
                if (!gScore.TryGetValue(nb, out int oldG) || tentativeG < oldG)
                {
                    gScore[nb] = tentativeG;
                    came[nb] = current;
                    int f = tentativeG + Heuristic(nb.Item1, nb.Item2, tx, ty);
                    open.PushOrDecreaseKey(nb, f);
                }
            }
        }

        return false;
    }

    int Heuristic(int x1, int y1, int x2, int y2)
    {
        int dx = Mathf.Abs(x1 - x2);
        int dy = Mathf.Abs(y1 - y2);
        return 10 * (dx + dy) + (4 * Mathf.Min(dx, dy)); // 대각선 허용 근사
    }

    // 아주 단순한 최소 힙
    class SimpleMinHeap
    {
        readonly List<((int, int) node, int f)> _arr = new();
        readonly Dictionary<(int, int), int> _index = new();

        public int Count => _arr.Count;

        public void Push((int, int) node, int f)
        {
            _arr.Add((node, f));
            _index[node] = _arr.Count - 1;
            Up(_arr.Count - 1);
        }

        public void PushOrDecreaseKey((int, int) node, int f)
        {
            if (_index.TryGetValue(node, out int i))
            {
                if (f < _arr[i].f)
                {
                    _arr[i] = (node, f);
                    Up(i);
                }
            }
            else Push(node, f);
        }

        public (int, int) PopMin()
        {
            var res = _arr[0].node;
            Swap(0, _arr.Count - 1);
            _index.Remove(_arr[^1].node);
            _arr.RemoveAt(_arr.Count - 1);
            Down(0);
            return res;
        }

        void Up(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (_arr[p].f <= _arr[i].f) break;
                Swap(i, p);
                i = p;
            }
        }

        void Down(int i)
        {
            while (true)
            {
                int l = i * 2 + 1, r = i * 2 + 2, s = i;
                if (l < _arr.Count && _arr[l].f < _arr[s].f) s = l;
                if (r < _arr.Count && _arr[r].f < _arr[s].f) s = r;
                if (s == i) break;
                Swap(i, s);
                i = s;
            }
        }

        void Swap(int a, int b)
        {
            var tmp = _arr[a];
            _arr[a] = _arr[b];
            _arr[b] = tmp;
            _index[_arr[a].node] = a;
            _index[_arr[b].node] = b;
        }
    }

#if UNITY_EDITOR
    // 필요하면 그리드 시각화
    void OnDrawGizmosSelected()
    {
        if (_walkable == null) return;
        for (int y = 0; y < _h; y++)
        {
            for (int x = 0; x < _w; x++)
            {
                var w = GridToWorld(x, y);
                Gizmos.color = _walkable[x, y] ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.2f);
                Gizmos.DrawCube(w, new Vector3(cellSize.x, cellSize.y, 0.1f) * 0.9f);
            }
        }
    }
#endif
}
