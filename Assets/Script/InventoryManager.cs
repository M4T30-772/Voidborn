using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{

    [SerializeField] Image heartShards;
    //[SerializeField] Image manaShards;
    [SerializeField] GameObject upCast, sideCast, downCast;
    [SerializeField] GameObject dash, varJump, wallJump;

private void ResetUI()
{
    sideCast.SetActive(false);
    upCast.SetActive(false);
    downCast.SetActive(false);
    dash.SetActive(false);
    wallJump.SetActive(false);
    varJump.SetActive(false);
}

private void OnEnable()
{
    ResetUI();
    
    heartShards.fillAmount = PlayerController.Instance.heartShards * 0.25f;
    //manaShard.fillAmount = PlayerController.Instance.orbShard * 0.34f;

    //spells
    if(PlayerController.Instance.unlockedSideCast)
    {
        sideCast.SetActive(true);
    }
        if(PlayerController.Instance.unlockedUpCast)
    {
        upCast.SetActive(true);
    }
        if(PlayerController.Instance.unlockedDownCast)
    {
        downCast.SetActive(true);
    }

    //abilities
    if(PlayerController.Instance.unlockedDash)
    {
        dash.SetActive(true);
    }
        if(PlayerController.Instance.unlockedWallJump)
    {
        wallJump.SetActive(true);
    }
        if(PlayerController.Instance.unlockedVarJump)
    {
        varJump.SetActive(true);
    }
}
}