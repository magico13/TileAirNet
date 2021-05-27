using UnityEngine;

public class PumpController : TileController
{
    public float Rate = 0.1f;
    public bool PumpActive = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void PostDiffusion(float[] next, int x, int y)
    {
        if (PumpActive)
        {
            // move particles from one side to the other
            float toMove = Pump(next[WorldController.indexOf(x, y - 1)]);
            next[WorldController.indexOf(x, y - 1)] -= toMove;
            next[WorldController.indexOf(x, y + 1)] += toMove;
        }
    }

    public float Pump(float sourceMoles)
    {
        return Rate * sourceMoles;
    }
}
