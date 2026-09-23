using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SurakshaAR.Backend.Services;

public class BlockchainService
{
    private readonly MongoService _mongo;
    private readonly List<Block> _chain = new();
    private readonly object _lock = new();

    public BlockchainService(MongoService mongo)
    {
        _mongo = mongo;
        LoadChain();
    }

    private void LoadChain()
    {
        var collection = _mongo.Collection<Block>("blockchain");
        var blocks = collection.Find(_ => true).SortBy(b => b.Index).ToList();
        if (blocks.Count > 0)
        {
            _chain.AddRange(blocks);
        }
        else
        {
            var genesis = new Block
            {
                Id = ObjectId.GenerateNewId(),
                Index = 0,
                Timestamp = DateTime.UtcNow,
                Data = "Genesis Block - SurakshaAR",
                PreviousHash = "0",
                Hash = ComputeHash(0, "Genesis Block - SurakshaAR", "0")
            };
            _chain.Add(genesis);
            collection.InsertOne(genesis);
        }
    }

    public string RecordEvent(string eventType, string userId, string details)
    {
        lock (_lock)
        {
            var lastBlock = _chain[^1];
            var data = $"{eventType}|{userId}|{details}|{DateTime.UtcNow:O}";
            var newBlock = new Block
            {
                Id = ObjectId.GenerateNewId(),
                Index = _chain.Count,
                Timestamp = DateTime.UtcNow,
                Data = data,
                PreviousHash = lastBlock.Hash,
                Hash = ComputeHash(_chain.Count, data, lastBlock.Hash)
            };
            _chain.Add(newBlock);
            _mongo.Collection<Block>("blockchain").InsertOne(newBlock);
            return $"LOCAL-{newBlock.Hash}";
        }
    }

    public string RecordCertificate(string certificateId, string employeeId, string moduleId, int score)
    {
        return RecordEvent("certificate", employeeId, $"{certificateId}|{moduleId}|{score}");
    }

    public string GetLastHash()
    {
        lock (_lock)
        {
            return _chain.Count > 0 ? _chain[^1].Hash : "0";
        }
    }

    public bool VerifyChain()
    {
        lock (_lock)
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
    }

    public List<Block> GetChain()
    {
        lock (_lock) { return new(_chain); }
    }

    public string ComputeHash(int index, string data, string previousHash)
    {
        var input = $"{index}{data}{previousHash}";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

public class Block
{
    public ObjectId Id { get; set; }
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public string Data { get; set; } = string.Empty;
    public string PreviousHash { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
