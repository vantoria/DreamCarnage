using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour 
{
    public static TagManager sSingleton { get { return _sSingleton; } }
    static TagManager _sSingleton;

    // Visual novel tags.
    public string dialogueBoxTag = "DialogueBox";
    public string leftCharacterTag = "LeftCharacter";
    public string rightCharacterTag = "RightCharacter";
    public string answerBoxTag = "AnswerBox";
    public string answerBoxBgTag = "AnswerBoxBg";

    // Dream Carnage tags.
	public string gmTag = "GameManager";
    public string canvasTag = "Canvas";
    public string player1Tag = "Player1";
    public string player2Tag = "Player2";
    public string hitboxTag = "Hitbox";
    public string player1BulletTag = "Player1Bullet";
    public string player2BulletTag = "Player2Bullet";
    public string reviveCircleTag = "ReviveCircle";
    public string magnumRadTag = "MagnumRadius";
    public string revivingPSName = "RevivingPS";

    // Skill
    public string bombShieldTag = "BombShield";
    public string bulletWipeTag = "BulletWipe";

    public string enemyTag = "Enemy";
    public string enemyBulletTag = "EnemyBullet";
    public string attackPatternTag = "AttackPattern";

    // Tags for environmental objects.
    public string ENV_OBJ_PowerUp1Tag = "PowerUp1";
    public string ENV_OBJ_PowerUp2Tag = "PowerUp2";
    public string ENV_OBJ_ScorePickUp1Tag = "ScorePickUp1";
    public string ENV_OBJ_ScorePickUp2Tag = "ScorePickUp2";
    public string ENV_OBJ_LifePickUpTag = "Life";
    public string ENV_OBJ_RockTag = "Rock";
    public string ENV_OBJ_DamagePlayerTag = "DamagePlayer";
    public string ENV_OBJ_ScoreBigPickUpTag = "ScoreBigPickUp";
    public string ENV_OBJ_CrateTag = "Crate";

    // Name for intial instantiated transform.
    public string player1BulletName = "Player1Bullet";
    public string player2BulletName = "Player2Bullet";
    public string enemy1BulletName = "Enemy1Bullet";

    // Name for UI.
    public string UI_SubjectName = "SubjectName";
    public string UI_HighScoreName = "HiScore_Display";
    public string UI_HighScoreMultName = "Multiplier_Display";
    public string UI_HighScoreMultXName = "MultiplierX";
    public string UI_ScoreName = "Score_Display";
    public string UI_LifePointName = "Life_Display";
    public string UI_PowerLevelName = "Power_Display";
    public string UI_BombName = "Bomb_Display";
    public string UI_LinkName = "Link";
    public string UI_LinkBarName = "LinkBar_Display";
    public string UI_LinkBarInsideName = "LinkBarInside";
    public string UI_LinkMaxName = "Max";
	public string UI_PauseMenuName = "PauseMenu";
    public string UI_GameOverMenuName = "GameOverMenu";
    public string UI_DeathDisplayName = "Death_Display";
    public string UI_RedCrossName = "RedCross";
    public string UI_PlyBombPotraitName = "BombPotrait";
    public string UI_BonusScoreName = "Score";
    public string UI_ReviveSoulName = "ReviveSoulInside";
    public string UI_KillScoreName = "ScorePopUp";
    public string UI_PowerUpTextName = "PowerUpText";

	public string UI_DeathWaveName = "DeathWave";
	public string UI_GetHitImpactName = "GetHitImpact";
    public string UI_GetHitImpactBigName = "GetHitBigImpact";
    public string UI_RifleShotSparksName = "RifleShotSparks";
    public string UI_SniperShotSparksName = "SniperShotSparks";
    public string UI_MagnumShotSparksName = "MagnumShotSparks";
	public string UI_RockDestroyName = "RockDestroy";

    public string repelStat = "RepelStat";

    // Layers
    public string playerBulletLayer = "PlayerBullet";
    public string playerBulletNoDestroyLayer = "P.Bullet_No_Destroy";

    // Sorting layers.
    public string sortLayerTopG = "TopGround";

    void Awake()
    {
        if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
        else _sSingleton = this;
    }
}
