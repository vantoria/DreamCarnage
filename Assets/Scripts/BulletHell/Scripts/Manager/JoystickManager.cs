using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickManager : MonoBehaviour 
{
	public static JoystickManager sSingleton { get { return _sSingleton; } }
	static JoystickManager _sSingleton;

	public class JoystickInput
	{
		public KeyCode acceptKey, backKey, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey;

		public JoystickInput(KeyCode acceptKey, KeyCode backKey, KeyCode fireKey, KeyCode bombKey, 
			KeyCode reviveKey, KeyCode slowMoveKey, KeyCode pauseKey)
		{
			this.acceptKey = acceptKey;
			this.backKey = backKey; 
			this.fireKey = fireKey;
			this.bombKey = bombKey;
			this.reviveKey = reviveKey;
			this.slowMoveKey = slowMoveKey;
			this.pauseKey = pauseKey;
		}
	}
	public JoystickInput p1_joystick;
	public JoystickInput p2_joystick;

	enum GamePadButtons
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
	[SerializeField] GamePadButtons accept = GamePadButtons.CIRCLE;
	[SerializeField] GamePadButtons back = GamePadButtons.CROSS;
	[SerializeField] GamePadButtons start = GamePadButtons.OPTIONS;

	[SerializeField] GamePadButtons fire = GamePadButtons.SQUARE;
	[SerializeField] GamePadButtons bomb = GamePadButtons.CROSS;
	[SerializeField] GamePadButtons revive = GamePadButtons.CIRCLE;
	[SerializeField] GamePadButtons slowMove = GamePadButtons.L2;
	[SerializeField] GamePadButtons pause = GamePadButtons.OPTIONS;

	[ReadOnlyAttribute]public int connectedGamepadCount = 0;

	void Awake()
	{
		if (_sSingleton != null && _sSingleton != this) Destroy(this.gameObject);
		else _sSingleton = this;
	}

	void Start()
	{
		int max = Input.GetJoystickNames().Length;
		for (int i = 0; i < max; i++)
		{
			// Check out with Input Manager....
			if (Input.GetJoystickNames()[i].Length == 19) connectedGamepadCount++;
		}

		// Set up joystick 1 input.
		KeyCode acceptKey = GetJoystickButton (1, accept);
		KeyCode backKey = GetJoystickButton (1, back);
		KeyCode fireKey = GetJoystickButton (1, fire);
		KeyCode bombKey = GetJoystickButton (1, bomb);
		KeyCode reviveKey = GetJoystickButton (1, revive);
		KeyCode slowMoveKey = GetJoystickButton (1, slowMove);
		KeyCode pauseKey = GetJoystickButton (1, pause);
		p1_joystick = new JoystickInput(acceptKey, backKey, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey);

		// Set up joystick 2 input.
		acceptKey = GetJoystickButton (2, accept);
		backKey = GetJoystickButton (2, back);
		fireKey = GetJoystickButton (2, fire);
		bombKey = GetJoystickButton (2, bomb);
		reviveKey = GetJoystickButton (2, revive);
		slowMoveKey = GetJoystickButton (2, slowMove);
		pauseKey = GetJoystickButton (2, pause);
		p2_joystick = new JoystickInput(acceptKey, backKey, fireKey, bombKey, reviveKey, slowMoveKey, pauseKey);
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
