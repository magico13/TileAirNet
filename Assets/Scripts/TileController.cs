using UnityEngine;

public class TileController : MonoBehaviour
{
    public const float GAS_CONSTANT = 8.206e-5f; //m^3*atm/K*mol

    public int Id { get; set; } = 0;
    public float Temperature { get; set; } = 20;
    public float MoleCount { get; set; } = 0;
    public bool IsSolid { get; private set; } = false;


    public float TemperatureK => Temperature + 273.15f;


    private SpriteRenderer _renderer;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!IsSolid)
        {
            _renderer.enabled = (WorldController.DisplayMode == 1);
        }
    }

    private void Update()
    {
        if (IsSolid) return;
        _renderer.enabled = (WorldController.DisplayMode == 1);
        if (WorldController.DisplayMode == 1)
        {
            _renderer.enabled = true;
            float fill = Mathf.Min(1, Mathf.Sqrt(MoleCount / 42));
            float fill_g = Mathf.Max(Mathf.Min(fill, 0.2f * (5 - MoleCount / 42)), 0);
            _renderer.color = new Color(0, fill_g, fill);
        }
    }

    public virtual void PostDiffusion(float[] next, int x, int y)
    {

    }

    public virtual void SetSolid(bool solid)
    {
        IsSolid = solid;
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        if (solid)
        {
            Id = 1;
            _renderer.color = new Color(0, 1, 0);
            _renderer.enabled = true;
            gameObject.SetActive(true);
        }
        else
        {
            //gameObject.SetActive(false);
            if (WorldController.DisplayMode != 1) //enable this code if air shouldn't render
            {
                _renderer.enabled = false;
            }
        }
    }

    /// <summary>
    /// Returns the pressure in atm for the given temperature/moles
    /// </summary>
    /// <returns>Pressure in atm</returns>
    public float GetPressure()
    {
        if (IsSolid) return 0;
        return MoleCount * TemperatureK * GAS_CONSTANT;
    }

    /// <summary>
    /// Update the MoleCount to fix the pressure
    /// </summary>
    /// <param name="pressure">Desired pressure in atm</param>
    public void SetPressure(float pressure)
    {
        if (IsSolid) return;
        if (pressure <= 0)
        {
            MoleCount = 0;
        }
        else
        {
            MoleCount = pressure / (GAS_CONSTANT * TemperatureK);
        }
    }

}

