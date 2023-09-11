using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private List<TextMeshProUGUI> playerMoves, playerMoveAttr;
    [SerializeField] private TextMeshProUGUI attackStatus, enemyName, playerName, loadText, keyStatus;
    [SerializeField] private Image playerPortrait, enemyPortrait, playerHealthBar, enemyHealthBar;
    [SerializeField] private GameObject loading, getKeyScreen;
    [SerializeField] private OpenAIController openai;
    [SerializeField] private InputField keyInput;
    private Pokemon playerPokemon, enemyPokemon; 
    private bool playerCanAttack = true;
    private Dictionary<string, Color> atkTypeColors = new Dictionary<string, Color>();

    private void Start()
    {
        instance = this;
        atkTypeColors["Fire"] = Color.red;
        atkTypeColors["Water"] = Color.blue;
        atkTypeColors["Earth"] = Color.green;
        atkTypeColors["Wind"] = Color.cyan;
        atkTypeColors["Normal"] = Color.white;
    }

    public async void SubmitKey()
    {
        bool success = await openai.SetKey(keyInput.text);
        if(!success){
            keyStatus.text = "invalid API key!";
            return;
            }
        getKeyScreen.SetActive(false);
        StartGame();
    }

    private async void StartGame()
    {
        loadText.text = "Constructing Pokemon...";
        var playerTask = openai.PokemonConstructor();
        var enemyTask = openai.PokemonConstructor();

        await Task.WhenAll(playerTask, enemyTask);

        playerPokemon = await playerTask;
        enemyPokemon = await enemyTask;

        loadText.text = "Setting Scene...";

        StartCoroutine(LoadImage(playerPokemon.ImgUrl, playerPortrait));
        StartCoroutine(LoadImage(enemyPokemon.ImgUrl, enemyPortrait));

        playerName.text = playerPokemon.Name.Trim();
        enemyName.text = enemyPokemon.Name.Trim();

        for (int i = 0; i < playerPokemon.MoveSet.Length; i++)
        {
            playerMoves[i].text = playerPokemon.MoveSet[i].MoveName;
            playerMoveAttr[i].text = playerPokemon.MoveSet[i].MoveType + " // " + playerPokemon.MoveSet[i].MoveDamage.ToString();
            playerMoveAttr[i].color = atkTypeColors[playerPokemon.MoveSet[i].MoveType];
        }
        await Task.Delay(500);

        loading.SetActive(false);
    }

    private async void NewPokemon(bool player, bool enem)
    {
        attackStatus.text = "";
        if(enem)
        {
            loadText.text = "Constructing New Opponent...";
            enemyPokemon = await openai.PokemonConstructor();
            StartCoroutine(LoadImage(enemyPokemon.ImgUrl, enemyPortrait));
            enemyName.text = enemyPokemon.Name.Trim();
        }
        if(player)
        {
            loadText.text = "You died :(\nMaking You a New Pokemon...";
            playerPokemon = await openai.PokemonConstructor();
            StartCoroutine(LoadImage(playerPokemon.ImgUrl, playerPortrait));
            playerName.text = playerPokemon.Name.Trim();
            for (int i = 0; i < playerPokemon.MoveSet.Length; i++)
            {
                playerMoves[i].text = playerPokemon.MoveSet[i].MoveName;
                playerMoveAttr[i].text = playerPokemon.MoveSet[i].MoveType + " // " + playerPokemon.MoveSet[i].MoveDamage.ToString();
                playerMoveAttr[i].color = atkTypeColors[playerPokemon.MoveSet[i].MoveType];
            }
        }

        //reset both enemy and player so you can fight until you
        //meet your match and vice versa.
        enemyPokemon.Health = enemyPokemon.MaxHealth;
        enemyHealthBar.transform.localScale = Vector3.one;
        playerPokemon.Health = playerPokemon.MaxHealth;
        playerHealthBar.transform.localScale = Vector3.one;
        await Task.Delay(500);

        loading.SetActive(false);
        playerCanAttack=true;
    }

    private void PokeAttack(int idx, bool player)
    {
        if(player && playerCanAttack==false){return;}
        else if(player){playerCanAttack=false;}

        Pokemon atkPoke = player ? playerPokemon : enemyPokemon;
        Pokemon defPoke = player ? enemyPokemon  : playerPokemon;
        Image health = player ? enemyHealthBar : playerHealthBar;

        attackStatus.text = atkPoke.Name + " used " + atkPoke.MoveSet[idx].MoveName + "!"; 
        int dmg = atkPoke.MoveSet[idx].MoveDamage;
        if(atkPoke.MoveSet[idx].MoveType != "Normal")
        {
            if(defPoke.PokeType == atkPoke.PokeStrength) {
                dmg *= 2;
                attackStatus.text += "\nIt's Super Effective!";    
            }
            else if(defPoke.PokeStrength == atkPoke.PokeType) {
                dmg /= 2;
                attackStatus.text += "\nIt's Not Very Effective...";    
            }
        }
        defPoke.Health -= dmg;
        health.transform.localScale = new Vector3((float)defPoke.Health / (float)defPoke.MaxHealth, 1, 1);

        if (defPoke.Health <= 0)
        {
            loading.SetActive(true);
            if(player){NewPokemon(false, true);}
            else{NewPokemon(true,false);}
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
        if(!player){playerCanAttack=true;}
        else{
            yield return new WaitForSeconds(1f);
            PokeAttack(UnityEngine.Random.Range(0,3), false);
        }
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
            targetImage.sprite = sprite;
        }
    }
}
