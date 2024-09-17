using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class SaveData
{
    public static SaveData Instance { get; private set; } = new SaveData();

    public HashSet<string> sceneNames = new HashSet<string>();
    public string benchSceneName;
    public Vector2 benchPos;
    public int playerHealth;
    public int playerMaxHealth;
    public int playerHeartShards;
    public float playerMana;
    public Vector2 playerPosition;
    public string lastScene;
    public bool playerUnlockedWallJump;
    public bool playerUnlockedDash;
    public bool playerUnlockedVarJump;
    public bool playerUnlockedSideCast;
    public bool playerUnlockedUpCast;    
    public bool playerUnlockedDownCast;
    public int money;
    public int weaponDamage;
    public int spellDamage;

    public void Initialize()
    {
        // Initialize bench save file
        string benchFilePath = Application.persistentDataPath + "/save.bench.data";
        if (!File.Exists(benchFilePath))
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(benchFilePath)))
            {
                writer.Write(string.Empty); // benchSceneName
                writer.Write(0f); // benchPos.x
                writer.Write(0f); // benchPos.y
            }
        }

        // Initialize player save file
        string playerFilePath = Application.persistentDataPath + "/save.player.data";
        if (!File.Exists(playerFilePath))
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(playerFilePath)))
            {
                writer.Write(0); // playerHealth
                writer.Write(0); // playerMaxHealth
                writer.Write(0); // playerHeartShards
                writer.Write(0f); // playerMana
                writer.Write(0f); // playerPosition.x
                writer.Write(0f); // playerPosition.y
                writer.Write(string.Empty); // lastScene
                writer.Write(false); // playerUnlockedWallJump
                writer.Write(false); // playerUnlockedDash
                writer.Write(false); // playerUnlockedVarJump
                writer.Write(false); // playerUnlockedSideCast
                writer.Write(false); // playerUnlockedUpCast
                writer.Write(false); // playerUnlockedDownCast
                writer.Write(0); // money
                writer.Write(0); // weaponDamage
                writer.Write(0); // spellDamage
            }
        }
    }

    public void ResetData()
    {
        string benchFilePath = Application.persistentDataPath + "/save.bench.data";
        string playerFilePath = Application.persistentDataPath + "/save.player.data";

        if (File.Exists(benchFilePath))
        {
            File.Delete(benchFilePath);
        }

        if (File.Exists(playerFilePath))
        {
            File.Delete(playerFilePath);
        }

        Initialize();
        
        // Reset in-memory data
        sceneNames.Clear();
        benchSceneName = string.Empty;
        benchPos = Vector2.zero;
        playerHealth = 0;
        playerMaxHealth = 0;
        playerHeartShards = 0;
        playerMana = 0f;
        playerPosition = Vector2.zero;
        lastScene = string.Empty;
        playerUnlockedWallJump = false;
        playerUnlockedDash = false;
        playerUnlockedVarJump = false;
        playerUnlockedSideCast = false;
        playerUnlockedUpCast = false;
        playerUnlockedDownCast = false;
        money = 0;
        weaponDamage = 1;
        spellDamage = 2;
    }

    public void SaveBench()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.bench.data")))
        {
            writer.Write(benchSceneName ?? string.Empty);
            writer.Write(benchPos.x);
            writer.Write(benchPos.y);
        }
    }

    public void LoadBench()
    {
        if (File.Exists(Application.persistentDataPath + "/save.bench.data"))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.bench.data")))
            {
                benchSceneName = reader.ReadString();
                benchPos.x = reader.ReadSingle();
                benchPos.y = reader.ReadSingle();
            }
        }
    }

    public void SavePlayerData()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Create(Application.persistentDataPath + "/save.player.data")))
        {
            writer.Write(PlayerController.Instance.Health);
            writer.Write(PlayerController.Instance.maxHealth);
            writer.Write(PlayerController.Instance.heartShards);
            writer.Write(PlayerController.Instance.Mana);
            writer.Write(PlayerController.Instance.unlockedWallJump);
            writer.Write(PlayerController.Instance.unlockedDash);
            writer.Write(PlayerController.Instance.unlockedVarJump);
            writer.Write(PlayerController.Instance.unlockedSideCast);
            writer.Write(PlayerController.Instance.unlockedUpCast);
            writer.Write(PlayerController.Instance.unlockedDownCast);
            writer.Write(PlayerController.Instance.transform.position.x);
            writer.Write(PlayerController.Instance.transform.position.y);
            writer.Write(SceneManager.GetActiveScene().name);
            writer.Write(money);
            writer.Write(weaponDamage);
            writer.Write(spellDamage);
        }
    }

    public void LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/save.player.data"))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.player.data")))
            {
                try
                {
                    playerHealth = reader.ReadInt32();
                    playerMaxHealth = reader.ReadInt32();
                    playerHeartShards = reader.ReadInt32();
                    playerMana = reader.ReadSingle();
                    playerUnlockedWallJump = reader.ReadBoolean();
                    playerUnlockedDash = reader.ReadBoolean();
                    playerUnlockedVarJump = reader.ReadBoolean();
                    playerUnlockedSideCast = reader.ReadBoolean();
                    playerUnlockedUpCast = reader.ReadBoolean();
                    playerUnlockedDownCast = reader.ReadBoolean();
                    playerPosition.x = reader.ReadSingle();
                    playerPosition.y = reader.ReadSingle();
                    lastScene = reader.ReadString();
                    money = reader.ReadInt32();
                    weaponDamage = reader.ReadInt32();
                    spellDamage = reader.ReadInt32();

                    SceneManager.LoadScene(lastScene);
                    PlayerController.Instance.transform.position = playerPosition;
                    PlayerController.Instance.Health = playerHealth;
                    PlayerController.Instance.maxHealth = playerMaxHealth;
                    PlayerController.Instance.heartShards = playerHeartShards;
                    PlayerController.Instance.Mana = playerMana;
                    PlayerController.Instance.unlockedWallJump = playerUnlockedWallJump;
                    PlayerController.Instance.unlockedDash = playerUnlockedDash;
                    PlayerController.Instance.unlockedVarJump = playerUnlockedVarJump;
                    PlayerController.Instance.unlockedSideCast = playerUnlockedSideCast;
                    PlayerController.Instance.unlockedUpCast = playerUnlockedUpCast;
                    PlayerController.Instance.unlockedDownCast = playerUnlockedDownCast;
                    PlayerController.Instance.damage = weaponDamage;
                    PlayerController.Instance.spellDamage = spellDamage;
                }
                catch (EndOfStreamException e)
                {
                    Debug.LogError("EndOfStreamException: " + e.Message);
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception: " + e.Message);
                }
            }
        }
    }
}
