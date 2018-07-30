using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickManager : MonoBehaviour 
{
	public static JoystickManager sSingleton { get { return _sSingleton; } }
	static JoystickManager _sSingleton;

	public class JoystickInput
	{
        public KeyCode acceptKey, backKey, returnDefault, fireKey, bombKey, reviveKey, slowMoveKey, startKey, skipConvoKey, languageChange;

        public JoystickInput(KeyCode acceptKey, KeyCode backKey, KeyCode returnDefault, KeyCode fireKey, KeyCode bombKey, 
            KeyCode reviveKey, KeyCode slowMoveKey, KeyCode startKey, KeyCode skipConvoKey, KeyCode languageChange)
		{
			this.acceptKey = acceptKey;
            this.backKey = backKey; 
            this.returnDefault = returnDefault; 
			this.fireKey = fireKey;
			this.bombKey = bombKey;
			this.reviveKey = reviveKey;
			this.slowMoveKey = slowMoveKey;
            this.startKey = startKey;
            this.skipConvoKey = skipConvoKey;
            this.languageChange = languageChange;
		}
	}
	public JoystickInput p1_joystick;
	public JoystickInput p2_joystick;

	public enum GamePadButtons
	{
		SQUARE = 0,
		CROSS,
		CIRCLE,
		TRIANGLE,
		L1,
		R1,
		L2,
		R2,
		SHARE,
		OPTIONS,
		L3,
		R3,
		PADPRESS
	}

	[System.Serializable]
	public class GamePad
	{
		public GamePadButtons accept = GamePadButtons.CIRCLE;
        public GamePadButtons back = GamePadButtons.CROSS;
        public GamePadButtons returnDefault = GamePadButtons.TRIANGLE;
		public GamePadButtons start = GamePadButtons.OPTIONS;

		public GamePadButtons fire = GamePadButtons.SQUARE;
		public GamePadButtons bomb = GamePadButtons.CROSS;
		public GamePadButtons revive = GamePadButtons.R2;
		public GamePadButtons slowMove = GamePadButtons.L2;
		public GamePadButtons pause = GamePadButtons.OPTIONS;
        public GamePadButtons skipConvo = GamePadButtons.TRIANGLE;
        public GamePadButtons languageChange = GamePadButtons.SQUARE;
	}
	public GamePad controlA, controlB;

	[ReadOnlyAttribute]public int connectedGamepadCount = 0;

    bool mIsP1KeybInput = true;

	void Awake()
	{
        if (_sSingleton != null && _sSingleton != this) Destroy(this);
		else _sSingleton = this;
	}

	void Start()
	{
		int max = Input.GetJoystickNames().Length;
		for (int i = 0; i < max; i++)
		{
//            Debug.Log(Input.GetJoystickNames()[1]);
			// Check out with Input Manager....
			if (Input.GetJoystickNames()[i].Length == 19) connectedGamepadCount++;
		}

        if (JoystickManager.sSingleton.connectedGamepadCount > 0) mIsP1KeybInput = false;

		// Set up joystick 1 input.
		KeyCode acceptKey = GetJoystickButton (1, controlA.accept);
        KeyCode backKey = GetJoystickButton (1, controlA.back);
        KeyCode returnDefault = GetJoystickButton (1, controlA.returnDefault);
		KeyCode fireKey = GetJoystickButton (1, controlA.fire);
		KeyCode bombKey = GetJoystickButton (1, controlA.bomb);
		KeyCode reviveKey = GetJoystickButton (1, controlA.revive);
		KeyCode slowMoveKey = GetJoystickButton (1, controlA.slowMove);
		KeyCode pauseKey = GetJoystickButton (1, controlA.start);
        KeyCode skipConvoKey = GetJoystickButton (1, controlA.skipConvo);
        KeyCode languageKey = GetJoystickButton (1, controlA.languageChange);
        p1_joystick = new JoystickInput(acceptKey, backKey, returnDefault, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey, skipConvoKey, languageKey);

		// Set up joystick 2 input.
		acceptKey = GetJoystickButton (2, controlA.accept);
		backKey = GetJoystickButton (2, controlA.back);
        returnDefault = GetJoystickButton (2, controlA.returnDefault);
		fireKey = GetJoystickButton (2, controlA.fire);
		bombKey = GetJoystickButton (2, controlA.bomb);
		reviveKey = GetJoystickButton (2, controlA.revive);
		slowMoveKey = GetJoystickButton (2, controlA.slowMove);
		pauseKey = GetJoystickButton (2, controlA.start);
        skipConvoKey = GetJoystickButton (2, controlA.skipConvo);
        languageKey = GetJoystickButton (2, controlA.languageChange);
        p2_joystick = new JoystickInput(acceptKey, backKey, returnDefault, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey, skipConvoKey, languageKey);
	}

    public bool IsP1KeybInput { get { return mIsP1KeybInput; } }

    public void SaveControlLayout(int joystickNum, int controlType)
	{
        GamePad gamepad = null;
        if (controlType == 0) gamepad = controlA;
        else if (controlType == 1) gamepad = controlB;

        KeyCode acceptKey = GetJoystickButton (joystickNum, gamepad.accept);
        KeyCode backKey = GetJoystickButton (joystickNum, gamepad.back);
        KeyCode returnDefault = GetJoystickButton (joystickNum, gamepad.returnDefault);
        KeyCode fireKey = GetJoystickButton (joystickNum, gamepad.fire);
        KeyCode bombKey = GetJoystickButton (joystickNum, gamepad.bomb);
        KeyCode reviveKey = GetJoystickButton (joystickNum, gamepad.revive);
        KeyCode slowMoveKey = GetJoystickButton (joystickNum, gamepad.slowMove);
        KeyCode pauseKey = GetJoystickButton (joystickNum, gamepad.start);
        KeyCode skipConvoKey = GetJoystickButton (joystickNum, gamepad.skipConvo);
        KeyCode languageKey = GetJoystickButton (joystickNum, gamepad.languageChange);

        JoystickInput input = new JoystickInput(acceptKey, backKey, returnDefault, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey, skipConvoKey, languageKey);
        if (joystickNum == 1) p1_joystick = input;
        else if (joystickNum == 2) p2_joystick = input;
	}

	KeyCode GetJoystickButton(int joystickNum, GamePadButtons button)
	{
		KeyCode temp = KeyCode.None;
		if (joystickNum == 1) 
		{
			if (button == GamePadButtons.SQUARE) temp = KeyCode.Joystick1Button0;
			else if (button == GamePadButtons.CROSS) temp = KeyCode.Joystick1Button1;
			else if (button == GamePadButtons.CIRCLE) temp = KeyCode.Joystick1Button2;
			else if (button == GamePadButtons.TRIANGLE) temp = KeyCode.Joystick1Button3;
			else if (button == GamePadButtons.L1) temp = KeyCode.Joystick1Button4;
			else if (button == GamePadButtons.R1) temp = KeyCode.Joystick1Button5;
			else if (button == GamePadButtons.L2) temp = KeyCode.Joystick1Button6;
			else if (button == GamePadButtons.R2) temp = KeyCode.Joystick1Button7;
			else if (button == GamePadButtons.SHARE) temp = KeyCode.Joystick1Button8;
			else if (button == GamePadButtons.OPTIONS) temp = KeyCode.Joystick1Button9;
			else if (button == GamePadButtons.L3) temp = KeyCode.Joystick1Button10;
			else if (button == GamePadButtons.R3) temp = KeyCode.Joystick1Button11;
			else if (button == GamePadButtons.PADPRESS) temp = KeyCode.Joystick1Button13;
		}
		else if (joystickNum == 2) 
		{
			if (button == GamePadButtons.SQUARE) temp = KeyCode.Joystick2Button0;
			else if (button == GamePadButtons.CROSS) temp = KeyCode.Joystick2Button1;
			else if (button == GamePadButtons.CIRCLE) temp = KeyCode.Joystick2Button2;
			else if (button == GamePadButtons.TRIANGLE) temp = KeyCode.Joystick2Button3;
			else if (button == GamePadButtons.L1) temp = KeyCode.Joystick2Button4;
			else if (button == GamePadButtons.R1) temp = KeyCode.Joystick2Button5;
			else if (button == GamePadButtons.L2) temp = KeyCode.Joystick2Button6;
			else if (button == GamePadButtons.R2) temp = KeyCode.Joystick2Button7;
			else if (button == GamePadButtons.SHARE) temp = KeyCode.Joystick2Button8;
			else if (button == GamePadButtons.OPTIONS) temp = KeyCode.Joystick2Button9;
			else if (button == GamePadButtons.L3) temp = KeyCode.Joystick2Button10;
			else if (button == GamePadButtons.R3) temp = KeyCode.Joystick2Button11;
			else if (button == GamePadButtons.PADPRESS) temp = KeyCode.Joystick2Button13;
		}

		return temp;
	}
}
