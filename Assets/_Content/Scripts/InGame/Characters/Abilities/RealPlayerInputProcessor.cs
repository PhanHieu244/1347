namespace _Content.InGame.Characters.Abilities
{
	public class RealPlayerInputProcessor : CharacterAbility
	{
		private Joystick _joystick;
		private CharacterMovement _characterMovement;

		protected override void Initialization()
		{
			base.Initialization();
			_joystick = FindObjectOfType<Joystick>();
			_characterMovement = _character.FindAbility<CharacterMovement>();
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (_joystick == null)
				return;

			_characterMovement.SetInput(_joystick.Direction);
		}
	}
}