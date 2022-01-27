using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindMill : BaseProperty
{
    private void Start()
    {
        Tile.TilesFound += BuffSurroundingFarms;
    }
    private new void OnEnable()
    {
        if (GameManager.Instance.IsRotating)
        {
            SetMaterial(PreBuilt);
            return;
        }
        else
            SetMaterial(UnFaded);

        BuffSurroundingFarms();
        base.OnEnable();
    }

    private void BuffSurroundingFarms()
    {
        Tile.TilesFound -= BuffSurroundingFarms;
        foreach (Tile tile in Tile.SurroundingTiles)
        {
            if(tile.Type == PropertyTypes.Farm)
            {
                tile.GetComponentInChildren<Farm>().NearWindmill();
            }
        }
    }
    private void DeBuffSurroundingFarms()
    {
        foreach (Tile tile in Tile.SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.Farm)
            {
                tile.GetComponentInChildren<Farm>().RemoveWindmillEffect();
            }
        }
    }
    public override void SellProperty()
    {
        DeBuffSurroundingFarms();
        base.SellProperty();
    }
}
