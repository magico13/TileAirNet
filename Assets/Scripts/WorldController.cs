using UnityEngine;

public class WorldController : MonoBehaviour
{
    public int tiles_width = 64;
    public int tiles_height = 48;
    public float diff = 250;
    public float dt = 0.0167f;

    float[] current;
    // Start is called before the first frame update
    void Start()
    {
        current = new float[tiles_width * tiles_height];
    }

    private void Update()
    {
        bool leftClick = Input.GetMouseButton(0);
        if (leftClick)
        {
            current[index_of(tiles_width / 2, tiles_height / 2)] += 42f;
        }
    }

    // FixedUpdate is called once per physics tick
    void FixedUpdate()
    {
        float[] next = new float[tiles_width * tiles_height];
        diffuse(next, current, diff, dt);
        current = next;
        //for (int x = 0; x < tiles_width; x++)
        //{
        //    for (int y = 0; y < tiles_height; y++)
        //    {
        //        float m = current[index_of(x, y)];
        //        if (m > 0)
        //        {
        //            Debug.LogWarning($"({x}, {y}): {m}");
        //        }
        //    }
        //}
    }

    int index_of(int x, int y)
    {
        return x + y * tiles_width;
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
                    int neighbors = 0;
                    float calc = 0;
                    if (x-1>0)
                    {
                        neighbors++;
                        calc += new_state[index_of(x - 1, y)];
                    }
                    if (x + 1 < tiles_width)
                    {
                        neighbors++;
                        calc += new_state[index_of(x + 1, y)];
                    }
                    if (y - 1 > 0)
                    {
                        neighbors++;
                        calc += new_state[index_of(x, y - 1)];
                    }
                    if (y + 1 < tiles_height)
                    {
                        neighbors++;
                        calc += new_state[index_of(x, y + 1)];
                    }
                    if (neighbors == 0)
                    {
                        new_state[index_of(x, y)] = initial_state[index_of(x, y)];
                        continue;
                    }
                    calc *= a;
                    calc = (initial_state[index_of(x, y)] + calc) / (1 + neighbors * a);
                    if (calc < 0.1)
                    {
                        calc = 0;
                    }
                    new_state[index_of(x, y)] = calc;

                    //new_state[index_of(x, y)] = (initial_state[index_of(x, y)] + a * (new_state[index_of(x - 1, y)] + new_state[index_of(x + 1, y)] + new_state[index_of(x, y - 1)] + new_state[index_of(x, y + 1)])) / (1 + 4 * a);
                }
            }
        }
        //long t_calc = t_start.ElapsedMilliseconds;
        //Debug.Log($"Calc: {t_calc} ms");
    }
}
