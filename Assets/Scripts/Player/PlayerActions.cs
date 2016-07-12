using UnityEngine;
using InControl;
using System.Collections;

public class PlayerActions : PlayerActionSet
{
	public PlayerAction select;
	public PlayerAction sprint;
	public PlayerAction forward;
	public PlayerAction backwards;
	public PlayerAction left;
	public PlayerAction right;
	public PlayerAction shoot;
	public PlayerAction reloadWeapon;
	public PlayerAction rotateUp;
	public PlayerAction rotateDown;
	public PlayerAction rotateLeft;
	public PlayerAction rotateRight;
	public PlayerAction toggleWeaponMenu;
	public PlayerAction toggleHighscoreMenu;
	public PlayerAction nextWeapon;
	public PlayerAction previousWeapon;
	public PlayerAction pauseButton;
	public PlayerAction selectWeapon1;
	public PlayerAction selectWeapon2;
	public PlayerAction selectWeapon3;
	public PlayerAction selectWeapon4;
	public PlayerAction selectWeapon5;
	public PlayerAction selectWeapon6;
	public PlayerTwoAxisAction move;
	public PlayerTwoAxisAction rotation;

	public PlayerActions()
	{
		select = CreatePlayerAction("Select");
		sprint = CreatePlayerAction("Sprint");
		forward = CreatePlayerAction("Move forward");
		backwards = CreatePlayerAction("Move backwards");
		left = CreatePlayerAction("Move left");
		right = CreatePlayerAction("Move right");
		rotateUp = CreatePlayerAction("Look up");
		rotateDown = CreatePlayerAction("Look down");
		rotateLeft = CreatePlayerAction("Look left");
		rotateRight = CreatePlayerAction("Look right");
		shoot = CreatePlayerAction("Fire weapon");
		reloadWeapon = CreatePlayerAction("Reload weapon");
		toggleWeaponMenu = CreatePlayerAction("Open weapon switcher");
		toggleHighscoreMenu = CreatePlayerAction("Open highscore list");
		nextWeapon = CreatePlayerAction("Next weapon");
		previousWeapon = CreatePlayerAction("Previous weapon");
		pauseButton = CreatePlayerAction("Pause button");
		selectWeapon1 = CreatePlayerAction("Select weapon 1");
		selectWeapon2 = CreatePlayerAction("Select weapon 2");
		selectWeapon3 = CreatePlayerAction("Select weapon 3");
		selectWeapon4 = CreatePlayerAction("Select weapon 4");
		selectWeapon5 = CreatePlayerAction("Select weapon 5");
		selectWeapon6 = CreatePlayerAction("Select weapon 6");

		move = CreateTwoAxisPlayerAction(left, right, backwards, forward);
		rotation = CreateTwoAxisPlayerAction(rotateUp, rotateDown, rotateLeft, rotateRight);
	}

	public static PlayerActions CreateWithDefaultBindings()
	{
		var playerActions = new PlayerActions();

		playerActions.left.AddDefaultBinding(Key.A);
		playerActions.left.AddDefaultBinding(InputControlType.LeftStickLeft);

		playerActions.right.AddDefaultBinding(Key.D);
		playerActions.right.AddDefaultBinding(InputControlType.LeftStickRight);

		playerActions.backwards.AddDefaultBinding(Key.S);
		playerActions.backwards.AddDefaultBinding(InputControlType.LeftStickDown);

		playerActions.forward.AddDefaultBinding(Key.W);
		playerActions.forward.AddDefaultBinding(InputControlType.LeftStickUp);

		playerActions.rotateLeft.AddDefaultBinding(Mouse.NegativeX);
		playerActions.rotateLeft.AddDefaultBinding(InputControlType.RightStickLeft);

		playerActions.rotateRight.AddDefaultBinding(Mouse.PositiveX);
		playerActions.rotateRight.AddDefaultBinding(InputControlType.RightStickRight);

		playerActions.rotateUp.AddDefaultBinding(Mouse.PositiveY);
		playerActions.rotateUp.AddDefaultBinding(InputControlType.RightStickUp);

		playerActions.rotateDown.AddDefaultBinding(Mouse.NegativeY);
		playerActions.rotateDown.AddDefaultBinding(InputControlType.RightStickDown);

		playerActions.select.AddDefaultBinding(Key.E);
		playerActions.select.AddDefaultBinding(InputControlType.Action1);//Cross on PS, A on Xbox controller

		playerActions.sprint.AddDefaultBinding(Key.LeftShift);
		playerActions.sprint.AddDefaultBinding(InputControlType.LeftStickButton);

		playerActions.shoot.AddDefaultBinding(Mouse.LeftButton);
		playerActions.shoot.AddDefaultBinding(InputControlType.RightBumper);

		playerActions.reloadWeapon.AddDefaultBinding(Key.R);
		playerActions.reloadWeapon.AddDefaultBinding(InputControlType.Action3);//Square on PS, X on Xbox controller

		playerActions.toggleWeaponMenu.AddDefaultBinding(Key.Tab);
		playerActions.toggleWeaponMenu.AddDefaultBinding(InputControlType.Action4); //Triangle on PS, Y on Xbox controller

		playerActions.toggleHighscoreMenu.AddDefaultBinding(Key.H);
		playerActions.toggleHighscoreMenu.AddDefaultBinding(InputControlType.Select);

		playerActions.nextWeapon.AddDefaultBinding(Mouse.PositiveScrollWheel);
		playerActions.nextWeapon.AddDefaultBinding(InputControlType.RightTrigger);

		playerActions.previousWeapon.AddDefaultBinding(Mouse.NegativeScrollWheel);
		playerActions.previousWeapon.AddDefaultBinding(InputControlType.LeftTrigger);

		playerActions.pauseButton.AddDefaultBinding(Key.Escape);
		playerActions.pauseButton.AddDefaultBinding(InputControlType.Start);

		playerActions.selectWeapon1.AddDefaultBinding(Key.Key1);
		playerActions.selectWeapon2.AddDefaultBinding(Key.Key2);
		playerActions.selectWeapon3.AddDefaultBinding(Key.Key3);
		playerActions.selectWeapon4.AddDefaultBinding(Key.Key4);
		playerActions.selectWeapon5.AddDefaultBinding(Key.Key5);
		playerActions.selectWeapon6.AddDefaultBinding(Key.Key6);

		return playerActions;
	}
}
