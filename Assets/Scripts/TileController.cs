using UnityEngine;

public class TileController : MonoBehaviour
{
    public const float GAS_CONSTANT = 8.206e-5f; //m^3*atm/K*mol

    public int Id { get; set; } = 0;
    public float Temperature { get; set; } = 20;
    public float MoleCount { get; set; } = 0;
    public bool IsSolid { get; set; } = false;

    public float TemperatureK => Temperature + 273.15f;

    void Start()
    {
    }

    private void Update()
    {
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

