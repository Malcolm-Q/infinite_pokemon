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
    [SerializeField] private TextMeshProUGUI pokemonName, pokemonMove1, pokemonMove2, pokemonMove3;
    private OpenAIAPI api;
    [SerializeField] private Image playerImage;

    public async Task<Pokemon> PokemonConstructor()
    {
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("openai_key"));
        var name = await api.Completions.GetCompletion("generate a name for a random animal that has a random attribute");
        var result = await api.Completions.GetCompletion("Output you answer with the names seperated by commas. Generate 3 rpg style attack moves that a " + name + " may have.");
        string[] moves = result.Split(',');
        var img = await api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest("Pixel art of " + name.Trim(), 1, ImageSize._256));

        return new Pokemon(name, MoveSetConstructor(moves), img.Data[0].Url);
    }

    private Move[] MoveSetConstructor(string[] moves)
    {
        return new Move[] {
            new Move(moves[0].Trim(), UnityEngine.Random.Range(10,35)),
            new Move(moves[1].Trim(), UnityEngine.Random.Range(10,35)),
            new Move(moves[2].Trim(), UnityEngine.Random.Range(10,35)),
        };
    }
}

public class Pokemon
{
    public int Health {get; set;}
    public string Name {get; set;}
    public string ImgUrl {get; set;}
    public Move[] MoveSet {get; set;}
    public int MaxHealth {get; set;}

    public Pokemon(string name, Move[] moveSet, string imgUrl)
    {
        Health = UnityEngine.Random.Range(80,120);
        MaxHealth = Health;
        Name = name;
        ImgUrl = imgUrl;
        MoveSet = moveSet;
    }
}


public class Move
{
    public string MoveName {get; set;}
    public int MoveDamage {get; set;}

    public Move(string moveName, int moveDamage)
    {
        MoveName = moveName;
        MoveDamage = moveDamage;
    }
}