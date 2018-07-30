using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : ScriptableObject 
{
    [System.Serializable]
    public class Info
    {
		public enum Character
		{
            NONE = 0,
			UNITY_CHAN,
			CHIBI_CHAN,
            KOSAME_HISAKAKI,
            TEST,
            HARUKA,
            BOSS,
            TOORU
		};

        public Character name;
        public Color color;
        public List<Sprite> spriteList;
        public List<Transform> charPosList = new List<Transform>();

        public Info()
        {
            this.name = Character.UNITY_CHAN;
            this.color = Color.white;
            this.spriteList = new List<Sprite>(1);
            this.charPosList = new List<Transform>();
        }

        public Info(Character name, List<Sprite> spriteList, Color color, List<Transform> charPosList)
        {
            this.name = name;
            this.color = color;
            this.spriteList = spriteList;
            this.charPosList = charPosList;
        }
    }

    public List<Info> characterList;

    //----------------------------------------------------------------------------------------------
    //-------------------------------------PUBLIC FUNCTIONS-----------------------------------------
    //----------------------------------------------------------------------------------------------

    public List<Sprite> GetCharacterSprites(Info.Character name)
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            if (characterList[i].name == name)
            {
                return characterList[i].spriteList;
            }
        }
        return null;
    }

    public int GetSpriteIndex(Info.Character name, Sprite sprite)
    {
        if(sprite == null) { Debug.Log("Sprite is null."); return -1; }

        for (int i = 0; i < characterList.Count; i++)
        {
            if (characterList[i].name == name)
            {
                for (int j = 0; j < characterList[i].spriteList.Count; j++)
                {
                    if (characterList[i].spriteList[j] == sprite) return j;
                }
            }
        }
        return -1;
    }

    public Color GetCharacterColor(string name)
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            string tempName = characterList[i].name.ToString();
            if (name == tempName) { return characterList[i].color; }
        }
        return Color.white;
    }
}
