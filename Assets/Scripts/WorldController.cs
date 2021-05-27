using TMPro;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static int DisplayMode { get; set; }

    public static int tiles_width = 105;
    public static int tiles_height = 53;
    public float diff = 250;
    public GameObject TileSource;
    public TMP_Text InfoText;
    

    int mode = 0;
    int tileid = 1;
    TileController[] tiles;
    float[] current;
    float cumulativeTime = 0;
    int frameCounter = 0;
    // Start is called before the first frame update
    void Start()
    {
        current = new float[tiles_width * tiles_height];
        tiles = new TileController[tiles_width * tiles_height];
        for (int i=0; i<tiles_width*tiles_height; i++)
        {
            var obj = Instantiate(TileSource);
            Vector2 pos = indexToXY(i);
            obj.transform.position = pos;
            //obj.SetActive(false);
            tiles[i] = obj.GetComponent<TileController>();
        }
    }

    private void Update()
    {
        //System.Diagnostics.Stopwatch t_start = System.Diagnostics.Stopwatch.StartNew();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = 1;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            DisplayMode = (DisplayMode + 1) % 2;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            tileid = (tileid+1) % 3;
            if (tileid == 0)
                tileid = 1;
        }

        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);
        if (leftClick || rightClick)
        {
            var localPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, tiles_width - 1));
            int y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, tiles_height - 1));
            int index = indexOf(x, y);
            if (mode == 0) //add/remove walls
            {
                if (tileid == 1)
                { //walls
                    tiles[index].SetSolid(leftClick);
                }
                else if (tileid == 2)
                { //pumps
                    if (Input.GetMouseButtonDown(0))
                    {
                        //DestroyImmediate(tiles[index].gameObject, true);
                        
                        PumpController pumpController;
                        if (tiles[index].TryGetComponent(out pumpController))
                        {
                            pumpController.PumpActive = true;
                        }
                        else
                        {
                            GameObject pump = Instantiate(Resources.Load<GameObject>("pump"));
                            pump.transform.position = new Vector2(x, y);
                            
                            pumpController = pump.GetComponent<PumpController>();
                        }

                        pumpController.gameObject.SetActive(true);
                        tiles[index] = pumpController;
                        tiles[index].SetSolid(true);
                        tiles[index].Id = 2;
                        tiles[index].GetComponent<SpriteRenderer>().color = Color.red;
                    }
                    else if (rightClick)
                    {
                        if (tiles[index].TryGetComponent(out PumpController controller))
                        {
                            controller.PumpActive = false;
                        }
                        //tiles[index].SetSolid(leftClick);
                    }
                }
            }
            else if (mode == 1) //add air
            {
                float val = 42f;
                if (rightClick)
                {
                    val *= -1;
                }
                current[index] += val;
            }
            if (current[index] < 0)
            {
                current[index] = 0;
            }
        }
        
        cumulativeTime += Time.deltaTime;
        if (++frameCounter > 10)
        {
            UpdateText(10/cumulativeTime);
            frameCounter = 0;
            cumulativeTime = 0;
        }
        //long t_calc = t_start.ElapsedMilliseconds;
        //Debug.Log($"Update: {t_calc} ms");
    }

    void UpdateText(float fps)
    {
        string modeTxt = mode == 0 ? "Tile" : "Air";

        var localPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.RoundToInt(Mathf.Clamp(localPos.x, 0, tiles_width - 1));
        int y = Mathf.RoundToInt(Mathf.Clamp(localPos.y, 0, tiles_height - 1));
        int index = indexOf(x, y);
        float m = Mathf.Round(current[index] * 1000) / 1000;
        float p = Mathf.Round(tiles[index].GetPressure()*100)/100;

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
        for (int x = 0; x < tiles_width; x++)
        {
            for (int y = 0; y < tiles_height; y++)
            {
                int index = indexOf(x, y);
                float m = current[index];
                if (tiles[index] != null && !tiles[index].IsSolid)
                {
                    tiles[index].MoleCount = m;
                    if (m == 0)
                    {
                        tiles[index].gameObject.SetActive(false);
                    }
                    else if (!tiles[index].gameObject.activeSelf)
                    {
                        tiles[index].gameObject.SetActive(true);
                    }
                }

                //if (m > 0)
                //{
                //    Debug.LogWarning($"({x}, {y}): {m}");
                //}
            }
        }
    }

    public static int indexOf(int x, int y)
    {
        return x + y * tiles_width;
    }

    Vector2 indexToXY(int index)
    {
        int x = index % tiles_width;
        int y = index / tiles_width;
        return new Vector2(x, y);
    }

    void diffuse(float[] new_state, float[] initial_state, float diff, float dt)
    {
        //System.Diagnostics.Stopwatch t_start = System.Diagnostics.Stopwatch.StartNew();
        float a = dt * diff;
        for (int k=0; k<20; k++)
        {
            for (int x=0; x<tiles_width; x++)
            {
                for (int y=0; y<tiles_height; y++)
                {
                    int i = indexOf(x, y);
                    int neighbors = 4;
                    float calc = 0;
                    TileController tile = tiles[i];
                    if (tile.IsSolid)
                    {
                        new_state[i] = 0;
                        continue;
                    }
                    if (x - 1 >= 0)
                    {
                        if (!tiles[indexOf(x - 1, y)].IsSolid)
                        {
                            calc += new_state[indexOf(x - 1, y)];
                        }
                        else
                        {
                            neighbors--;
                        }
                    }
                    if (x + 1 < tiles_width)
                    {
                        if (!tiles[indexOf(x + 1, y)].IsSolid)
                        {
                            calc += new_state[indexOf(x + 1, y)];
                        }
                        else
                        {
                            neighbors--;
                        }
                    }
                    if (y - 1 >= 0)
                    {
                        if (!tiles[indexOf(x, y - 1)].IsSolid)
                        {
                            calc += new_state[indexOf(x, y - 1)];
                        }
                        else
                        {
                            neighbors--;
                        }
                    }
                    if (y + 1 < tiles_height)
                    {
                        if (!tiles[indexOf(x, y + 1)].IsSolid)
                        {
                            calc += new_state[indexOf(x, y + 1)];
                        }
                        else
                        {
                            neighbors--;
                        }
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

                    //new_state[index_of(x, y)] = (initial_state[index_of(x, y)] + a * (new_state[index_of(x - 1, y)] + new_state[index_of(x + 1, y)] + new_state[index_of(x, y - 1)] + new_state[index_of(x, y + 1)])) / (1 + 4 * a);
                }
            }
        }
        //long t_calc = t_start.ElapsedMilliseconds;
        //Debug.Log($"Calc: {t_calc} ms");
    }
}
