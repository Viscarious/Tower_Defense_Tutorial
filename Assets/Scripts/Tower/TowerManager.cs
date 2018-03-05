using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerManager : Singleton<TowerManager> {

    public TowerButton towerButtonPressed { get; set; }
    private SpriteRenderer spriteRenderer;

    private List<Tower> towersList = new List<Tower>();
    private List<Collider2D> buildSiteList = new List<Collider2D>();

	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.tag == "BuildSite")
                {
                    hit.collider.enabled = false;
                    RegisterBuildSite(hit.collider);
                    PlaceTower(hit);
                }
            }
        }

        if (spriteRenderer.enabled)
        {
            FollowMouse();
        }
    }

    public void RegisterBuildSite(Collider2D buildTag)
    {
        buildSiteList.Add(buildTag);
    }

    public void RegisterTower(Tower tower)
    {
        towersList.Add(tower);
    }

    public void ReenableBuildSites()
    {
        foreach(Collider2D collider in buildSiteList)
        {
            collider.enabled = true;
        }

        buildSiteList.Clear();
    }

    public void DestroyAllTowers()
    {
        foreach (Tower tower in towersList)
        {
            Destroy(tower.gameObject);
        }

        towersList.Clear();
    }

    private void FollowMouse()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(transform.position.x, transform.position.y);
    }

    private void EnableDragSprite(Sprite sprite)
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite;
    }

    public void DisableDragSprite()
    {
        spriteRenderer.enabled = false;
        spriteRenderer.sprite = null;
    }

    /// <summary>
    /// Places the most recently selected tower on a build site.
    /// </summary>
    /// <param name="hit"></param>
    private void PlaceTower(RaycastHit2D hit)
    {
        if (!EventSystem.current.IsPointerOverGameObject() && towerButtonPressed != null)
        {
            Tower newTower = Instantiate(towerButtonPressed.TowerObject);
            newTower.transform.position = hit.transform.position;
            BuyTower(towerButtonPressed.TowerPrice);

            RegisterTower(newTower);
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Towerbuilt);
            DisableDragSprite();
        }
    }

    public void BuyTower(int price)
    {
        GameManager.Instance.SubtractMoney(price);
    }

    /// <summary>
    /// Event called when a tower button in the Canvas is clicked
    /// </summary>
    /// <param name="towerSelected"></param>
    public void SelectedTower(TowerButton towerSelected)
    {
        if (towerSelected.TowerPrice <= GameManager.Instance.CurrentFunds)
        {
            towerButtonPressed = towerSelected;

            EnableDragSprite(towerButtonPressed.DragSprite);
        }
    }
}
