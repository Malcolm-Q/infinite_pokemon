using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using OpenAI_API.Images;

public class OpenAIController : MonoBehaviour
{
    private OpenAIAPI api;

    private string[] pokeTypes = new string[] {
        "Fire",
        "Water",
        "Earth",
        "Wind",
        "Normal",
    };

    private string[] pokeStrengths = new string[]{
        "Wind",
        "Earth",
        "Fire",
        "Water",
        "Normal"
    };

    public async Task<bool> SetKey(string key)
    {
        api = new OpenAIAPI(key);
        await api.Completions.GetCompletion("test");
        return true;
    }
    public async Task<Pokemon> PokemonConstructor()
    {
        int idx = UnityEngine.Random.Range(0,pokeTypes.Length);
        string pokeType = pokeTypes[idx];
        string name = await api.Completions.GetCompletion("Only answer with the name!!! Generate a name for a random " + pokeType + " influenced animal/creature that has a random verb.");

        var moveTask = api.Completions.GetCompletion("Output your answer with the names separated by commas. Generate 3 RPG style attack moves that a " + name + " may have.");
        var imgTask = api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest("Pixel art of " + name.Trim() + " black background", 1, ImageSize._256));

        await Task.WhenAll(moveTask, imgTask);

        string result = await moveTask;
        var img = await imgTask;

        string[] moves = result.Split(',');

        return new Pokemon(name, MoveSetConstructor(moves, pokeType), img.Data[0].Url, pokeType, pokeStrengths[idx]);
    }

    private Move[] MoveSetConstructor(string[] moves, string pokeType)
    {
        string type1 = (UnityEngine.Random.Range(0f,100f) > 66) ? pokeType : "Normal";
        string type2 = (UnityEngine.Random.Range(0f,100f) > 66) ? pokeType : "Normal";
        string type3 = (UnityEngine.Random.Range(0f,100f) > 66) ? pokeType : "Normal";
        return new Move[] {
            new Move(moves[0].Trim(), type1, UnityEngine.Random.Range(10,35)),
            new Move(moves[1].Trim(), type2, UnityEngine.Random.Range(10,35)),
            new Move(moves[2].Trim(), type3, UnityEngine.Random.Range(10,35)),
        };
    }
}

public class Pokemon
{
    public int Health {get; set;}
    public string Name {get; set;}
    public string ImgUrl {get; set;}
    public string PokeType {get; set;}
    public Move[] MoveSet {get; set;}
    public int MaxHealth {get; set;}
    public string PokeStrength {get; set;}

    public Pokemon(string name, Move[] moveSet, string imgUrl, string pokeType, string pokeStrength)
    {
        Health = UnityEngine.Random.Range(80,120);
        MaxHealth = Health;
        Name = name;
        ImgUrl = imgUrl;
        MoveSet = moveSet;
        PokeType = pokeType;
        PokeStrength = pokeStrength;
    }
}


public class Move
{
    public string MoveName {get; set;}
    public string MoveType {get; set;}
    public int MoveDamage {get; set;}

    public Move(string moveName, string moveType, int moveDamage)
    {
        MoveName = moveName;
        MoveDamage = moveDamage;
        MoveType = moveType;
    }
}