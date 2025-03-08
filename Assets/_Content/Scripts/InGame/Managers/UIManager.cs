using _Content.InGame.UI.Main;
using _Content.InGame.UI.Misc;
using Base;
using Common.UI;
using UnityEngine;

namespace _Content.InGame.Managers
{
	public class UIManager: Singleton<UIManager>
	{
		[SerializeField] private Transform _coinParticleDestination;
		[SerializeField] private NewLevelAbilitiesUi _newLevelAbilitiesUi;
		[SerializeField] private CharacterProgressUi _characterProgressUi;
		[SerializeField] private MovementTutorialUi _movementTutorialUi;
		[SerializeField] private CurrentSkillsUi _currentSkillsUi;
		[SerializeField] private LevelProgressUi _levelProgressUi;
		[SerializeField] private SettingsUi _settingsUi;
		[SerializeField] private SettingsButton _settingsButton;
		[SerializeField] private UIViewWrapper _joystick;
		[SerializeField] private UIViewWrapper _loadingUi;
		[SerializeField] private MainMenu _mainMenu;
		[SerializeField] private WorldProgress _worldProgress;
		[SerializeField] private RewardUi _rewardUi;
		[SerializeField] private UnlockSkillUi _unlockSkillUi;
		[SerializeField] private CheatsUi _cheatsUi;
		[SerializeField] private DefeatUi _defeatUi;
		[SerializeField] private NotificationUi _notificationUi;

		public NotificationUi NotificationUi => _notificationUi;
		public CheatsUi CheatsUi => _cheatsUi;
		public UnlockSkillUi UnlockSkillUi => _unlockSkillUi;
		public RewardUi RewardUi => _rewardUi;
		public WorldProgress WorldProgress => _worldProgress;
		public SettingsButton SettingsButton => _settingsButton;
		public UIViewWrapper Joystick => _joystick;
		public SettingsUi SettingsUi => _settingsUi;
		public LevelProgressUi LevelProgressUi => _levelProgressUi;
		public CurrentSkillsUi CurrentSkillsUi => _currentSkillsUi;
		public MovementTutorialUi MovementTutorialUi => _movementTutorialUi;
		public CharacterProgressUi CharacterProgressUi => _characterProgressUi;
		public NewLevelAbilitiesUi NewLevelAbilitiesUi => _newLevelAbilitiesUi;
		public UIViewWrapper LoadingUi => _loadingUi;
		public MainMenu MainMenu => _mainMenu;
		public Transform CoinParticleDestination => _coinParticleDestination;
		public DefeatUi DefeatUi => _defeatUi;
	}
}