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
            // move particles from below to above
            
            int below = WorldController.indexOf(x, y - 1);
            int above = WorldController.indexOf(x, y + 1);
            if (!WorldController.tiles[below].IsSolid && !WorldController.tiles[above].IsSolid)
            {
                float toMove = Pump(next[below]);
                next[below] -= toMove;
                next[above] += toMove;
            }
        }
    }

    public override void OnLeftClick(int x, int y)
    {
        base.OnLeftClick(x, y);
    }

    public override void OnRightClick(int x, int y)
    {
        PumpActive = false;
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
