using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttackPattern))]
public class AttackPatternCI : Editor 
{
    AttackPattern mSelf;

	void OnEnable () 
	{
        mSelf = (AttackPattern)target;
	}

	public override void OnInspectorGUI()
	{
        serializedObject.Update();

        // ----------------------------------------------- Base stat -------------------------------------------------
        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("Base Stat");
        GUILayout.EndVertical ();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ownerType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("owner"));

        if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Primary Weapon");
        }

		EditorGUILayout.PropertyField(serializedObject.FindProperty("isMainPiercing"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isAlternateFire"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("isMagnum"));
        if (mSelf.isMagnum)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magnumMarkedDuration"));
        }

		if (mSelf.isAlternateFire) EditorGUILayout.PropertyField(serializedObject.FindProperty("alternateFireOffset"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainBulletDamage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainBulletSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseEverySec"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseDur"));

        if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainBulletOffset"));

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Secondary Weapon");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isSecondaryPiercing"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryAttackType"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletType"));
            if (mSelf.bulletType == AttackPattern.BulletType.PROJECTILE || mSelf.bulletType == AttackPattern.BulletType.PROJECTILE_LOCK_ON)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryBulletDamage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryBulletSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletDirection"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryBulletOffset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("withinRange"));
            }
            else if (mSelf.bulletType == AttackPattern.BulletType.LASER)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damagePerFrame"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("expandSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("expandTillXScale"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isDestroyBullets"));
            }
        }
        else if (mSelf.ownerType != AttackPattern.OwnerType.PLAYER)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shootDelay")); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hpPercentSkipAtk")); 
        }

        // --------------------------------------------- Bullet Preview -----------------------------------------------
        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("Bullet Preview");
        GUILayout.EndVertical ();

        if (BulletManager.sSingleton != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletPrefabData"));
            BulletPrefabData currBulletData = BulletManager.sSingleton.bulletPrefabData;

            Texture2D mainTexture = null, secondaryTexture = null;
            if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
            {
                Transform mainBullet = currBulletData.plyMainBulletTransList[mSelf.mainBulletIndex];
                Transform secondaryBullet = null;

                int currIndex = mSelf.secondaryBulletIndex;
				if (mSelf.bulletType == AttackPattern.BulletType.PROJECTILE || mSelf.bulletType == AttackPattern.BulletType.PROJECTILE_LOCK_ON)
                {
                    int totalIndex = currBulletData.plySecondaryBulletTransList.Count - 1;
                    if (currIndex > totalIndex) mSelf.secondaryBulletIndex = totalIndex;

                    secondaryBullet = currBulletData.plySecondaryBulletTransList[mSelf.secondaryBulletIndex];
                }
                else if (mSelf.bulletType == AttackPattern.BulletType.LASER)
                {
                    int totalIndex = currBulletData.plyLaserTransNoCacheList.Count - 1;
                    if (currIndex > totalIndex) mSelf.secondaryBulletIndex = totalIndex;

                    if (totalIndex >= 0) secondaryBullet = currBulletData.plyLaserTransNoCacheList[mSelf.secondaryBulletIndex];
                }

                mainTexture = mainBullet.GetComponentInChildren<SpriteRenderer>().sprite.texture;
                secondaryTexture = secondaryBullet.GetComponentInChildren<SpriteRenderer>().sprite.texture;
            }
            else mainTexture = currBulletData.enemyBulletTransList[mSelf.mainBulletIndex].GetComponent<SpriteRenderer>().sprite.texture;

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
                {
                    GUILayout.Space(20);
                    GUILayout.Label("Primary bullet");
                    GUILayout.Space(45);
                    GUILayout.Label("Secondary bullet");
                }
                else
                {
                    GUILayout.Space(15);
                    GUILayout.Label("Primary bullet");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);

                // --------------------------------------- Primary bullet. --------------------------------------- 
                GUILayout.BeginVertical();
                GUILayout.Space(20);
                if(mSelf.mainBulletIndex - 1 < 0) GUI.enabled = false;
                if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(30))) mSelf.mainBulletIndex--;
                GUILayout.EndVertical();

                GUI.enabled = false;
                EditorGUILayout.ObjectField(mainTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
                GUI.enabled = true;

                int max = 0;
                if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER) max = currBulletData.plyMainBulletTransList.Count - 1;
                else max = currBulletData.enemyBulletTransList.Count - 1;

                if(mSelf.mainBulletIndex + 1 > max) GUI.enabled = false;
                else GUI.enabled = true;

                GUILayout.BeginVertical();
                GUILayout.Space(20);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(30))) mSelf.mainBulletIndex++;
                GUI.enabled = true;
                GUILayout.EndVertical();

                // --------------------------------------- Secondary bullet ---------------------------------------

                int secondaryTotal = 0;
				if (mSelf.bulletType == AttackPattern.BulletType.PROJECTILE || mSelf.bulletType == AttackPattern.BulletType.PROJECTILE_LOCK_ON) 
					secondaryTotal = currBulletData.plySecondaryBulletTransList.Count;
                else if (mSelf.bulletType == AttackPattern.BulletType.LASER) secondaryTotal = currBulletData.plyLaserTransNoCacheList.Count;

                if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    if(mSelf.secondaryBulletIndex - 1 < 0) GUI.enabled = false;
                    if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(30))) mSelf.secondaryBulletIndex--;
                    GUILayout.EndVertical();

                    GUI.enabled = false;
					if (mSelf.bulletType == AttackPattern.BulletType.PROJECTILE || mSelf.bulletType == AttackPattern.BulletType.PROJECTILE_LOCK_ON)
                    {
                        EditorGUILayout.ObjectField(secondaryTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
                    }
                    else if (mSelf.bulletType == AttackPattern.BulletType.LASER)
                    {
                        EditorGUILayout.ObjectField(secondaryTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
                    }

                    GUI.enabled = true;

                    if(mSelf.secondaryBulletIndex + 1 > secondaryTotal - 1) GUI.enabled = false;
                    else GUI.enabled = true;

                    GUILayout.BeginVertical();
                    GUILayout.Space(20);
                    if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(30))) mSelf.secondaryBulletIndex++;
                    GUI.enabled = true;
                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (mSelf.ownerType == AttackPattern.OwnerType.PLAYER)
                {
                    GUILayout.Space(12);
                    GUILayout.Label((mSelf.mainBulletIndex + 1).ToString() + " / " + currBulletData.plyMainBulletTransList.Count.ToString());
                    GUILayout.Space(110);
                    GUILayout.Label((mSelf.secondaryBulletIndex + 1).ToString() + " / " + secondaryTotal.ToString());
                }
                else
                {
                    GUILayout.Space(10);
                    GUILayout.Label((mSelf.mainBulletIndex + 1).ToString() + " / " + currBulletData.enemyBulletTransList.Count.ToString());
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            // ---------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        // ----------------------------------------- Current attack pattern -------------------------------------------
        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("Attack Template");
        GUILayout.EndVertical();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("template"));

        if (mSelf.template == AttackPattern.Template.SINGLE_SHOT)
        {
            if (mSelf.ownerType != AttackPattern.OwnerType.PLAYER)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isShootPlayer"));
                if (mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isFollowPlayer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                }
                if (!mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isDirectionFromOwner"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shootDirection"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialSpacing"));
        }
        else if (mSelf.template == AttackPattern.Template.ANGLE_SHOT)
        {
            if (mSelf.ownerType != AttackPattern.OwnerType.PLAYER)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isShootPlayer"));
                if (mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isFollowPlayer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                }
                if (!mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isDirectionFromOwner"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shootDirection"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialSpacing"));

            if (mSelf.viewAngle < 0) mSelf.viewAngle = 0; if (mSelf.viewAngle > 360) mSelf.viewAngle = 360;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("viewAngle"));

            if (mSelf.segments < 1) mSelf.segments = 1;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("segments"));
        }
        else if (mSelf.template == AttackPattern.Template.SHOOT_AROUND_IN_CIRCLE)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isClockwise"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("distance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("segments"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startTurnDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("turningRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("increaseTR"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("increaseTRTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxTR"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yOffset"));
        }
        else if (mSelf.template == AttackPattern.Template.DOUBLE_SINE_WAVE)
        {
            if (mSelf.ownerType != AttackPattern.OwnerType.PLAYER)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isShootPlayer"));
                if (mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isFollowPlayer"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
                }
                if (!mSelf.isShootPlayer)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isDirectionFromOwner"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shootDirection"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("offsetPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frequency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magnitude"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("magExpandMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sineWaveBullets"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"));
        }
        else if (mSelf.template == AttackPattern.Template.SHOCK_REPEL_AND_TRAP_LASER)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("giveStatRepelDur"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slowValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("repelValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapDelayExpand"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapExpandDur"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapExpandSpd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapRotateSpd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapMoveSpd"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trapDelayShrink"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("trapShrinkSpd"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("trapFadeSpd"));
        }

        if (mSelf.ownerType != AttackPattern.OwnerType.PLAYER)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speedChangeList"), true);
            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Enemy Stat");
            GUILayout.EndVertical ();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isShowDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onceStartDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isPotraitShow"));

            if (mSelf.isPotraitShow)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("charSprite"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spellCardSprite"));
            }
        }

        serializedObject.ApplyModifiedProperties();
		if (GUI.changed) EditorUtility.SetDirty(target); 
	}
}