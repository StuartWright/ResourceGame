using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;



public class Tile : MonoBehaviour
{
    public delegate void TileSearch();
    public event TileSearch TilesFound;
    public List<Tile> SurroundingTiles = new List<Tile>();
    public MeshRenderer Renderer;
    public int TileIndex;
    public string BuildingName;
    private GameManager GM;
    private MeshRenderer Mesh;
    private Material FloorMat;
    public GameObject Builder, Stone;
    public GameObject LogsGO, StoneGO, IronGO, canvas;
    public GameObject Boarder;
    public bool IsEmpty = true, CantInteract;
    private bool IsBuilder;
    public PropertyTypes Type;
    private int layerMask = 1 << 8;
    public Builder BuilderRef;
    private BaseProperty Property;
    private BuyPanelManager BuyPanel;
    public List<GameObject> Properties;
    public PropertyID PropertyID;
    private void Awake()
    {
        GM = GameManager.Instance;
    }
    void Start()
    {
        
        Mesh = GetComponent<MeshRenderer>();
        FloorMat = Mesh.material;
        BuyPanel = GameObject.Find("CatagoryPanel").GetComponent<BuyPanelManager>();
        StartCoroutine(LookForSurroundings());
        //if (PlayerPrefs.HasKey("HasSaved"))
           // LoadTile();
    }

    public void LoadTile()
    {
        if(GM.TilePropertyName[TileIndex] == "Builder")
        {
            Builder.SetActive(true);
            //Type = PropertyTypes.Builder;
            IsBuilder = true;
            BuilderRef = Builder.GetComponent<Builder>();
            BuilderRef.LoadBuilder();
            BuilderRef.Init();
        }
        else if (GM.TilePropertyName[TileIndex] == "Stone")
        {
            Stone.SetActive(true);
            Type = PropertyTypes.Stone;
        }
        else
        {
            foreach (PropertyID id in GameManager.Instance.AllIDs)
            {
                if (id.Property.PropertyName.ToString() == GM.TilePropertyName[TileIndex])
                {
                    PropertyID = id;
                    foreach (GameObject property in Properties)
                    {
                        if (property.name == id.Property.PropertyName.ToString())
                        {
                            Property = property.GetComponent<BaseProperty>();
                            Property.Level = PlayerPrefs.GetInt("TilePropertyLevel" + TileIndex.ToString());
                            property.SetActive(true);
                            Property.LoadProperty();                                                      
                            Property.transform.eulerAngles = new Vector3(0, PlayerPrefs.GetFloat("PropertyRotation" + TileIndex.ToString()), 0);                                                      
                            break;
                        }
                    }
                    Type = id.Property.PropertyName;
                    break;
                }
            }
        }       
        IsEmpty = false;
    }
    public void BuyProperty(PropertyID ID, bool HasBuilt)
    {       
        foreach(GameObject _property in Properties)
        {
            if(_property.name == ID.Property.PropertyName.ToString())
            {
                if (HasBuilt)
                {
                    PropertyID = ID;
                    Property = _property.GetComponent<BaseProperty>();
                    Property.Level = PlayerPrefs.GetInt("TilePropertyLevel" + TileIndex.ToString());
                    PlayerPrefs.SetInt("TilePropertyLevel" + TileIndex.ToString(), ++Property.Level);
                    Builder.SetActive(false);
                    _property.SetActive(true);
                    Type = ID.Property.PropertyName;
                    GM.TilePropertyName[TileIndex] = Type.ToString();
                    PlayerPrefsX.SetStringArray("TilePropertyName", GM.TilePropertyName);
                    IsBuilder = false;                    
                    break;
                }
                else if (ID.Property.CheckResources())
                {
                    BuilderRef.LogAmount = ID.Property.LogAmount;
                    GM.Logs -= ID.Property.LogAmount;
                    BuilderRef.StoneAmount = ID.Property.StoneAmount;
                    GM.Stone -= ID.Property.StoneAmount;
                    //Type = PropertyTypes.Builder;
                    Type = ID.Property.PropertyName;
                    GM.TilePropertyName[TileIndex] = "Builder";
                    PlayerPrefsX.SetStringArray("TilePropertyName", GM.TilePropertyName);
                    IsBuilder = true;
                    Builder.SetActive(true);
                    BuilderRef.Index = ID.Property.Index;
                    BuilderRef.tile = this;
                    BuilderRef.Init();
                    IsEmpty = false;
                    SetFloorMat();
                    break;
                }
                else
                {
                    GM.CanBuild = false;                    
                    return;
                }
            }
        }
      
        Boarder.SetActive(false);       
        
    }
    public bool CheckForRoad()
    {
        foreach (Tile tile in SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.Road)
            {
                return true;
            }
                
        }
        return false;
    }
    public bool CheckForKeep()
    {
        foreach (Tile tile in SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.Keep)
            {
                return true;
            }

        }
        return false;
    }
    public bool CheckForStone()
    {
        foreach (Tile tile in SurroundingTiles)
        {
            if (tile.Type == PropertyTypes.Stone)
            {
                return true;
            }

        }
        return false;
    }
    public void SellProperty()
    {
        if (IsBuilder)
        {
            BuilderRef.SellProperty();
            Builder.SetActive(false);
        }
        else
        {
            foreach (GameObject property in Properties)
            {
                if (property.name == PropertyID.Property.PropertyName.ToString())
                {
                    property.SetActive(false);
                    property.GetComponent<ISellable>().SellProperty();
                    break;
                }
            }
        }
        BuilderRef.InitiallyBuilt = false;
        Type = PropertyTypes.Empty;
        if (GM.Build)
        Boarder.SetActive(true);        
        GM.TilePropertyName[TileIndex] = "Empty";
        PlayerPrefsX.SetStringArray("TilePropertyName", GM.TilePropertyName);
        IsEmpty = true;
        SetFloorMat();
    }
    public void TurnOnOff()
    {
        Property.TurnOnOff();
    }
    public void UpgradeProperty()
    {
        if (Property.Level < PropertyID.Property.MaxLevel)
        {
            if (Keep.Instance.Level >= PropertyID.Property.UpgradeAmounts[Property.Level - 1].RequiredKeepLevel && PropertyID.Property.CheckUpgradeResources(Property.Level - 1))
            {
                Property.UpgradeProperty();
                Property.gameObject.SetActive(false);

                BuilderRef.LogAmount = PropertyID.Property.UpgradeAmounts[Property.Level-1].LogAmount;
                GM.Logs -= PropertyID.Property.UpgradeAmounts[Property.Level - 1].LogAmount;
                BuilderRef.StoneAmount = PropertyID.Property.UpgradeAmounts[Property.Level - 1].StoneAmount;
                GM.Stone -= PropertyID.Property.UpgradeAmounts[Property.Level - 1].StoneAmount;
                //Type = ID.Property.PropertyName;
                GM.TilePropertyName[TileIndex] = "Builder";
                PlayerPrefsX.SetStringArray("TilePropertyName", GM.TilePropertyName);
                IsBuilder = true;
                Builder.SetActive(true);
                BuilderRef.Index = PropertyID.Property.Index;
                BuilderRef.tile = this;
                BuilderRef.Init();
                IsEmpty = false;
                SetFloorMat();
            }
            else
                GameManager.Instance.ErrorMessage("Not enough resources");
        }
    }
    private void OnMouseUp()
    {
        if (CameraMovement.Instance.CanMove) return;

        if (IsEmpty)
        {
            if (GM.Build && !GM.UIClicked)
                BuyPanel.Open();
            else
                return;
        }
        //GM.BuyPanel.SetActive(true);
        else if (!GM.Build)
        {
            GM.SetPropertyUI(Property);
            if (Property != null && Property.CanTurnOff)
                GM.TurnOnOffButton.interactable = true;
            else
                GM.TurnOnOffButton.interactable = false;
        }
        else
            return;
        if (CantInteract) return;
        if(GM.SelectedTile != null)
        {
            GM.SwitchTile(this);
        }
        GM.SelectedTile = this;
        Mesh.material = null;
    }
    /*
    public void OnMouseOver()
    {
        if (CameraMovement.Instance.CanMove) return;
        if (GM.SelectedTile != null) return;
        Mesh.material = null; 
    }
    */
    private void OnMouseExit()
    {
        if (GM.SelectedTile != null) return;
        SetFloorMat();
    }
    //public void SetFloorMat() => Mesh.material = FloorMat;
    public void SetFloorMat() => Renderer.enabled = false;

    private void FindSurrounding()
    {
        RaycastHit Hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out Hit, 10, layerMask))
        {
            if (Hit.collider.gameObject != gameObject && Hit.collider.GetComponent<Tile>())
            {
                SurroundingTiles.Add(Hit.collider.GetComponent<Tile>());
            }
        }
        if (Physics.Raycast(transform.position, Vector3.right, out Hit, 10, layerMask))
        {
            if (Hit.collider.gameObject != gameObject && Hit.collider.GetComponent<Tile>())
            {
                SurroundingTiles.Add(Hit.collider.GetComponent<Tile>());
            }
        }
        if (Physics.Raycast(transform.position, Vector3.back, out Hit, 10, layerMask))
        {
            if (Hit.collider.gameObject != gameObject && Hit.collider.GetComponent<Tile>())
            {
                SurroundingTiles.Add(Hit.collider.GetComponent<Tile>());
            }
        }       
        if (Physics.Raycast(transform.position, Vector3.left, out Hit, 10, layerMask))
        {
            if (Hit.collider.gameObject != gameObject && Hit.collider.GetComponent<Tile>())
            {
                SurroundingTiles.Add(Hit.collider.GetComponent<Tile>());
            }
        }
        TilesFound?.Invoke();
    }
    IEnumerator LookForSurroundings()
    {
        yield return new WaitForSeconds(.2f);
        FindSurrounding();
       
    }
    public void ShowBoarder()
    {
        if(Type == PropertyTypes.Empty)
        Boarder.SetActive(true);
    }
    public void HideBoarder()
    {
        Boarder.SetActive(false);
    }
    public void CheckMultiBuild(PropertyID ID)
    {
        for(int i = 0; i < SurroundingTiles.Count; i++)
        {
            if(SurroundingTiles[i].Type == ID.Property.PropertyName)
            {
                if(i == 0)
                {
                    if (2 >= SurroundingTiles.Count)
                        return;
                        GM.BuildMultiple(ID, SurroundingTiles[2]);
                    break;
                }
                else if (i == 1)
                {
                    if (3 >= SurroundingTiles.Count)
                        return;
                    GM.BuildMultiple(ID, SurroundingTiles[3]);
                    break;
                }
                else if (i == 2)
                {
                    if (0 == SurroundingTiles.Count)
                        return;
                    GM.BuildMultiple(ID, SurroundingTiles[0]);
                    break;
                }
                else if (i == 3)
                {
                    if (1 == SurroundingTiles.Count)
                        return;
                    GM.BuildMultiple(ID, SurroundingTiles[1]);
                    break;
                }
            }
        }
    }
}
