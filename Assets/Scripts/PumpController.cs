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


    public override void SetSolid(bool solid)
    {
        base.SetSolid(solid);
        PumpActive = solid;
        if (solid)
        {
            Id = 2;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            Id = 0;
        }
    }
    public float Pump(float sourceMoles)
    {
        return Rate * sourceMoles;
    }
}
