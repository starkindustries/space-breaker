﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private Material whiteFlash = null;
    private Material defaultMaterial;

    [SerializeField]
    private int baseHitPoints = 10;
    private int hitPoints;

    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private SpriteRenderer minimapSpriteRenderer = null;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;
        minimapSpriteRenderer.color = spriteRenderer.color;
        hitPoints = baseHitPoints;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only count collisions from bullets
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("block struct. hit points: " + hitPoints);
            hitPoints--;
            StartCoroutine(HitAnimation());
        }        

        // Destroy block if HP = 0
        if (hitPoints < 1)
        {
            EndlessModeManager.Instance.DestroyedBlock(block: this, position: gameObject.transform.position);
            Destroy(this.gameObject);            
        }       
    }

    // White Flash Effect
    private IEnumerator HitAnimation()
    {
        // Enable white flash material
        spriteRenderer.material = whiteFlash;
        yield return new WaitForSeconds(0.1f);
        // Re-enable default material
        spriteRenderer.material = defaultMaterial;
    }

    public int GetBaseHitPoints()
    {
        return baseHitPoints;
    }
}
