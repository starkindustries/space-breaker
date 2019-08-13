﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EndlessMode : MonoBehaviour
{
    public GameObject[] blockPrefabs;
    public Tilemap tilemap;
    public int levelWidth;
    public float blockShiftInterval;

    // Origin is the grid coordinate of the top left position of the border
    // which all other blocks can be positioned off of via an offset
    // xoxxxxxxxxxxxxxxxxxxxxxxx  <=== here, o = origin
    // x                       x  <=== the border box
    // x                       x
    // 
    // Origin is set here so that blocks can be generated behind the top border
    // and then slide down into the playable space.
    private Vector3 origin = new Vector3(x: -0.5f, y: 24.5f);
    private float bottomY = -0.5f;
    private float timeSinceLastBlockShift;

    private List<GameObject> blocks;

    private int rowSpawnCount;
    public int rowsToSpawn = 1;

    // Start is called before the first frame update
    private void Start()
    {
        blocks = new List<GameObject>();
        timeSinceLastBlockShift = 0;
        rowSpawnCount = 0;

        // Load player data
        PlayerData data = SaveSystem.LoadPlayerData();
        if (data == null)
        {
            // start new level
            Debug.Log("No save file. Start new level");
        }
        else
        {
            Debug.Log("Save file found. Load player data!");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Remove all null/destroyed blocks
        // https://stackoverflow.com/questions/3069748/how-to-remove-all-the-null-elements-inside-a-generic-list-in-one-go
        blocks.RemoveAll(item => item == null);

        // Check for win condition
        if (rowSpawnCount >= rowsToSpawn)
        {
            if (blocks.Count == 0)
            {
                // Mission Complete! 
                Debug.Log("MISSION COMPLETE!");
                Time.timeScale = 0;
                return;
            }
        }

        // on timer, move old blocks down one level
        // and generate new round of blocks above
        timeSinceLastBlockShift += Time.deltaTime;

        // Check blockShift timer
        if (timeSinceLastBlockShift > blockShiftInterval)
        {
            // If we've reached this point, it is time to shift all rows of blocks

            // Reset blockShift timer
            timeSinceLastBlockShift = 0;

            // Generate new row of blocks if the limit has not been reached        
            if (rowSpawnCount < rowsToSpawn)
            {
                List<GameObject> row = EndlessMode.GenerateRowOfBlocks(rowLength: levelWidth, blocks: blockPrefabs, skipPercentage: 0f, origin: origin, parent: tilemap);
                blocks.AddRange(row);
                rowSpawnCount++;
            }

            // Shift blocks down
            EndlessMode.ShiftBlocksDown(blocks, bottomY, this);
        }
    }
    
    // Shifts a list of blocks down
    // Checks for the GameOver condition
    // Returns true if gameOver or false if not.
    private static bool ShiftBlocksDown(List<GameObject> blocks, float bottomY, MonoBehaviour gameObject)
    {
        foreach (GameObject block in blocks)
        {
            if (block.transform.position.y <= bottomY)
            {
                // Game Over
                Debug.Log("GameOver");
                Time.timeScale = 0;

                // Note because the blocks spawn in from left to right, with the lowest row spawning first,
                // the first block in the blocks array will always be the lowest leftmost block.
                // By returning if the block is at the bottom, the StartCoroutine function will never be
                // called because the first block checked is always the lowest leftmost block.
                // This prevents the unwanted behavior of blocks shifting down one more row (into the wall) 
                // after the game over condition is met.
                return true;
            }
            gameObject.StartCoroutine(ShiftBlock(block: block));
        }
        return false;
    }

    // Shifts a single block down one unit
    private static IEnumerator ShiftBlock(GameObject block)
    {
        // Check if block is null
        if (block == null)
        {
            yield break;
        }

        // Shift block down one position
        Vector3 oldPosition = block.transform.position;
        for(int i = 0; i <= 10; i++)
        {
            // Check if block is null *again* because it could be destroyed between waits/yields
            if (block == null)
            {
                yield break;
            }
            // The block shifts down in 10 steps. Each step is 1/10th of a second.
            block.transform.position = oldPosition + new Vector3(x: 0, y: -1) * i / 10;            
            yield return new WaitForSeconds(1 / 10);
        }        
    }

    // Generates a row of block gameObjects. 
    // rowLength:       the number of blocks (including blank blocks) to generate
    // blocks:          the different block prefabs to choose from
    // skipPercentage:  the chance that a block should be blank/skipped
    // origin:          the x, y position of the topLeft block to use as an anchor for the rest of the blocks
    // parent:          the tilemap to use as the parent obect
    private static List<GameObject> GenerateRowOfBlocks(int rowLength, GameObject[] blocks, float skipPercentage, Vector3 origin, Tilemap parent)
    {
        List<int> blocksIndexList = GenerateRowOfIndices(rowLength, blocks.Length, skipPercentage);
        List<GameObject> blocksList = new List<GameObject>();

        for (int i = 0; i < blocksIndexList.Count; i++)
        {
            int index = blocksIndexList[i];
            if (index < 0)
            {
                // skip block if index is -1
            }
            else
            {
                Vector3 position = origin + new Vector3(x: 1, y: 0) * i;
                GameObject block = GameObject.Instantiate(original: blocks[index], position: position, rotation: Quaternion.identity, parent: parent.transform);
                blocksList.Add(item: block);
            }
        }

        return blocksList;
    }

    // Returns an array of integer indices. If the index is -1 then the the block is skipped.
    // Skip percentage is the chance that a block should be blank/skipped.
    // Skip percentage should be between 0 (0%, never skip) and 1 (100%, always skip)
    private static List<int> GenerateRowOfIndices(int rowLength, int blockCount, float skipPercentage)
    {
        List<int> indices = new List<int>();

        if (skipPercentage < 0 || skipPercentage > 1)
        {
            Debug.LogError("Incorrect skipPercentage value: " + skipPercentage + ". Value should be between 0 and 1 inclusive.");
            skipPercentage = 0;
        }

        for (int i = 0; i < rowLength; i++)
        {
            // Check if this block should be blank/skipped
            float shouldSkip = Random.Range(0f, 1f);
            if (shouldSkip < skipPercentage)
            {
                // Skip block
                indices.Add(item: -1);
            }
            else
            {
                // Add a random block
                int index = Random.Range(0, blockCount);
                indices.Add(item: index);
            }
        }
        return indices;
    }
}