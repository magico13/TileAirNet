using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static int DisplayMode { get; set; }

    public static int tiles_width = 105;
    public static int tiles_height = 53;
    public float diff = 250;
    public GameObject[] TileSources;
    public TMP_Text InfoText;

    public static float[] current;
    public static TileController[] tiles;

    int mode = 0;
    int tileid = 1;
    float cumulativeTime = 0;
    int frameCounter = 0;
    readonly HashSet<int> neighborlist = new HashSet<int>();
    // Start is called before the first frame update
    void Start()
    {
        current = new float[tiles_width * tiles_height];
        tiles = new TileController[tiles_width * tiles_height];
        for (int i = 0; i < tiles_width * tiles_height; i++)
        {
            var obj = Instantiate(TileSources[0]);
            Vector2 pos = indexToXY(i);
            obj.transform.position = pos;
            //obj.SetActive(false);
            tiles[i] = obj.GetComponent<TileController>();

            neighborlist.Add(i); //first run we check everything
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            tileid = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            tileid = 2;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DisplayMode = (DisplayMode + 1) % 2;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            mode = (mode + 1) % 2;
        }

        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);
        if (leftClick || rightClick)
        {
            var localPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, tiles_width - 1));
            int y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, tiles_height - 1));
            int index = indexOf(x, y);
            TileController tile = tiles[index];
            if (mode == 0) //tile mode
            {
                if (leftClick && tile.Id != tileid)
                { //different block, remove old one and replace with new one
                    tiles[index].gameObject.SetActive(false); //disable old one

                    var obj = Instantiate(TileSources[tileid]);//create a new one in its place
                    obj.transform.position = new Vector2(x, y);
                    tiles[index] = obj.GetComponent<TileController>();
                    tile = tiles[index];
                }

                if (leftClick)
                {
                    tile.OnLeftClick(x, y);
                }
                if (rightClick)
                {
                    tile.OnRightClick(x, y);
                }
            }
            else if (mode == 1) //air mode
            {
                float val = 42f;
                if (rightClick)
                {
                    val *= -1;
                }
                current[index] += val;
                addSelfAndNeighborsToList(x, y);

            }
            if (current[index] < 0)
            {
                current[index] = 0;
            }
        }

        cumulativeTime += Time.deltaTime;
        if (++frameCounter > 10)
        {
            UpdateText(10 / cumulativeTime);
            frameCounter = 0;
            cumulativeTime = 0;
        }
    }

    void UpdateText(float fps)
    {
        string modeTxt = mode == 0 ? "Tile" : "Air";

        var localPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, tiles_width - 1));
        int y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, tiles_height - 1));
        int index = indexOf(x, y);
        float m = Mathf.Round(current[index] * 1000) / 1000;
        float p = Mathf.Round(tiles[index].GetPressure() * 100) / 100;

        InfoText.SetText($"{modeTxt} Mode\nFPS: {Mathf.FloorToInt(fps)}\nP: {p} atm\nM: {m}\n({x}, {y})\nTile: {tileid}");
    }

    // FixedUpdate is called once per physics tick
    void FixedUpdate()
    {
        float[] next = new float[tiles_width * tiles_height];
        // start with next being the current state, we only change a bit each iteration so this is the closest guess (ie faster convergence)
        for (int x = 0; x < tiles_width; x++)
        {
            for (int y = 0; y < tiles_height; y++)
            {
                int i = indexOf(x, y);
                next[i] = current[i];
            }
        }
        diffuse(next, current, diff, Time.fixedDeltaTime);
        for (int x = 0; x < tiles_width; x++)
        {
            for (int y = 0; y < tiles_height; y++)
            {
                tiles[indexOf(x, y)].PostDiffusion(next, x, y);
            }
        }
        current = next;
        neighborlist.Clear();
        for (int x = 0; x < tiles_width; x++)
        {
            for (int y = 0; y < tiles_height; y++)
            {
                int index = indexOf(x, y);
                float m = current[index];
                tiles[index].UpdateMoleCount(m);
                if (m > 0)
                {
                    addSelfAndNeighborsToList(x, y);
                }
            }
        }
        
    }

    bool checkEdge(int x, int y)
    {
        if (x <= 0 || y <= 0 || x >= tiles_width || y >= tiles_height)
        {
            return true;
        }
        return false;
    }

    bool checkNeighbor(int x, int y)
    {
        if (checkEdge(x, y))
        {
            return true;
        }
        int j = indexOf(x, y);
        if (!tiles[j].IsSolid)
        {
            return true;
        }
        return false;
    }

    void addSelfAndNeighborsToList(int x, int y)
    {
        neighborlist.Add(indexOf(x, y));
        if (checkNeighbor(x - 1, y) && !checkEdge(x - 1, y))
        {
            neighborlist.Add(indexOf(x - 1, y));
        }
        if (checkNeighbor(x + 1, y) && !checkEdge(x + 1, y))
        {
            neighborlist.Add(indexOf(x + 1, y));
        }
        if (checkNeighbor(x, y - 1) && !checkEdge(x, y - 1))
        {
            neighborlist.Add(indexOf(x, y - 1));
        }
        if (checkNeighbor(x, y + 1) && !checkEdge(x, y + 1))
        {
            neighborlist.Add(indexOf(x, y + 1));
        }
    }

    public static int indexOf(int x, int y)
    {
        return x + y * tiles_width;
    }

    Vector2Int indexToXY(int index)
    {
        int x = index % tiles_width;
        int y = index / tiles_width;
        return new Vector2Int(x, y);
    }

    void diffuse(float[] new_state, float[] initial_state, float diff, float dt)
    {
        System.Diagnostics.Stopwatch t_start = System.Diagnostics.Stopwatch.StartNew();
        float a = dt * diff;
        
        //Debug.Log($"Count: {neighborlist.Count}");
        for (int k = 0; k < 20; k++)
        {
            foreach (int i in neighborlist)
            {
                Vector2Int pos = indexToXY(i);
                int x = pos.x;
                int y = pos.y;
                int neighbors = 0;
                float calc = 0;
                TileController tile = tiles[i];
                if (tile.IsSolid)
                {
                    new_state[i] = 0;
                    continue;
                }

                if (checkNeighbor(x - 1, y))
                {
                    if (!checkEdge(x-1, y))
                        calc += new_state[indexOf(x - 1, y)];
                    neighbors++;
                }
                if (checkNeighbor(x + 1, y))
                {
                    if (!checkEdge(x+1, y))
                        calc += new_state[indexOf(x + 1, y)];
                    neighbors++;
                }
                if (checkNeighbor(x, y - 1))
                {
                    if (!checkEdge(x, y-1))
                        calc += new_state[indexOf(x, y - 1)];
                    neighbors++;
                }
                if (checkNeighbor(x, y + 1))
                {
                    if (!checkEdge(x, y+1))
                        calc += new_state[indexOf(x, y + 1)];
                    neighbors++;
                }

                if (neighbors == 0)
                {
                    new_state[i] = initial_state[i];
                    continue;
                }

                calc *= a;
                calc = (initial_state[i] + calc) / (1 + neighbors * a);
                if (Mathf.Abs(calc) < 0.001)
                {
                    calc = 0;
                }
                new_state[i] = calc;
            }
        }
        long t_calc = t_start.ElapsedMilliseconds;
        Debug.Log($"Calc: {t_calc} ms");
    }
}
