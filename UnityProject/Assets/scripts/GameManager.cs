using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private List<TextMeshProUGUI> playerMoves;
    [SerializeField] private TextMeshProUGUI attackStatus, enemyName, playerName;
    [SerializeField] private Image playerPortrait, enemyPortrait, playerHealthBar, enemyHealthBar;
    [SerializeField] private GameObject loading;
    [SerializeField] private OpenAIController openai;
    private Pokemon playerPokemon, enemyPokemon; 
    private bool playerCanAttack = true;

    private async void Start()
    {
        instance = this;
        // build player and enemy
        playerPokemon = await openai.PokemonConstructor();
        enemyPokemon = await openai.PokemonConstructor();

        StartCoroutine(LoadImage(playerPokemon.ImgUrl, playerPortrait));
        StartCoroutine(LoadImage(enemyPokemon.ImgUrl, enemyPortrait));

        playerName.text = playerPokemon.Name.Trim();
        enemyName.text = enemyPokemon.Name.Trim();

        for (int i = 0; i < playerPokemon.MoveSet.Length; i++)
        {
            playerMoves[i].text = playerPokemon.MoveSet[i].MoveName;
        }
        loading.SetActive(false);
    }

    private async void NewPokemon(bool player, bool enem)
    {
        if(player)
        {
            enemyPokemon = await openai.PokemonConstructor();
            StartCoroutine(LoadImage(enemyPokemon.ImgUrl, enemyPortrait));
            enemyName.text = enemyPokemon.Name.Trim();
        }
        if(enem)
        {
            playerPokemon = await openai.PokemonConstructor();
            StartCoroutine(LoadImage(playerPokemon.ImgUrl, playerPortrait));
            playerName.text = playerPokemon.Name.Trim();
        }
        loading.SetActive(false);
        playerCanAttack=true;
    }

    private void PokeAttack(int idx, bool player)
    {
        Debug.Log(player);
        if(player && playerCanAttack==false){return;}
        else if(player){playerCanAttack=false;}

        Pokemon atkPoke = player ? playerPokemon : enemyPokemon;
        Pokemon defPoke = player ? enemyPokemon  : playerPokemon;
        Image health = player ? enemyHealthBar : playerHealthBar;

        attackStatus.text = atkPoke.Name + " used " + atkPoke.MoveSet[idx].MoveName + "!"; 
        defPoke.Health -= atkPoke.MoveSet[idx].MoveDamage;

        Vector3 scale = health.transform.localScale;
        health.transform.localScale = new Vector3((float)defPoke.Health / (float)defPoke.MaxHealth, scale.y, scale.z);

        if (defPoke.Health <= 0)
        {
            loading.SetActive(true);
            if(!player){NewPokemon(true, true);}
            else{NewPokemon(false,true);}
        }
        else{StartCoroutine(WaitForMove(player));}
    }

    public void ExecutePlayerAttack(int idx)
    {
        //this is dumb but I'm feeling lazy
        PokeAttack(idx, true);
    }


    private IEnumerator WaitForMove(bool player)
    {
        yield return new WaitForSeconds(1f);
        attackStatus.text = "";
        yield return new WaitForSeconds(1f);
        if(player){PokeAttack(UnityEngine.Random.Range(0,3), false);}
        else{playerCanAttack=true;}
    }
    private IEnumerator LoadImage(string imageUrl, Image targetImage)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error loading image: " + www.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            Debug.Log(texture.width);
            targetImage.sprite = sprite;
            Debug.Log("set");
        }
    }
}
