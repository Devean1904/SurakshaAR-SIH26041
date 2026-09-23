using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class LocalBlockchain : MonoBehaviour
{
    public static LocalBlockchain Instance { get; private set; }

    private readonly List<LocalBlock> _chain = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateGenesisBlock();
    }

    void CreateGenesisBlock()
    {
        _chain.Add(new LocalBlock
        {
            Index = 0,
            Timestamp = System.DateTime.UtcNow.ToString("O"),
            Data = "Genesis - SurakshaAR Offline Chain",
            PreviousHash = "0",
            Hash = ComputeHash(0, "Genesis - SurakshaAR Offline Chain", "0")
        });
    }

    public string AddBlock(string eventType, string userId, string details)
    {
        var lastBlock = _chain[^1];
        var data = $"{eventType}|{userId}|{details}|{System.DateTime.UtcNow:O}";
        var newBlock = new LocalBlock
        {
            Index = _chain.Count,
            Timestamp = System.DateTime.UtcNow.ToString("O"),
            Data = data,
            PreviousHash = lastBlock.Hash,
            Hash = ComputeHash(_chain.Count, data, lastBlock.Hash)
        };
        _chain.Add(newBlock);
        Debug.Log($"[Blockchain] Block #{newBlock.Index} added: {eventType}");
        return newBlock.Hash;
    }

    public bool VerifyIntegrity()
    {
        for (int i = 1; i < _chain.Count; i++)
        {
            var current = _chain[i];
            var previous = _chain[i - 1];
            if (current.Hash != ComputeHash(current.Index, current.Data, current.PreviousHash))
                return false;
            if (current.PreviousHash != previous.Hash)
                return false;
        }
        return true;
    }

    public string ComputeHash(int index, string data, string previousHash)
    {
        var input = $"{index}{data}{previousHash}";
        byte[] bytes;
        using (var sha256 = SHA256.Create())
        {
            bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public int GetChainLength() => _chain.Count;
    public List<LocalBlock> GetChain() => new(_chain);
}

[System.Serializable]
public class LocalBlock
{
    public int Index;
    public string Timestamp;
    public string Data;
    public string PreviousHash;
    public string Hash;
}
