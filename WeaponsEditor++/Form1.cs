using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using JWC; //MRU List

namespace WeaponsEditor__
{
    public partial class Form1 : Form
    {
        Timer clearConsoleTimer = new Timer();
        bool multiline_hideTags = false;
        ToolTip toolTip1 = new ToolTip();
        string fileName1;
        Dictionary<string, string> sourceDict;
        bool fileReadingComplete = false;
        List<Control> savedControlsState = new List<Control>(); //save which controls are disabled in case the user decides he wants to turn off advanced mode.
        List<string> miscBO1Settings = new List<string> { "DualWieldWeapon", "doGibbing", "maxGibDistance", "DualMag", "ammoCountClipRelative", "continuousFire", "fullMetalJacket", "hollowPoint", "rapidFirE", "isCarriedKillstreakWeapon", "useAltTagFlash", "jamFireTime", "lowReadyInTime", "lowReadyLoopTime", "lowReadyOutTime", "contFireInTime", "contFireLoopTime", "contFireOutTime", "dtpInTime", "slideInTime", "dtpLoopTime", "dtpOutTime", "adsZoomSound", "lowReadyInAnim", "lowReadyLoopAnim", "lowReadyOutAnim", "contFireInAnim", "contFireLoopAnim", "contFireOutAnim", "dtp_in", "dtp_loop", "dtp_out", "dtp_empty_in", "dtp_empty_loop", "dtp_empty_out", "slide_in", "sprintCameraAnim", "dtpInCameraAnim", "dtpLoopCameraAnim", "dtpOutCameraAnim", "mantleCameraAnim", "useOnlyAltWeaoponHideTagsInAltMode", "dualWield", "reloadAnimRight", "reloadQuickAnim", "sprintInEmptyAnim", "sprintLoopEmptyAnim", "sprintOutEmptyAnim", "mantleAnim", "meleeChargeRange", "useAsMelee", "AdditionalMeleeModel", "lastFireTime", "reloadQuickTime", "reloadQuickAddTime", "isCameraSensor", "isAcousticSensor", "adsZoomFov1", "adsZoomFov2", "adsZoomFov3", "adsOverlayAlphaScale", "grenadeWeapon", "reloadWhileAds", "noThirdPersonDropsOrRaises", "lowReadyOfsF", "lowReadyOfsR", "lowReadyOfsU", "lowReadyRotP", "lowReadyRotY", "lowReadyRotR", "dtpOfsF", "dtpOfsR", "dtpOfsU", "dtpRotP", "dtpRotY", "dtpRotR", "dtpBobH", "dtpBobV", "dtpScale", "mantleOfsF", "mantleOfsR", "mantleOfsU", "mantleRotP", "mantleRotY", "mantleRotR", "slideOfsF", "slideOfsR", "slideOfsU", "slideRotP", "slideRotY", "slideRotR", "strafeMoveF", "strafeMoveR", "strafeMoveU", "strafeRotP", "strafeRotY", "strafeRotR", "indicatorRadius", "projectileSpeedRelativeUp", "showIndicator", "noPing", "lockOnRadius", "lockOnSpeed", "reloadRumble", "stackFire", "stackFireSpread", "stackFireAccuracyDecay", "stackSound", "ikLeftHandOffsetF", "ikLeftHandOffsetR", "ikLeftHandOffsetU", "ikLeftHandRotationP", "ikLeftHandRotationY", "ikLeftHandRotationR", "ikLeftHandProneOffsetF", "ikLeftHandProneOffsetR", "ikLeftHandProneOffsetU", "ikLeftHandProneRotationP", "ikLeftHandProneRotationY", "ikLeftHandProneRotationR", "ikLeftHandUiViewerOffsetF", "ikLeftHandUiViewerOffsetR", "ikLeftHandUiViewerOffsetU", "ikLeftHandUiViewerRotationP", "ikLeftHandUiViewerRotationY", "ikLeftHandUiViewerRotationR", "retrievable", "dieOnRespawn", "noCrumpleMissile", "forceBounce", "useDroppedModelAsStowed", "noQuickDropWhenEmpty", "keepCrosshairWhenADS", "useAntiLagRewind", "parentWeaponName" };

        bool anyUnsavedChanges = false;
        string anyUnSavedChanges_text = "";

        protected MruStripMenu mruMenu;
        static string mruRegKey = "SOFTWARE\\UGX\\WeaponsEditor";

        static BackgroundWorker _bw = new BackgroundWorker();

        public Form1(string[] args)
        {
            InitializeComponent();
            this.Text = "UGX WeaponsEditor++ v" + Application.ProductVersion.Substring(0, 5);
            Version nonBeta = new Version(2, 0, 0, 0);
            if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version < nonBeta)
                this.Text += " BETA";

            TextBox.CheckForIllegalCrossThreadCalls = false;

            mruMenu = new MruStripMenuInline(menuItem1, menuRecentFile, new MruStripMenu.ClickedHandler(OnMruFile), mruRegKey + "\\MRU", 16);

            _bw.DoWork += checkForUpdates;
            _bw.RunWorkerAsync();

            clearConsoleTimer.Tick += new EventHandler(clearConsoleTimer_Tick);
            clearConsoleTimer.Interval = 15000;
            this.AllowDrop = true;
            this.DragEnter +=  new DragEventHandler(dragEnter);
            this.DragDrop +=  new DragEventHandler(dragDrop);
            searchBox.KeyDown += new KeyEventHandler(searchBox_keyDown);
            toolTip1.InitialDelay = 150;
            toolTip1.ToolTipIcon = ToolTipIcon.Info;
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(advancedMode, "Enable all weaponSettings for current file. \nThe WaW engine sometimes allows unusual settings from one weaponClass to be used on another, even if not shown in Asset Manager. \nThis also allows you to convert a weapon to a different weaponClass because you can add the needed additional settings.");
            toolTip1.SetToolTip(convertCod5, "If you have opened a BO1 weaponFile, press this button to remove some BO1-only settings from the currently open file. It also renames some of the edited BO1 names to their CoD4/5 versions (ex. adsZoomFov1 to adsZoomFov). \nThis will usually make the weaponFile work in CoD5 without any further action.");
            toolTip1.SetToolTip(searchBox, "Jump to a specific setting by typing at least the first new characters of its name. Use your arrowkeys to select from the list if necessary, then hit Enter to jump to the setting.");

//Tooltip Definitions
#region General
            createTip("Display Name", "Localization alias for weapon name displayed on HUD in game.");
            createTip("modeName", "Localization alias for selective firing mode text to be displayed on HUD in-game.");
            createTip("Player Anim Type", "Select an Player Anim Type - specifies \"playerAnimType\" in playeranim.script");
            createTip("Alt Weapon", "Weapon to switch to when this weapon's selective fire mode is switched in the game.");
            createTip("AIOverlayDescription", "Localization alias for text shown when crosshair is placed over a friendly. Eg. Rifleman, Submachine Gunner, etc.");
            createTip("InventoryType", "Select what sort of inventory this weapon is.");
            createTip("WeaponType", "Select a weapon type.");
            createTip("weaponClass", "Select an appropriate class for this weapon.");
            createTip("penetrateType", "Type of bullet penetration.");
            createTip("Impact Type", "The impact type, used to play impact effects based on surfacetype");
            createTip("Fire Type", "Behavior of the weapon when the trigger is held down.");
            createTip("Clip Type", "Determines how the clip gets inserted into the weapon.");
            createTip("Rifle Bullet", "Uses pistol bullets if not checked. Rifle bullets apply damage to the highest priority hit location (locationdamage.gdt) along the bullet's path, and will go through people. Pistol bullets don't.");
            createTip("Armor Piercing", "Does damage to armored targets if checked.");
            createTip("Bolt Action", "Turn this on for bolt-action weapons only. Animation control.");
            createTip("aimdownsight", "Must be turned on for proper viewmodel appearance of a weapon that can be Aimed Down the Sight (ADS).");
            createTip("ADS Fire", "Can only be fired ADS.  Firing from the hip brings up ADS.");
            createTip("rechamberWhileADS", "Weapon can be rechambered while in ADS.");
            createTip("No ADS Auto-Reload", "Disallow auto-reloading while the weapon is in ADS.");
            createTip("noAdsWhenMagEmpty", "Disallow ADS when magazine is empty.");
            createTip("avoidDropCleanup", "Avoid having dropped weapons of this type deleted to make room for new ones.");
            createTip("No Partial Reload", "When noPartialReload is set for a weapon, it can not be reloaded unless reloadAmmoAdd amount of ammo can be put into the gun. If reloadAmmoAdd is 0, it's treated as the weapon's clip size.");
            createTip("Segmented Reload", "Turn on for weapons that reload X rounds at a time (Lee-Enfield, bolt-action sniper rifles), set reload amounts in Reload Ammo Add and Reload Start Add.");
            createTip("Enhanced", "This weapon will be an upgraded version of the ones with the same ammo type.");
            createTip("Bayonet", "This weapon is equipped with a bayonet, which will be used for melee attacks.");
            createTip("blocksProne", "The player cannot go prone when they have this weapon equipped.");
            createTip("Silenced", "This weapon is considered silenced.");
            createTip("laserSightDuringNightvision", "When using nightvision, this weapon will emit an infrared laser.");
            createTip("mountableWeapon", "This weapon can be mounted on mount brushes");
            createTip("Deploy Time", "The time it takes to deploy this weapon");
            createTip("Breakdown Time", "The time it takes to breakdown this weapon if it is deployed");
            createTip("standMountedWeapdef", "This is the weapondef in turretsettings.gdt that will be used if the weapon is mounted while standing");
            createTip("Mounted Model", "This is the model that will be used if the weapon is mounted");
            createTip("crouchMountedWeapdef", "This is the weapondef in turretsettings.gdt that will be used if the weapon is mounted while crouching");
            createTip("proneMountedWeapdef", "This is the weapondef in turretsettings.gdt that will be used if the weapon is mounted while prone");
            createTip("Enemy Crosshair Range", "The range in at which friendly names appear and friendly or enemy changes your crosshair color.");
            createTip("Crosshair Color Change", "Change crosshair color if pointing at friendly or enemy");
            createTip("Move Speed Scale", "When using this weapon, player movement speed is multiplied by this amount.");
            createTip("ADS Move Speed Scale", "When using this weapon and in ADS, player movement speed is multiplied by this amount.");
            createTip("Sprint Duration Scale", "When sprinting with this weapon, sprint duration is multiplied by this amount.");
            createTip("lowAmmoWarningThreshold", "The game optionally displays low-ammo warnings when remaining clip ammo goes below this percentage.");
            createTip("Gun Max Pitch", "Maximum allowed vertical ascent of the viewmodel due to recoil (degrees).");
            createTip("Gun Max Yaw", "Maximum allowed horizontal travel of the viewmodel due to recoil (degrees).");
            createTip("Ammo Name", "Allows different weapons to share the same ammo pool.");
            createTip("Clip Name", "Allows different weapons to share clips. Used for weapons that have a selective fire option, and would therefore need to use the same type of clip.");
            createTip("Max Ammo", "Max ammo the player can collect for this weapon. No effect on AI.");
            createTip("Start Ammo", "How much ammo the player gets when starting with this weapon. One clip/magazine from this amount will be already in the weapon. No effect on AI.");
            createTip("Clip Size", "Specifies how many bullets per clip/magazine.");
            createTip("Dropammo Min", "When dropped by AI/player/hand-placed in editor, contains at least this much ammo. Not limited to real-life clip/magazine size.");
            createTip("dropammo Max", "When dropped by AI/player/hand-placed in editor, contains no more than this much ammo. Not limited to real-life clip/magazine size.");
            createTip("Shot Count", "Specifies how many chunks per shotgun blast.");
            createTip("Reload Ammo Add", "For weapons with Segmented Reload turned on. Amount to add when reloading with some amount of bullets still remaining in the weapon.");
            createTip("Reload Start Add", "For weapons with Segmented Reload turned on. Amount to add for the first reload segment (ie: when weapon is empty.)");
            createTip("cancelAutoHolsterWhenEmpty", "When weapons are empty, they are normally auto-swapped to the next usable weapon in the player's inventory.  This disables that.");
            createTip("damage", "Damage per-bullet, applied up to Max Dmg Range. Damage falls off linearly from Max Dmg Range until reaching Min Dmg at Min Dmg Range.");
            createTip("minDamage", "Damage per-bullet, applied beyond Min Dmg Range.");
            createTip("minDamageRange", "Range in world units, beyond which the minimum damage is applied. (1 world unit = 1 inch)");
            createTip("maxDamageRange", "Range in world units, up to which the maximum damage is applied. (1 world unit = 1 inch)");
            createTip("meleeDamage", "Damage per melee hit.");
            createTip("locNone", "Unrelated to weapon. Used for damage that's not location based, such as grenades or falling.  Included here for completeness.");
            createTip("locHelmet", "Damage per-bullet multiplier.");
            createTip("locHead", "Damage per-bullet multiplier.");
            createTip("locNeck", "Damage per-bullet multiplier.");
            createTip("locTorsoUpper", "Damage per-bullet multiplier.");
            createTip("locTorsoLower ", "Damage per-bullet multiplier.");
            createTip("locLeftArmUpper", "Damage per-bullet multiplier.");
            createTip("locRightArmUpper", "Damage per-bullet multiplier.");
            createTip("locLeftArmLower", "Damage per-bullet multiplier.");
            createTip("locRightArmLower", "Damage per-bullet multiplier.");
            createTip("locLeftHand", "Damage per-bullet multiplier.");
            createTip("locRightHand", "Damage per-bullet multiplier.");
            createTip("locLeftLegUpper", "Damage per-bullet multiplier.");
            createTip("locRightLegUpper ", "Damage per-bullet multiplier.");
            createTip("locLeftLegLower ", "Damage per-bullet multiplier.");
            createTip("locRightLegLower ", "Damage per-bullet multiplier.");
            createTip("locLeftFoot", "Damage per-bullet multiplier.");
            createTip("locRightFoot", "Damage per-bullet multiplier.");
            createTip("locGun", "Damage per-bullet multiplier.");
            createTip("viewFlashEffect", "The muzzleflash fx that the player sees on the barrel in first-person view.");
            createTip("worldFlashEffect", "The muzzleflash fx that everyone else sees on the barrel in third-person view.");
            createTip("viewShellEjectEffect", "The shell ejection fx that the player sees from tag_brass");
            createTip("worldShellEjectEffect", "The shell ejection fx that everyone else sees from tag_brass in third-person view.");
            createTip("viewLastShotEjectEffect", "If for some reason you want a different shell ejection fx to play for the last shot of a clip, use this setting. Leaving this BLANK means the engine will use the value of viewShellEjectEffect!");
            createTip("worldLastShotEjectEffect", "If for some reason you want a different shell ejection fx to play for the last shot of a clip, use this setting. Leaving this BLANK means the engine will use the value of worldShellEjectEffect!");
            createTip("worldClipDropEffect", "Left over from CoD4. Does not do anything.");
            createTip("worldClipModel", "Left over from CoD4. Does not do anything.");
            createTip("gunModel", "Xmodel rendered in first-person view.");
            createTip("worldModel", "Xmodel rendered in third-person view.");
            createTip("handModel", "");
            createTip("knifeModel", "Xmodel used for melee animations in first-person.");
            createTip("worldKnifeModel", "Xmodel used for melee animations in third-person.");
            createTip("hideTags", "List of joint names to hide on the viewmodel in first-person. ONE PER LINE.");
#endregion
#region Type-specific
            createTip("flameTableFirstPerson", "The flametable to use to generate a flame in first-person. Flametables are data that define how a flame should be generated by the engine. See raw/flameconfigs");
            createTip("flameTableThirdPerson", "The flametable to use to generate a flame in third-person. See flameTableFirstPerson for more info.");
            createTip("handModel", "Override the player's current viewmodel with the specified xmodel. Use viewmodel_hands_no_model to use the default hands set by _loadout.gsc");
#endregion
#region State Timers
            createTip("fireTime", "Rate of fire in seconds per round. Maximum possible rate is 0.05 seconds per round, or 1200 rounds per minute.");
            createTip("Fire Delay", "Delay in seconds between pressing the fire button and the weapon actually firing.");
            createTip("MeleeTime", "Rate of fire in seconds per melee attack.");
            createTip("Melee Delay", "Delay in seconds between pressing the fire button and the melee attack actually happening.");
            createTip("Melee Charge Time", "Rate of fire in seconds per melee charge attack.");
            createTip("Melee Charge Delay", "Delay in seconds between pressing the fire button and the melee charge attack actually happening.");
            createTip("Reload Time", "The number of seconds over which the non-empty reload animation will be played.  In segmented reload weapons, this is the animation that loops to give the player ammo.");
            createTip("Reload Empty Time", "The number of seconds over which the empty reload animation will be played.");
            createTip("Reload Empty Add Time", "During an empty reload, when the gun will literally get more ammo (ammo counter fills up). Uses Reload Add if set to zero.");
            createTip("Reload Start Time", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Reload End Time", "For a segmented reload weapon, the number of seconds over which the reload end animation will be played.");
            createTip("Reload Add Time", "During an partial reload, when the gun will literally get more ammo (ammo counter fills up).");
            createTip("Reload Start Add Time", "Animations will get scaled to match this time.");
            createTip("RechamberTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("rechamberBoltTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("DropTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("RaiseTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("First RaiseTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Alt DropTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Alt RaiseTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Quick DropTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Quick RaiseTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Empty DropTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Empty RaiseTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Sprint InTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Sprint LoopTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("Sprint OutTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("nightVisionWearTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("nightVisionWearTimeFadeOutEnd", "At this point in 'Nightvision Wear', player's vision has faded to black as they bring their goggles down.");
            createTip("nightVisionWearTimePowerUp", "At this point in 'Nightvision Wear', the player's goggles start their power up.");
            createTip("nightVisionRemoveTime", "How long this weapon state will last. The viewmodel animations will get scaled to match the times set.");
            createTip("nightVisionRemoveTimePowerDown", "At this point in 'Nightvision Remove', the player's goggles cut to black.");
            createTip("nightVisionRemoveTimeFadeInStart", "At this point in 'Nightvision Remove', the player's vision fades from black to normal as they remove their goggles.");
#endregion
#region Movement
            //StanceK is a keyword, will be replaced with stand, sprint, ducked, and prone
            createTip("stancekOfsF", "When the player changes to this stance, the viewmodel will slide forward by this amount. (-ive for backward)");
            createTip("stancekOfsR", "When the player changes to this stance, the viewmodel will translate horizontally by this amount. (+/- for left/right respectively)");
            createTip("stancekOfsU", "When the player changes to this stance, the viewmodel will translate vertically by this amount. (+/- for up/down respectively)");
            createTip("stancekRotP", "Viewmodel will pitch by this amount if the player is moving in this stance. (+/- for down/up respectively)");
            createTip("stancekRotY", "Viewmodel will yaw by this amount if the player is moving in this stance. (+/- for left/right respectively)");
            createTip("stancekRotR", "Viewmodel will roll by this amount if the player is moving in this stance. (+/- for left/right respectively)");
            createTip("stancekMoveF", "Viewmodel will translate forward/backward by this amount if the player is moving in this stance. (+/- for forward/backward respectively)");
            createTip("stancekMoveR", "Viewmodel will translate horizontally by this amount if the player is moving in this stance. (+/- for left/right respectively)");
            createTip("stancekMoveU", "Viewmodel will translate vertically by this amount if the player is moving in this stance. (+/- for up/down respectively)");
            createTip("standMoveMinSpeed", "Not used apparently.");
            createTip("standRotMinSpeed", "Not used apparently.");
            createTip("proneMoveMinSpeed", "Not used apparently.");
            createTip("proneRotMinSpeed", "Not used apparently.");
            createTip("duckedMoveMinSpeed", "Not used apparently.");
            createTip("duckedRotMinSpeed", "Not used apparently.");
            createTip("sprintBobH", "A multiplier applied to the standard horizontal bob for weapons when sprinting domain 0-10");
            createTip("sprintBobV", "A multiplier applied to the standard vertical bob for weapons when sprinting domain 0-10");
            createTip("sprintScale", "A multiplier applied to xy movement of the viewmodel during sprint higher is faster");
            createTip("Pos Move Rate", "Controls speed of viewmodel translation while moving in stand and crouch, transitions between stand and crouch, and crouch into prone.");
            createTip("Pos Prone Move Rate", "Controls speed of viewmodel translation while moving in stand and crouch, transitions between stand and crouch, and crouch into prone.");
            createTip("Pos Rot Rate", "Controls the speed of viewmodel rotation while moving in this stance.");
            createTip("Pos Prone Rot Rate", "Controls the speed of viewmodel rotation while moving in this stance.");
            createTip("HipIdleAmount", "Strength of viewmodel wavering motion when in hipfire position.");
            createTip("ADSIdleAmount", "Strength of range of viewmodel (or sniper rifle overlay) wavering motion when in ADS.");
            createTip("HipIdleSpeed", "How fast the viewmodel wavers in hipfire position within Hip Amount.");
            createTip("ADSIdleSpeed", "How fast the viewmodel wavers in ADS within ADS Amount.");
            createTip("IdleCrouchFactor", "Hip/ADS Amount multiplier for reducing viewmodel wavering when in this stance.");
            createTip("IdleProneFactor", "Hip/ADS Amount multiplier for reducing viewmodel wavering when in this stance.");
            createTip("adsSpread", "Size of bullet spread cone when firing in ADS mode. Bullets will project randomly within the confines of the cone.");
            createTip("adsAim Pitch", "Adjusts the pitch of the viewmodel in ADS. Defaults to 0, -6 is perfectly centered.");
            createTip("adsTrans In Time", "Time it will take to switch from hipfire to ADS.");
            createTip("adsTrans Out Time", "Time it will take to switch from ADS to hipfire.");
            createTip("adsReload Trans Time", "Time it takes once you start the reload to re-enter ADS. (ie: It allows you to finish up reloading while you enter ADS.");
            createTip("adsCrosshair In Frac", "Fraction of the hipfire-to-ADS transition time over which the crosshair disappears.");
            createTip("adsCrosshair Out Frac", "Fraction of the ADS-to-hipfire transition time over which the crosshair appears.");
            createTip("adsZoomFOV", "Field of view when in ADS.");
            createTip("adsZoomIn Frac", "Fraction of the hipfire-to-ADS transition time over which the FOV zoom-in effect happens.");
            createTip("adsZoomOut Frac", "Fraction of the ADS-to-hipfire transition time over which the FOV zoom-out effect happens.");
            createTip("adsBobFactor", "Strength of viewmodel bobbing due to player movement when using this weapon.");
            createTip("adsViewBob Mult", "Bob Factor multiplier for view bobbing due to player movement when using this weapon in ADS.");
            createTip("adsViewErrorMin ", "Min View Error.");
            createTip("adsViewErrorMax ", "Max View Error.");
#endregion
#region Spread
            createTip("hipSpreadStand Min", "Smallest diameter the crosshairs can contract to.");
            createTip("hipSpreadMax", "Largest diameter the crosshairs can expand to while standing.");
            createTip("hipSpreadDecay Rate", "Rate at which the crosshairs try to return to the Min hip spread size.");
            createTip("hipSpreadDucked Min", "Size of the crosshairs at rest when crouched.");
            createTip("hipSpreadDucked Max", "Largest diameter the crosshairs can expand to while crouched.");
            createTip("hipSpreadDucked Decay", "Multiplier of Decay Rate for crouched stance, controlling how fast the crosshairs return to Crouch Min.");
            createTip("hipSpreadProne Min", "Size of the crosshairs at rest when prone.");
            createTip("hipSpreadProne Max", "Largest diameter the crosshairs can expand to while prone.");
            createTip("hipSpreadProne Decay", "Multiplier of Decay Rate for prone stance, controlling how fast the crosshairs return to Prone Min.");
            createTip("hipSpreadFire Add", "Amount by which the crosshairs expand per bullet fired.");
            createTip("hipSpreadMove Add", "Rate of crosshair expansion due to player movement.");
            createTip("hipSpreadTurn Add", "Rate of crosshair expansion due to player panning the view in any direction.");
#endregion
#region Kick
            createTip("hipGunKickReducedKickBullets", "Hipfire viewmodel control reduced kick number of bullets. This is the number of bullets before the weapon uses a full kick amount.");
            createTip("adsGunKickReducedKickBullets", "ADS viewmodel control reduced kick number of bullets. This is the number of bullets before the weapon uses a full kick amount.");
            createTip("hipGunKickReducedKickPercent", "Hip viewmodel control reduced kick percentage. This is the percentage of the full kick amount to kick the gun for the first few bullets.");
            createTip("adsGunKickReducedKickPercent", "ADS viewmodel control reduced kick percentage. This is the percentage of the full kick amount to kick the gun for the first few bullets.");
            createTip("Hip Gun Kick Pitch Min", "Hipfire viewmodel control. +/- sign means 'kick down/up'. Larger absolute numbers increase viewmodel climb/descent. From -100 to 100. Actual viewmodel angle will not exceed Max Gun Pitch.");
            createTip("ADS Gun Kick Pitch Min", "ADS viewmodel control. +/- sign means 'kick down/up'. Larger absolute numbers increase viewmodel climb/descent. From -100 to 100.");
            createTip("Hip Gun Kick Pitch Max", "Hipfire viewmodel control. +/- sign means 'kick down/up'. Larger absolute numbers increase viewmodel climb/descent. From -100 to 100. Actual viewmodel angle will not exceed Max Gun Pitch.");
            createTip("ADS Gun Kick Pitch Max", "ADS viewmodel control. +/- sign means 'kick down/up'. Larger absolute numbers increase viewmodel climb/descent. From -100 to 100.");
            createTip("Hip Gun Kick Yaw Min", "Hipfire viewmodel control. +/- sign means 'kick left/right'. Larger absolute numbers increase viewmodel yawing. From -100 to 100. Actual viewmodel angle will not exceed Max Gun Yaw.");
            createTip("ADS Gun Kick Yaw Min", "ADS viewmodel control. +/- sign means 'kick left/right'. Larger absolute numbers increase viewmodel yawing. From -100 to 100.");
            createTip("Hip Gun Kick Yaw Max", "Hipfire viewmodel control. +/- sign means 'kick left/right'. Larger absolute numbers increase viewmodel yawing. From -100 to 100. Actual viewmodel angle will not exceed Max Gun Yaw.");
            createTip("ADS Gun Kick Yaw Max", "ADS viewmodel control. +/- sign means 'kick left/right'. Larger absolute numbers increase viewmodel yawing. From -100 to 100.");
            createTip("Hip Gun Kick Accel", "Rate at which viewmodel attempts to recenter in hipfire. Directly opposes yaws and pitch accumulation.");
            createTip("ADS Gun Kick Accel", "Rate at which viewmodel attempts to recenter in ADS. Directly opposes yaws and pitch accumulation.");
            createTip("Hip Gun Kick Speed Max", "Maximum deflection speed reached by the viewmodel in hipfire.");
            createTip("ADS Gun Kick Speed Max", "Maximum deflection speed reached by the viewmodel in ADS.");
            createTip("Hip Gun Kick Speed Decay", "Strength of decay on viewmodel deflection speed in hipfire.");
            createTip("ADS Gun Kick Speed Decay", "Strength of decay on viewmodel deflection speed in ADS.");
            createTip("Hip Gun Kick Static Decay", "Strength of decay on viewmodel recentering once it has stopped deflecting.");
            createTip("ADS Gun Kick Static Decay", "Strength of decay on viewmodel recentering once it has stopped deflecting.");
            createTip("Hip View Kick Pitch Min", "Hipfire view kick control. -/+ sign means 'kick down/up'. Larger absolute numbers increase view kick climb/descent. From -100 to 100.");
            createTip("ADS View Kick Pitch Min", "ADS view kick control. -/+ sign means 'kick down/up'. Larger absolute numbers increase view kick climb/descent. From -100 to 100.");
            createTip("Hip View Kick Pitch Max", "Hipfire view kick control. -/+ sign means 'kick down/up'. Larger absolute numbers increase view kick climb/descent. From -100 to 100.");
            createTip("ADS View Kick Pitch Max", "ADS view kick control. -/+ sign means 'kick down/up'. Larger absolute numbers increase view kick climb/descent. From -100 to 100.");
            createTip("Hip View Kick Yaw Min", "Hipfire view kick control. -/+ sign means 'kick right/left'. Larger absolute numbers increase view kick yaw. From -100 to 100.");
            createTip("ADS View Kick Yaw Min", "ADS view kick control. -/+ sign means 'kick right/left'. Larger absolute numbers increase view kick yaw. From -100 to 100.");
            createTip("Hip View Kick Yaw Max", "Hipfire view kick control. -/+ sign means 'kick right/left'. Larger absolute numbers increase view kick yaw. From -100 to 100.");
            createTip("ADS View Kick Yaw Max", "ADS view kick control. -/+ sign means 'kick right/left'. Larger absolute numbers increase view kick yaw. From -100 to 100.");
            createTip("Hip View Kick Center Speed", "Speed with which the view continuously attempts to recenter in hipfire.");
            createTip("ADS View Kick Center Speed", "Speed with which the view continuously attempts to recenter in ADS.");
#endregion
#region Sway
            createTip("swayMax Angle", "Max angle change that will be applied to the hipfire viewmodel sway.");
            createTip("ADSsway Max Angle", "Max angle change that will be applied to the ADS viewmodel sway.");
            createTip("swayLerp Speed", "Speed at which the sway will lerp in hipfire.");
            createTip("ADSsway Lerp Speed", "Speed at which the sway will lerp in ADS.");
            createTip("swayPitch Scale", "Amount of pitch change in the viewmodel to apply to the sway pitch in hipfire.");
            createTip("ADSsway Pitch Scale", "Amount of pitch change in the viewmodel to apply to the sway pitch in ADS.");
            createTip("swayYaw Scale", "Amount of yaw change in the viewmodel to apply to the sway yaw in hipfire.");
            createTip("ADSsway Yaw Scale", "Amount of yaw change in the viewmodel to apply to the sway yaw in ADS.");
            createTip("swayHoriz Scale", "Amount of yaw change in the viewmodel to apply to the sway horizontal offset in hipfire.");
            createTip("ADSsway Horiz Scale", "Amount of yaw change in the viewmodel to apply to the sway horizontal offset in ADS.");
            createTip("swayVert Scale", "Amount of pitch change in the view model to apply to the sway vertical offset in hipfire.");
            createTip("ADSsway Vert Scale", "Amount of pitch change in the view model to apply to the sway vertical offset in ADS.");
            createTip("swayShell Shock Scale", "This scale gets applied to the weapon sway while you're in shell shock.");
#endregion
#region AI
            createTip("Fight Dist", "Aggro radius. AI using this weapon try to fight enemies detected in this radius. Center of this circle constantly traces along AI's path up to maxdist.");
            createTip("Max Dist", "Effective range radius. AI must get to this distance before opening fire on their target with this weapon.");
            createTip("aiVsAiAccuracyGraph", "Graph file for in-game editing of the non-linear accuracy curve used by the AI for this weapon against another AI.");
            createTip("AI Vs. Player Accuracy", "Graph file for in-game editing of the non-linear accuracy curve used by the AI for this weapon against the player.");
#endregion
#region HUD
            createTip("ReticleCenter", "Center Reticle.");
            createTip("ReticleSide", "Side Reticle.");
            createTip("Reticle Center Size", "Center Size.");
            createTip("Reticle Side Size", "Side Size.");
            createTip("hipReticleSidePos", "Side Position.");
            createTip("reticleMinOfs", "Min Offset.");
            createTip("adsOverlayShader", "Overlay for sniper rifles in ADS. Uses a quarter circle image to construct a full scope view.");
            createTip("adsOverlayShaderLowRes", "Low resolution version of the overlay for sniper rifles in ADS. Uses a quarter circle image to construct a full scope view.  The image is used for 480 verticle resolution or lower and for split screen.");
            createTip("adsOverlayReticle", "Set to 'none' for normal ADS behavior, 'crosshair' to hide viewmodel and display the holdbreah overlay.");
            createTip("FlipKillIcon", "Used for weapons that need to have their kill icon horizontally flipped before displaying. (For MP obituaries).");
            createTip("adsDofStart", "The distance at which depth of field starts to apply to the player's vision while in ADS. Default is 0, cannot be negative");
            createTip("adsDofEnd", "The distance from the adsDofStart which vision will be perfectly in focus. Default is 0, cannot be negative or less than adsDodStart.");
#endregion
#region Sound
            createTip("Firesound", "Fire sound used by player. If not defined, player will play normal 'Fire' that is used by the AI.");
            createTip("FiresoundPlayer", "Fire sound used by player. If not defined, player will play normal 'Fire' that is used by the AI.");
            createTip("Last Shot sound", "Last Shot sound used by player. If not defined, player will play normal 'Last Shot' that is used by the AI.");
            createTip("Last Shot soundplayer", "Last Shot sound used by player. If not defined, player will play normal 'Last Shot' that is used by the AI.");
            createTip("Empty FireSound", "Empty Fire sound used by player. If not defined, player will play normal 'Empty Fire' that is used by the AI.");
            createTip("Empty FireSoundplayer", "Empty Fire sound used by player. If not defined, player will play normal 'Empty Fire' that is used by the AI.");
            createTip("MeleeHitsound", "Leave empty to use default");
            createTip("MeleeMisssound", "Leave empty to use default");
            createTip("Deploysound", "Rechamber sound used by player. If not defined, player will play normal 'Deploy' that is used by the AI.");
            createTip("Deploysoundplayer", "Rechamber sound used by player. If not defined, player will play normal 'Deploy' that is used by the AI.");
            createTip("Finish Deploy Sound", "Rechamber sound used by player. If not defined, player will play normal 'Finish Deploy' that is used by the AI.");
            createTip("Finish Deploy Soundplayer", "Rechamber sound used by player. If not defined, player will play normal 'Finish Deploy' that is used by the AI.");
            createTip("Breakdownsound", "Rechamber sound used by player. If not defined, player will play normal 'Breakdown' that is used by the AI.");
            createTip("Breakdownsoundplayer", "Rechamber sound used by player. If not defined, player will play normal 'Breakdown' that is used by the AI.");
            createTip("Rechambersound", "Rechamber sound used by player. If not defined, player will play normal 'Rechamber' that is used by the AI.");
            createTip("Rechambersoundplayer", "Rechamber sound used by player. If not defined, player will play normal 'Rechamber' that is used by the AI.");
            createTip("Reloadsound", "Reload sound used by player. If not defined, player will play normal 'Reload' that is used by the AI.");
            createTip("Reloadsoundplayer", "Reload sound used by player. If not defined, player will play normal 'Reload' that is used by the AI.");
            createTip("ReloadEmptysound", "Reload Empty sound used by player. If not defined, player will play normal 'Reload Empty' that is used by the AI.");
            createTip("ReloadEmptysoundplayer", "Reload Empty sound used by player. If not defined, player will play normal 'Reload Empty' that is used by the AI.");
            createTip("ReloadStartsound", "Reload Start sound used by player. If not defined, player will play normal 'Reload Start' that is used by the AI.");
            createTip("ReloadStartsoundplayer", "Reload Start sound used by player. If not defined, player will play normal 'Reload Start' that is used by the AI.");
            createTip("ReloadEndsound", "Reload End sound used by player. If not defined, player will play normal 'Reload End' that is used by the AI.");
            createTip("ReloadEndsoundplayer", "Reload End sound used by player. If not defined, player will play normal 'Reload End' that is used by the AI.");
            createTip("noteTrackSoundMap", "Sounds to play when viewmodel hits different notetrack events.  One per line, format is: NOTETRACKNAME,soundalias");
#endregion
#region user-submitted
            createTip("fireRumble", "Rumble file to use when weapon is firing. No effect on PC.");
            createTip("meleeImpactRumble", "Rumble file that is used when player is meleeing and he hit something. No effect on PC.");
            createTip("playerDamage","The amount of damage that is dealt to player when he is hit by AI with this weapon.");
            createTip("deployAnim","The animation that is played when player deploys this weapon to a mantle brush.");
            createTip("adsDownAnim","The animation that tells the game where to move the viewmodel when you go out of ADS.");
            createTip("idleAnim","The animation that is played when the weapon is idle.");
            createTip("lastShotAnim","The animation that is played when the player shoots the last bullet in the clip.");
            createTip("adsUpAnim","The animation that tells the game where to move the viewmodel when you go in ADS.");
            createTip("nightVisionRemoveAnim","The animation that is played when player takes off nightvision. No effect in CoD4 MP and CoDWaW.");
            createTip("nightVisionWearAnim","The animation that is played when player put on nightvision. No effect in CoD4 MP or CoDWaW.");
            createTip("rechamberAnim","The animation that is played when the gun is being rechambered (example: shotgun shells or bolt-action rifle).");
            createTip("adsFireAnim","The animation that is played when player fires the gun while ADSing. It's possible to use the same animation as for normal fire.");
            createTip("meleeAnim","The animation that is played when player melees while holding this weapon.");
            createTip("meleeChargeAnim","The animation that is played when player melees an AI.");
            createTip("sprintOutAnim","The animation that is played when player goes out from sprinting state.");
            createTip("sprintLoopAnim","The animation that is played when player is sprinting.");
            createTip("sprintInAnim","The animation that is played when player goes into sprinting state.");
            createTip("reloadAnim","The animation that is played when player reloads the weapon while the clip is not empty.");
            createTip("reloadEmpty","The animation that is played when player reloads the weapon after he emptied the weapon clip.");
            createTip("adsDofEnd","The end of viewmodel Depth of Field when player is ADSing.");
            createTip("adsDofStart","The start of viewmodel Depth of Field when player is ADSing.");
            createTip("firstRaiseAnim","The animation that is played the first time the weapon is equipped.");
            createTip("killIcon","HUD Icon used in CoDWaWmp's Obituary display to denote what gun the player was killed with.");
            createTip("pickupSoundPlayer","Sound to play on weapon pickup by player. Default: weap_pickup_plr");
            createTip("pickupSound","Sound to play on weapon pickup. Default: weap_pickup");
            createTip("ammoPickupSoundPlayer","Sound to play on ammo pickup by player. Default: ammo_pickup_plr");
            createTip("ammoPickupSound","Sound to play on ammo pickup. Default: ammo_pickup");
            createTip("whizbySound","Sound to play when bullets come close to the player but miss.");
            createTip("vertTurnSpeed","Adjusts the speed of which you can look vertically with this weapon equipped.");
            createTip("horTurnSpeed","Adjusts the speed of which you can look horizontally with this weapon equipped.");
            createTip("putawaySoundPlayer","Sound to play when holstering the weapon by player. Default: weap_putaway_plr");
            createTip("putawaySound","Sound to play when holstering the weapon. Default: weap_putaway");
            createTip("firstRaiseSoundPlayer","Sound to play when equipping the weapon for the first time by the player. Should match the typical raiseSound unless you are making a custom firstRaise.");
            createTip("firstRaiseSound","Sound to play when equipping the weapon for the first time. Should match the typical raiseSound unless you are making a custom firstRaise.");
            createTip("overheatSoundPlayer","Sound to play when the gun is continually shot and overheated by the player. Used for turret-typed weapons.");
            createTip("overheatSound","Sound to play when the gun is continually shot overheated. Used for turret-typed weapons.");
            createTip("flameCooldownSoundPlayer","Sound to play after the weapon is no longer being fired by the player, and the weapons starts to cool-down.");
            createTip("flameCooldownSound","Sound to play after the weapon is no longer being fired, and the weapons starts to cool-down.");
            createTip("flameOnLoopSoundPlayer","Sound to play while the weapon is being fired by the player and continues to play until the player is no longer being firing.");
            createTip("flameOnLoopSound","Sound to play while the weapon is being fired and continues to play until the weapon is no longer being fired.");
            createTip("flameIgniteSoundPlayer","Sound to play when the weapon is fired by the player but only plays for the first 'shot' until the weapon stops firing.");
            createTip("flameIgniteSound","Sound to play when the weapon is fired but only plays for the first 'shot' until the weapon stops firing.");
            createTip("flameOffLoopSoundPlayer","Idle loop-sound to play for the player when the weapon is not being fired.");
            createTip("flameOffLoopSound","Idle loop-sound to play when the weapon is not being fired.");
            createTip("cooldownRate","How fast the gun cools down when the weapon is idle and not being fired.");
            createTip("overheatRate","How fast the gun heats up when the weapon is being fired until it overheats.");
            createTip("overheatWeapon","Whether this gas-weapon can over-heat or can continually fire without penalty.");
            createTip("unlimitedAmmo","Whether this gas-weapon has infinite ammo. Usually used in coordination with OverheatWeapon to limit the weapon's capabilities.");
            createTip("projectileModel","Model of the projectile 'fired' from a grenade-type weapon.");
            createTip("rocketModel","Model of the projectile 'fired' from this projectile-type weapon.");
            createTip("holdButtonToThrow", "Whether you need to press or hold the fire button to shoot this grenade-type weapon.");
            createTip("canUseInVehicle","Whether or not you can equip and fire this weapon while in a vehicle.");
            createTip("twoHanded","Does this weapon use both viewhands in its idle animation?");
            createTip("ammoCounterClip", "The type of ammo image to use when displaying the weapon's clip in the player's ammo HUD.");
            createTip("ammoCounterIconRatio", "The aspect ratio in which the ammo counter icon will be displayed.");
            createTip("aiVsPlayerAccuracyGraph", "The accuracy file used to determine how 'accurate' this weapon when used by AI to fire at players.");
            createTip("suppressAmmoReserveDisplay", "The option to 'suppress' or 'hide' the reserved ammo counter on this weapon.");
            createTip("overheatEndVal", "The value for how long a turret weapon can be fired before it will be disabled for the 'cool down' period.");
            createTip("coolWhileFiring", "The setting for whether the weapon will 'cool' while firing. If the 'overheat' setting is set higher than the 'cooldown' this setting will not take affect.");
            createTip("detonateTime", "The time, in seconds, after which a grenade explosive will 'explode' after throwing it.");
            createTip("projTrailEffect", "The FX file that will play when the projectile weapon is fired, and will follow the projectile as it moves.");
            createTip("requireLockonToFire", "The option for requiring a projectile weapon to lock-on to a target before firing.");
            createTip("useHintString", "The 'hintstring' to display when a player gets within range to pick this weapon off of the ground.");
            createTip("maxRange", "The maximum effective range for this weapon. Once a bullet goes past this range, it will do 0 damage.");
            createTip("semiAuto", "Whether the gasWeapon will fire continuously or semi-auto.");
            createTip("killIconRatio","The sizing ratio for the HUD icon used in Multiplayer's death log. The ratio selects the size used for the icon to allow for proper placement of the icon and surrounding text.");
            createTip("hudIconRatio","The sizing ratio for the HUD icon used for the gun icon on use and on pickup prompt. The ratio selects the size used for the icon to allow for proper placement of the icon and surrounding text.");
            createTip("hudIcon","HUD Icon shown when the weapon is equipped or in weapon's pickup prompt.");
            createTip("adsTransBlendTime","Time it will take to blend into full ADS, including Depth of Field.");
            createTip("aimAssistRangeAds","If the enemy is within this range then aim assist will snap to the enemy when you ADS. Only applicable to the console build of the game.");
            createTip("aimAssistRange","If the enemy is withing this range then aimassist will snap to the enemy. Only applicable to the console build of the game.");
            createTip("autoAimRange","If the enemy if within this range then the camera will automatically turn towards an enemy. Only applicable to the console build of the game.");
            createTip("adsRechamberAnim","Viewmodel animation for rechambering a bolt-action weapon while ADS. Requires 'rechamberWhileADS' to be check to allow the animation to play.");
            createTip("emptyIdleAnim","Viewmodel idle animation while the gun's magazine/clip is empty. Usually used in pistols to keep the slide locked back.");
            createTip("adsLastShotAnim","Viewmodel animation. This is the animation played when the last-shot in the magazine/clip is fired.");
            createTip("fireAnim","Viewmodel animation of the gun firing, from the hip.");
            createTip("reloadEmptyAnim","Viewmodel animation of a reload, while the gun's magazine/clip is empty.");
            createTip("reloadStartAnim","Viewmodel animation to start a reload for shot-guns and weapons where the weapon is loaded by 1 shot at a time. Used when the reload animation is a loop.");
            createTip("emptyRaiseAnim","Viewmodel animation shown when switching to the gun. Usually used in pistols to keep the slide locked back.");
            createTip("reloadEndAnim","Viewmodel animation to end a reload for shot-guns and weapons where the weapon is loaded by 1 shot at a time. Used when the reload animation is a loop.");
            createTip("quickDropAnim","Viewmodel animation shown when swapping to another weapon or holstering their weapons. This is a fast version of the animation used for special purposes.");
            createTip("raiseAnim","Viewmodel animation shown when swapping to the weapon normally.");
            createTip("quickRaiseAnim","Viewmodel animation shown when swapping to the weapon. A fast variant of the animation, used for special purposes.");
            createTip("dropAnim","Viewmodel animation shown when swapping to another weapon or holstering their weapons.");
            createTip("altDropAnim","Viewmodel animation shown when swapping to another weapon or holstering their weapons, alternate variant of the for special purposes. Usually the same as dropAnim.");
            createTip("altRaiseAnim","Viewmodel animation shown when swapping to the weapon, alternate variant of the animation for special purposes. Usually the same as raiseAnim.");
            createTip("nightVisionRemoveSoundPlayer","Sound played when the player takes off night-vision. No effect in COD5.");
            createTip("nightVisionRemoveSound","Sound played when night-vision is taken off for AI and other players. No effect in COD5.");
            createTip("nightVisionWearSoundPlayer","Sound looped for the player when night-vision is equipped. No effect in COD5.");
            createTip("nightVisionWearSound","Sound looped when night-vision is equipped for AI and other players. No effect in COD5.");
            createTip("raiseSoundPlayer","Sound played when weapon is equipped by the player. Accompanies raiseAnim.");
            createTip("raiseSound","Sound played when the weapon is equipped by AI or other players.");
            createTip("meleeSwipeSoundPlayer","Sound played when the player melees with this weapon equipped.");
            createTip("meleeSwipeSound","Sound played when AI or other players melee with this weapon equipped.");
            createTip("lockonAimRangeAds","If the enemy is within this range then the camera will automatically track them while ADS.");
            createTip("lockonAimRange","If the enemy is within this range then the camera will automatically track them.");
            createTip("slowdownAimRangeAds","If the enemy is within this range then the look sensitivity will decrease to allow the player in ADS more precision when enemies are close-up.");
            createTip("slowdownAimRange","If the enemy is within this range then the look sensitivity will decrease to allow for more precision when enemies are close-up.");
            createTip("hipDofEnd","The end of Depth-of-Field effect's range when in hip-view.");
            createTip("hipDofStart","The start of the Depth-of-Field effect's range when in hip-view.");
            createTip("stickiness","Sticking mode for the 'grenade' fired, whether it sticks to all, ground, or doesn't stick.");
            createTip("aiFuseTime","Length of time until the fired 'grenade' from an AI using this weapon explodes.");
            createTip("fuseTime","Length of time until the fired 'grenade' from any player using this weapon explodes.");
            createTip("detonateDelay","Time after the fuse is completed until the 'grenade' actually explodes.");
            createTip("timedDetonation","Whether or not the 'grenade's' detonator is on a timer.");
            createTip("freezeMovementWhenFiring","Whether or not the player will be forced to stay still while the weapon is firing the 'grenade'.");
            createTip("projExplosionSound","Sound to play when the time on the 'grenade' expires and it explodes.");
            createTip("projExplosionEffect","The FX to play when the time on the 'grenade' expires and it explodes");;
            createTip("projExplosionType","What type of explosion or effect the 'grenade' has when it explodes/detonates.");
            createTip("projectileSpeed","How fast the 'grenade' flies through the air after it is 'fired'.");
            createTip("explosionRadius","Radius of the explosion when the 'grenade's' detonation timer is finished.");
            createTip("explosionOuterDamage","Damage dealt to players and AI when on the outside of the explosion.");
            createTip("explosionInnerDamage","Damage dealt to players and AI when they are inside the explosion radius.");
            createTip("projImpactExplode","Whether or not the 'grenade' explodes on impact with a surface.");
            createTip("lastShotEjectEffect","The third-person effect of the shell ejecting from the gun when the last shot from the magazine/clip is fired.");
            createTip("shellEjectEffect","The third-person effect of the shell ejecting from the gun when shot.");
            createTip("bounceSound","Sound when the 'projectile' hits or bounces off of a surface.");

            createTip("offhandClass", "Sets an appropriate class for the type of offhand weapon [Example: Frag Grenade].");
            createTip("altSwitchSoundPlayer", "Sound alias played when the player switches to the alternate weapon in 1st person.");
            createTip("altSwitchSound", "Sound alias played when the player switches to the alternate weapon in 3rd person.");
            createTip("explosionRadiusMin", "Minimum amount of units away an object has to be from the projectile to be affected.");
            createTip("detonateSoundPlayer", "Sound alias played when a player detonates a weapon in 1st person [Example: The click used for the C4 in CoD4 in 1st person].");
            createTip("detonateSound", "Sound alias played when a player detonates a weapon in 3rd person [Example: The click used for the C4 in CoD4 in 3rd person].");
            createTip("pullbackSoundPlayer", "Sound alias played when a player pulls the offhand weapon back in 1st person [Example: Player pulling the pin of a grenade in 1st person].");
            createTip("pullbackSound", "Sound alias played when a player pulls the offhand weapon back in 3rd person [Example: Player pulling the pin of a grenade in 3rd person].");
            createTip("holdFireTime", "Amount of time in seconds that is used for how long the hold-fire animation is played for.");
            createTip("detonateAnim", "Animation used when a player activates a weapon [Example: Player activating C4 in CoD4].");
            createTip("holdFireAnim", "Animation used when a player pulls out an offhand weapon.");
            createTip("hasDetonator", "If checked, detonator animations will play when the player presses the detonate button, and a 'detonate' notify will occur on the player in script.");
            createTip("tagFlash_preparationEffect", "Name of an effect that is attached to tag_flash and plays at the start of the hold-fire animation.");
            createTip("tagFx_preparationEffect", "Name of an effect that is attached to tag_fx and plays at the start of the hold-fire animation.");
            createTip("adsZoomGunFov", "Field of view when aiming down sights.");
            createTip("isHandModelOverridable", "If checked, then the hand model can be updated from the script.");
            createTip("projDudSound", "Sound alias the projectile will play when it impacts before the Activate Distance is reached.");
            createTip("projDudEffect", "Name of an effect that will play if the projectile collides with an object before the Activate Distance is reached.");
            createTip("projectileLifetime", "Amount of time in seconds after which the projectile will explode in teh air, if it hasn't hit anything yet.");
            createTip("guidedMissleType", "Type of missile that determines how the projectile will react when fired.");
            createTip("destabilizeDistance", "Maximum amount of units the projectile goes before becoming unstable.");
            createTip("destabilizationCurvatureMax", "The maximum curvature in degrees per second when projectile becomes unstable.");
            createTip("destabilizationRateTime", "The time in seconds between 'instability' course changes.");
            createTip("projectileDLight", "Set the radius in inches of the light to follow the projectile.");
            createTip("projectileRed", "Sets the red value of the dynamic light [0-255].");
            createTip("projectileGreen", "Sets the green value of the dynamic light [0-255].");
            createTip("projectileBlue", "Sets the blue value of the dynamic light [0-255].");
            createTip("projIgnitionDelay", "Amount of time in seconds for how long after launch to wait for the rocket to ignite.");
            createTip("projIgnitionEffect", "Name of an effect that is played when a projectile first ignites.");
            createTip("projIgnitionSound", "Sound alias played when the projectile is ignited.");
            createTip("projectileSound", "Sound alias played when a projectile is in motion.");
            createTip("stopFireSoundPlayer", "Sound alias played when a weapon is no longer firing in 1st player [Example: Sound when you stop firing the Minigun in 1st person in BO1].");
            createTip("stopFireSound", "Sound alias played when a weapon is no longer firing in 3rd player [Example: Sound when you stop firing the Minigun in 3rd person in BO1].");
            createTip("loopFireSoundPlayer", "Sound alias played when a weapon is being fired continuously in 1st player [Example: Sound when you are firing the Minigun continuously in 1st person in BO1].");
            createTip("loopFireSound", "Sound alias played when a weapon is being fired continuously in 3rd player [Example: Sound when you are firing the Minigun continuously in 3rd person in BO1].");

#endregion
            //findControlsWithNoTips(); //for debugging
            addMouseEnterEvents();
            assignChangedEvents();
            ClearAllControls();

            foreach (string fileLoc in args) //If they dragged/dropped anything onto the program or used "Open New Instance of Current File", this will catch it.
                openFile(fileLoc);
        }
        //Utilities
        void consoleOut(string msg)
        {
            clearConsoleTimer.Start();
            consoleT.Text = "[" + DateTime.Now.ToLongTimeString() + "] " + msg;
        }
        void checkForUpdates (object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("======== CHECKING FOR UPDATES ========");
            // in newVersion variable we will store the  
            // version info from xml file  
            Version newVersion = null;
            // and in this variable we will put the url we  
            // would like to open so that the user can  
            // download the new version  
            // it can be a homepage or a direct  
            // link to zip/exe file  
            string url = "http://ugx-mods.com/downloads/weaponseditor/version.xml";
            string desc = "";
            try
            {
                XmlTextReader reader = new XmlTextReader(url);
                string elementName = "";
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        elementName = reader.Name;
                    else
                    {
                        // for text nodes...  
                        if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                        {
                            // we check what the name of the node was  
                            switch (elementName)
                            {
                                case "version":
                                    Console.WriteLine("version found: " + reader.Value);
                                    newVersion = new Version(reader.Value);
                                    break;
                                case "description":
                                    //Console.WriteLine("description: " + reader.Value);
                                    desc = reader.Value.ToString();
                                    break;
                                case "url":
                                    //Console.WriteLine("url found: " + reader.Value);
                                    url = reader.Value;
                                    break;
                                default:
                                    Console.WriteLine("Unrecognized Element: " + elementName);
                                    break;
                            }
                        }
                    }
                }
                reader.Close();
            }
            catch
            {
                consoleOut("Update Check Failed!. Ensure the program has permission to access the internet and ensure the UGX-Mods site is online.");
            }

            // get the running version  
            Version curVersion = getVersion();
            // compare the versions  
            if (curVersion.CompareTo(newVersion) < 0)
            {
                string title = "Update is available!";
                string question = "An update for UGX WeaponsEditor++ is available!\n\nv" + newVersion + " Changelog: " + desc + "\n\nView the new version now?";
                if (DialogResult.Yes == MessageBox.Show(question, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process.Start(url);
                }
            }
            else
                consoleOut("Update Check Successful. Latest version is v" + newVersion + ". You are up to date.");
        }
        private void addMouseEnterEvents() //disables and clears all controls until they are re-enabled as-needed in the dragDrop event
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if ((control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox))
                        control.MouseEnter += resetToolTipBullshit;
                    else
                        func(control.Controls);
                }
            };

            func(Controls);
        }
        private void resetToolTipBullshit(object sender, System.EventArgs e) //congrats microsoft on making buggy tooltips - this is courtesy of stackoverflow
        {
            toolTip1.Active = false;
            var sentControl = (Control)sender;
            if (sentControl is Control)
                toolTip1.ToolTipTitle = sentControl.Name.Substring(0, sentControl.Name.Length - 1);
            if (sentControl.Name == "convertCod5")
                toolTip1.ToolTipTitle = "Convert BO1 weaponFile to CoD5 Format";
            if (sentControl.Name == "advancedMode")
                toolTip1.ToolTipTitle = "Advanced Mode";
            if (sentControl.Name == "searchBox")
                toolTip1.ToolTipTitle = "Search for Setting";
            toolTip1.Active = true;
            
        }
        private void findControlsWithNoTips() //DEBUG ONLY
        {
            Console.WriteLine("============== tips Debug ===============");
            bool foundDebugtest = false;

            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if(!(control is TabControl) && !(control is TabPage) && !(control is GroupBox) && !(control is Label))
                    {
                        try
                        {
                            if(toolTip1.GetToolTip(control) == "")
                            //Console.WriteLine(control.Name + " has no tooltip!");
                            foundDebugtest = true;
                        }
                        catch (Exception error)
                        {
                            
                        }
                    }
                    else
                        func(control.Controls);
                }
            };

            func(Controls);

            //if (!foundDebugtest)
        }
        private void createTip(string controlName, string msg)
        {
            //Console.WriteLine("Looking for " + controlName + " | Found ?");
            controlName = new string(controlName.Where(c => Char.IsLetterOrDigit(c)).ToArray());

            bool foundDebugtest = false;

            if (controlName.Length > 7 && controlName.Substring(0, 7) == "stancek") //same tooltip applies to multiple stances, save some work and some copy/paste
            {
                string stancevalue = controlName.Substring(7);
                createTip("sprint" + stancevalue, msg);
                createTip("stand" + stancevalue, msg);
                createTip("ducked" + stancevalue, msg);
                createTip("prone" + stancevalue, msg);
                return;
            }

            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if (control.Name.Substring(0, control.Name.Length - 1).ToLower() == controlName.ToLower())
                    {
                        try
                        {
                            toolTip1.SetToolTip((Control)control, msg);
                            //Console.WriteLine("Setting tip for " + control.Name + ": " + msg);
                            foundDebugtest = true;
                        }
                        catch (Exception error)
                        {
                            //Console.WriteLine(">>>>>>>>>>>>>> Setting tip for " + controlName + " failed!");
                        }
                    }
                    else
                        func(control.Controls);
                }
            };

            func(Controls);

            //if (!foundDebugtest) Console.WriteLine(">>>>>>>>>>>>>> Setting tip for " + controlName + " failed!");
        }
        private void clearConsoleTimer_Tick(object sender, EventArgs e)
        {
            consoleT.Text = "";
            clearConsoleTimer.Stop();
        }
        private void ClearAllControls() //disables and clears all controls until they are re-enabled as-needed in the dragDrop event
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if ((control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox) && control.Name != "searchBox" && control.Name != "consoleT" && control.Name != "loadedFileT" && control.Name != "advancedMode" && control.Name != "convertCod5") //some specific stuff that doesn't need to be disabled
                        control.Enabled = false;
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else if (control is RichTextBox)
                        (control as RichTextBox).Clear();
                    else if (control is ComboBox)
                        (control as ComboBox).SelectedIndex = -1;
                    else if (control is CheckBox)
                        (control as CheckBox).Checked = false;
                    else
                        func(control.Controls);
                }
            };

            func(Controls);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                menuFileOpen_Click(null, null);
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                menuFileSave_Click(null, null);
                return true;
            }
            if (keyData == (Keys.Control | Keys.Shift | Keys.S))
            {
                menuFileSaveAs_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void assignChangedEvents() //catch the edits the user makes to settings.
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if (control is TextBox)
                        (control as TextBox).TextChanged += new EventHandler(Form1_TextChanged);
                    else if (control is RichTextBox)
                        (control as RichTextBox).TextChanged += new EventHandler(Form1_RichTextChanged);
                    else if (control is ComboBox)
                        (control as ComboBox).TextChanged += new EventHandler(Form1_SelectedValueChanged);
                    else if (control is CheckBox)
                        (control as CheckBox).CheckedChanged += new EventHandler(Form1_CheckedChanged);
                    else
                        func(control.Controls);
                }
            };

            func(Controls);
        }

        //Program Events
        private void OnMruFile(int number, String filename)
        {
            openFile(filename);
        }
        private void enableButton_Click(object sender, EventArgs e)
        {
            PictureBox btn = (PictureBox)sender;
            TextBox textBox = (TextBox)btn.Parent;
            textBox.Controls.Remove(btn);
            string setting = textBox.Name.Substring(0, textBox.Name.Length - 1);
            sourceDict.Add(setting, "");
        }
        void Form1_CheckedChanged(object sender, EventArgs e)
        {
            if (!fileReadingComplete) return;
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Name == "advancedMode") return;
            
            anyUnsavedChanges = true;
            anyUnSavedChanges_text = checkBox.Name;
            string settingName = checkBox.Name.Substring(0, checkBox.Name.Length - 1);
            if (sourceDict.ContainsKey(settingName))
            {
                if (checkBox.Checked)
                    sourceDict[settingName] = "1";
                else
                    sourceDict[settingName] = "0";
            }
            else
            {
                if (checkBox.Checked)
                    sourceDict.Add(settingName, "1");
                else
                    sourceDict.Add(settingName, "0");
            }
        }
        void Form1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!fileReadingComplete) return;
            ComboBox comboBox = (ComboBox)sender;
            
            anyUnsavedChanges = true;
            anyUnSavedChanges_text = comboBox.Name;
            string settingName = comboBox.Name.Substring(0, comboBox.Name.Length - 1);
            if (sourceDict.ContainsKey(settingName))
                sourceDict[settingName] = comboBox.Text;
            else
                sourceDict.Add(settingName, comboBox.Text);
        }
        void Form1_TextChanged(object sender, EventArgs e)
        {
            if (!fileReadingComplete) return;
            TextBox textBox = (TextBox)sender;
            if (textBox.Name == "searchBox" || textBox.Name == "consoleT" || textBox.Name == "loadedFileT") return;

            anyUnsavedChanges = true;
            anyUnSavedChanges_text = textBox.Name;

            string settingName = textBox.Name.Substring(0, textBox.Name.Length - 1);
            if (sourceDict.ContainsKey(settingName))
                sourceDict[settingName] = textBox.Text;
            else
                sourceDict.Add(settingName,textBox.Text);
        }
        void Form1_RichTextChanged(object sender, EventArgs e)
        {
            if (!fileReadingComplete) return;
            anyUnsavedChanges = true;
            RichTextBox textBox = (RichTextBox)sender;
            anyUnSavedChanges_text = textBox.Name;
            string settingName = textBox.Name.Substring(0, textBox.Name.Length - 1);
            if (sourceDict.ContainsKey(settingName))
                sourceDict[settingName] = textBox.Text;
            else
                sourceDict.Add(settingName, textBox.Text);
        }
        private void dragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void dragDrop(object sender, DragEventArgs e)
        {
            fileReadingComplete = false;
            anyUnsavedChanges = false;
            anyUnSavedChanges_text = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                foreach (string fileLoc in filePaths)
                {
                   openFile(fileLoc);
                }
            }
        }
        private void openFile(string fileLoc)
        {
             // Code to read the contents of the text file
            if (File.Exists(fileLoc))
            {
                Console.WriteLine("============== NEW FILE ===============");
                ClearAllControls();
                fileName1 = Path.GetFileName(fileLoc);
                loadedFileT.Text = fileLoc;
                consoleOut("Loaded " + fileLoc);

                #region File Reading

                //Store each key of the input file as a string for the AutoComplete searchBox
                var autoCompleteList = new AutoCompleteStringCollection();
                        
                //Store each key/value pair of the input file within a Dictionary for read/write.
                StreamReader sourceReader = new StreamReader(File.OpenRead(fileLoc));

                sourceDict = new Dictionary<string, string>();
                List<string> lines = new List<string>();
                int counter = 0;
                Action<Control.ControlCollection> func = null;
                multiline_hideTags = false; 
                while (!sourceReader.EndOfStream)
                {
                    var line = sourceReader.ReadLine();
                    lines.Add(line);
                    //Console.WriteLine(line);
                    var values = line.Split('\\');

                    if (counter == 0 && values[0] != "WEAPONFILE") //if the first key of the first line of the file is not WEAPONFILE, this is not a weaponfile
                    {
                        consoleOut("ERROR: Could not read any stats - invalid file format! (did not find WEAPONFILE\\ keyword)");
                        return;
                    }
                    for (int i = 1; i < values.Length; i = i + 2)
                    {
                        if ((i + 1) <= values.Length && !sourceDict.ContainsKey(values[i])) sourceDict.Add(values[i], values[i + 1]);

                        //Ignore the gunmodels 1 through 11 or whatever, these are not even used
                        if (values[i].Length > 2)
                        {
                            
                            if (values[i].Substring(0, values[i].Length - 1) == "gunModel" && values[i].Length > 8) { }
                            else if (values[i].Substring(0, values[i].Length - 2) == "gunModel" && values[i].Length > 8) { }
                            else if (values[i].Substring(0, values[i].Length - 2) == "worldModel" && values[i].Length > 10) { }
                            else
                                if (!autoCompleteList.Contains(values[i])) 
                                    autoCompleteList.Add(values[i]);
                        }
                        else
                            if (!autoCompleteList.Contains(values[i])) 
                                autoCompleteList.Add(values[i]);
                        
                        

                        bool debugtest_hasControlBeenFound = false;
                        //Keep looping until we've gone through every nested control in the form
                        func = (controls) =>
                        {
                            foreach (Control control in controls)
                            {
                                if (control is TextBox)
                                {
                                    if ((control as TextBox).Name == values[i] + "T")
                                    {
                                        (control as TextBox).Text = values[i + 1];
                                        control.Enabled = true;
                                        debugtest_hasControlBeenFound = true;
                                    }
                                }
                                else if (control is ComboBox)
                                {
                                    if ((control as ComboBox).Name == values[i] + "T")
                                    {
                                        (control as ComboBox).Text = values[i + 1];
                                        control.Enabled = true;
                                        debugtest_hasControlBeenFound = true;
                                    }
                                }
                                else if (control is CheckBox)
                                {
                                    if ((control as CheckBox).Name == values[i] + "C")
                                    {
                                        if (values[i + 1] == "1")
                                            (control as CheckBox).Checked = true;
                                        else 
                                            (control as CheckBox).Checked = false;
                                        control.Enabled = true;
                                        debugtest_hasControlBeenFound = true;
                                    }
                                }
                                else
                                {
                                    func(control.Controls);
                                }
                            }
                        };

                        func(Controls);
                        //if (!debugtest_hasControlBeenFound)
                            //Console.WriteLine("ERROR: NOT FOUND -> " + values[i]);
                        try
                        {
                            if (values[values.Length - 2] == "hideTags" && !multiline_hideTags) //multi-line hideTags flags this as true, if was one-line this would be "notetrackSoundMap"
                            {
                                multiline_hideTags = true;
                            }
                        }
                        catch (Exception error)
                        {
                            //Console.WriteLine("ERROR: " + error);
                        }
                    }
                    counter++;
                }
                sourceReader.Dispose();
                searchBox.AutoCompleteCustomSource = autoCompleteList;
                //Add the first hidetag which is stored correctly by sourceDict.
                if (sourceDict.ContainsKey("hideTags"))
                {
                    Console.WriteLine("Adding [" + sourceDict["hideTags"] + "] to hideTags.text \n[" + hideTagsT.Text + "]");
                    hideTagsT.Text += sourceDict["hideTags"];
                    hideTagsT.Enabled = true;
                }
                //Add the first notetrack which is stored correctly by sourceDict.
                if (sourceDict.ContainsKey("notetrackSoundMap"))
                {
                    Console.WriteLine("Adding [" + sourceDict["notetrackSoundMap"] + "] to notetrackSoundMapT.text \n[" + notetrackSoundMapT.Text + "]");
                    notetrackSoundMapT.Text += sourceDict["notetrackSoundMap"];
                    notetrackSoundMapT.Enabled = true;
                }
                if (multiline_hideTags)
                {
                    bool hideTagsEnd = false;
                    for (int i = 1; i < lines.Count; i++) //Add any continued lines here.
                    {
                        var values = lines[i].Split('\\');
                        for (int j = 0; j < values.Length; j++)
                        {
                            if (values[j] == "notetrackSoundMap")
                            {
                                for (int k = i + 1; k < lines.Count; k++)
                                {
                                    Console.WriteLine("Adding [" + lines[k] + "] to notetrackSoundMapT.text \n[" + notetrackSoundMapT.Text + "]");
                                    notetrackSoundMapT.Text += "\r\n" + lines[k];
                                }
                                hideTagsEnd = true;
                                break;
                            }
                            if (!hideTagsEnd)
                            {
                                Console.WriteLine("Adding [" + values[j] + "] to hideTagsT.text \n[" + hideTagsT.Text + "]");
                                hideTagsT.Text += "\r\n" + values[j];
                            }
                        }
                    }
                }
                else
                {
                    if (sourceDict.ContainsKey("notetrackSoundMap"))
                    {
                        for (int i = 1; i < lines.Count; i++) //Add any continued lines here.
                        {
                            notetrackSoundMapT.Text += "\r\n" + lines[i];
                            Console.WriteLine("Adding [" + lines[i] + "] to notetrackSoundMapT.text [" + notetrackSoundMapT.Text + "]");
                        }
                    }
                }
                detectCoDVersion();
                fileReadingComplete = true;
                #endregion

                mruMenu.AddFile((String)fileLoc);
                mruMenu.SaveToRegistry();
            }
        }
        private void searchBox_keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string setting = searchBox.Text + "T";
                //ActiveControl = this.Controls.Find(setting, true)[0];
                //this.Controls.Find(setting, true)[0].Focus();
                var possibleMatches = this.Controls.Find(setting, true);
                if(possibleMatches.Length > 0)
                    huntDownControl(possibleMatches[0]);
            }
        }

        //Find out which tab contains the desired control by checking the visibility while iterating tabs
        private void huntDownControl(Control wc)
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                {
                    if (control == wc)
                    {
                        if ((control.Parent is TabPage) || (control.Parent is GroupBox) || (control.Parent is Panel))
                        {
                            var baseControl = control;
                            if (((control.Parent is GroupBox) || (control.Parent is Panel)) && !(control.Parent is TabPage))
                            {
                                baseControl = control.Parent;
                            }
                            if (baseControl.Parent.Parent is TabControl)
                            {
                                var tabPage = baseControl.Parent;
                                if (baseControl.Parent.Parent.Parent.Parent is TabControl)
                                {
                                    (baseControl.Parent.Parent.Parent.Parent as TabControl).SelectedTab = (TabPage)baseControl.Parent.Parent.Parent;
                                }
                                (baseControl.Parent.Parent as TabControl).SelectedTab = (TabPage)baseControl.Parent;
                            }
                        }
                        control.Focus();
                    }
                    else
                        func(control.Controls);
                }
            };

            func(Controls);
        }

        private void advancedMode_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            Action<Control.ControlCollection> func = null;

            if (checkBox.Checked)
            {
                savedControlsState.Clear();
                disableBOSettings("Enable");
                consoleOut("Advanced mode activated. All disabled settings are now available.");
                func = (controls) =>
                {
                    foreach (Control control in controls)
                    {
                        if (control is TextBox)
                        {
                            if (!control.Enabled)
                            {
                                savedControlsState.Add(control);
                                control.Enabled = true;
                            }

                        }
                        else if (control is ComboBox)
                        {
                            if (!control.Enabled)
                            {
                                savedControlsState.Add(control);
                                control.Enabled = true;
                            }
                        }
                        else if (control is CheckBox)
                        {
                            if (!control.Enabled)
                            {
                                savedControlsState.Add(control);
                                control.Enabled = true;
                            }
                        }
                        else
                        {
                            func(control.Controls);
                        }
                    }
                };
                this.consoleOut("Disabled BO settings.");
                func(Controls);
            }
            else
            {
                consoleOut("Advanced mode deactivated. All previously disabled settings are now disabled again.");

                func = (controls) =>
                {
                    if (fileFormatL.Text == "CoD5")
                    {
                        disableBOSettings("Disable");
                        this.consoleOut("All BO settings are disabled.");
                    }
                    foreach (Control control in savedControlsState)
                    {
                        if (control.Name == "searchBox" || control.Name == "consoleT" || control.Name == "loadedFileT" || control.Name == "advancedMode" || control.Name == "convertCod5") //some specific stuff that doesn't need to be disabled
                            continue;

                        if (control is TextBox)
                        {
                            if (control.Enabled)
                            {
                                //Console.WriteLine(control.Name + " is enabled and was in array, disabling");
                                control.Enabled = false;
                            }

                        }
                        else if (control is ComboBox)
                        {
                            if (control.Enabled)
                            {
                                control.Enabled = false;
                            }
                        }
                        else if (control is CheckBox)
                        {
                            if (control.Enabled)
                            {
                                control.Enabled = false;
                            }
                        }
                        else
                        {
                            func(control.Controls);
                        }
                    }
                };

                func(Controls);
            }
        }

        #region Folder and URL links, simple stuff
        private void modsButton_Click(object sender, EventArgs e)
        {
            string path = GetRootFolder() + "mods";
            Console.WriteLine(path);
            if (Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
            else
                RelocateCoDWaW();
        }
        private void spFolder_Click(object sender, EventArgs e)
        {
            string path = GetRootFolder() + "raw\\weapons";

            if (Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
            else
                RelocateCoDWaW();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ugx-mods.com/forum");
        }
        private string GetRootFolder()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Activision\\Call of Duty WAW");
            string path = Convert.ToString(key.GetValue("InstallPath"));
            if (string.IsNullOrWhiteSpace(path)) path = "";
            return GetActualDirectoryPath(path);
        }
        private string GetActualDirectoryPath(string path)
        {
            if (path.Length == 0)
                return path;
            if (path[path.Length - 1] != '\\')
                path += "\\";
            return path;
        }
        private void RelocateCoDWaW()
        {
            //Prompt them to find their root folder.
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            fbd.Description = "Your CoDWaW installation could not be found. Please navigate to it. The location will be saved to your registry so that other programs can correctly find your installation.";
            DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;
            string path = fbd.SelectedPath;
            //guy didn't install the game legitimately, create the key for him to prevent problems with less-intelligent programs :P
            Microsoft.Win32.RegistryKey newkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\Activision\\Call of Duty WAW");
            newkey.SetValue("InstallPath", path);
        }
        #endregion

        private void readAndSaveStats(bool overwriteTheFile)
        {
            #region File Writing

            string fileLoc = loadedFileT.Text;
            if (!overwriteTheFile)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Title = "Save new weapponFile as...";
                save.ShowDialog();
                fileLoc = save.FileName;
            }

            if (fileLoc == null || fileLoc == "")
            {
                consoleOut("Save action canceled.");
                return;
            }

            List<string> lines = new List<string>();

            bool foundHideTags = false;
            bool foundNoteTrack = false;

            try
            {
                using (StreamWriter wr = new StreamWriter(fileLoc))
                {
                    wr.Write("WEAPONFILE\\");
                    
                    //Write all settings to the file except the notetrack or hidetags
                    foreach (KeyValuePair<string, string> setting in sourceDict)
                    {
                        if (setting.Key == "hideTags" || setting.Key == "notetrackSoundMap")
                        {
                            continue;
                        }
                        else
                            wr.Write(setting.Key + "\\" + setting.Value + "\\");
                    }
                    //Now that all other settings are added, tack on the notetracks and hidetags.
                    foreach (KeyValuePair<string, string> setting in sourceDict)
                    {
                        if (setting.Key == "hideTags")
                        {
                            wr.Write(setting.Key + "\\" + hideTagsT.Text + "\\");
                            foundHideTags = true;
                        }
                        else if (setting.Key == "notetrackSoundMap")
                        {
                            wr.Write(setting.Key + "\\" + notetrackSoundMapT.Text);
                            foundNoteTrack = true;
                        }
                        else
                            continue;
                    }
                    if (!foundHideTags) wr.Write("hideTags\\\\");
                    if (!foundNoteTrack) wr.Write("notetrackSoundMap\\");
                }

                if (!overwriteTheFile)
                {
                    consoleOut("File saved successfully.");
                    mruMenu.AddFile((String)fileLoc);
                    mruMenu.SaveToRegistry();

                    OpenNewlyCreatedFile OpenNewlyCreatedFileDialog = new OpenNewlyCreatedFile();

                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\UGX\\WeaponsEditor");
                    if (key.GetValue("NewFileConfirmation") == null)
                    {
                        DialogResult result = OpenNewlyCreatedFileDialog.ShowDialog();
                        if (result == DialogResult.Yes)
                        {
                            string checkbox = Convert.ToString(OpenNewlyCreatedFileDialog.areYouSureChecked);
                            if (checkbox == "True")
                            {
                                //Console.WriteLine("Key not found, saving to YES");
                                Microsoft.Win32.RegistryKey newkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\UGX\\WeaponsEditor");
                                newkey.SetValue("NewFileConfirmation", "Yes");
                            }
                            openFile(fileLoc);
                        }
                        else if (result == DialogResult.No)
                        {
                            string checkbox = Convert.ToString(OpenNewlyCreatedFileDialog.areYouSureChecked);
                            if (checkbox == "True")
                            {
                                //Console.WriteLine("Key not found, saving to NO");
                                Microsoft.Win32.RegistryKey newkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\UGX\\WeaponsEditor");
                                newkey.SetValue("NewFileConfirmation", "No");
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Key found.... " + (string)key.GetValue("NewFileConfirmation"));
                        if ((string)key.GetValue("NewFileConfirmation") == "Yes")
                            openFile(fileLoc);
                    }
                }
                else consoleOut("File overwritten successfully.");
                anyUnsavedChanges = false;
                anyUnSavedChanges_text = "";
            }
            catch (IOException e)
            {
                consoleOut("Could not write to the file. Ensure that the file is not open in another program.");
                MessageBox.Show(e.Message);
            }
           
            #endregion
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (loadedFileT.Text == "") return;
            AreYouSure AreYouSureDialog = new AreYouSure();

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\UGX\\WeaponsEditor");
            if (key.GetValue("OverwriteConfirmation") == null)
            {
                DialogResult result = AreYouSureDialog.ShowDialog();
                if (result == DialogResult.Yes)
                {
                    string checkbox = Convert.ToString(AreYouSureDialog.areYouSureChecked);
                    if (checkbox == "True")
                    {
                        Console.WriteLine("Key not found, saving to YES");
                        Microsoft.Win32.RegistryKey newkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\UGX\\WeaponsEditor");
                        newkey.SetValue("OverwriteConfirmation", "Yes");
                    }
                    readAndSaveStats(true);
                }
                else if (result == DialogResult.No)
                {
                    string checkbox = Convert.ToString(AreYouSureDialog.areYouSureChecked);
                    if (checkbox == "True")
                    {
                        Console.WriteLine("Key not found, saving to NO");
                        Microsoft.Win32.RegistryKey newkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\UGX\\WeaponsEditor");
                        newkey.SetValue("OverwriteConfirmation", "No");
                    }
                    saveAsButton_Click(sender, e);
                }
            }
            else
            {
                Console.WriteLine("Key found.... " + (string)key.GetValue("OverwriteConfirmation"));
                if ((string)key.GetValue("OverwriteConfirmation") == "Yes")
                    readAndSaveStats(true);
                else
                    saveAsButton_Click(sender, e);
            }
        }
        private void saveAsButton_Click(object sender, EventArgs e)
        {
            if (loadedFileT.Text == "") return;
            readAndSaveStats(false);
        }
        private void detectCoDVersion()
        {
            bool BO1 = false;

            foreach (KeyValuePair<string, string> setting in sourceDict.ToList())
            {
                foreach (string bo1setting in miscBO1Settings.ToList())
                {
                    if (setting.Key == bo1setting)
                    {
                        BO1 = true;
                        break;
                    }
                }
            }

            if (BO1)
                fileFormatL.Text = "BO1";
            else if (!BO1)
                fileFormatL.Text = "CoD5";
            else
                fileFormatL.Text = "N/A";
        }
        private void convertCod5_Click(object sender, EventArgs e)
        {
            disableBOSettings("Disable");
            bool deleteRest = false;
            bool deletedAny = false;
            int count = 0;

            foreach (KeyValuePair<string, string> setting in sourceDict.ToList())
            {
                if (setting.Key == "adsZoomFov2")
                {
                    sourceDict["adsZoomFov"] = setting.Value;
                    adsZoomFovT.Enabled = true;
                    adsZoomFovT.Text = setting.Value;
                }
                foreach (string bo1setting in miscBO1Settings.ToList())
                {
                    if (setting.Key == bo1setting)
                    {
                        deletedAny = true;
                        count++;
                        sourceDict.Remove(setting.Key);
                    }
                    else if (setting.Key == "playerAnimType")
                    {
                        if (sourceDict.ContainsKey("playerAnimType"))
                        {
                            sourceDict["playerAnimType"] = "autorifle";
                            playerAnimTypeT.Text = "autorifle";
                        }
                    }
                }
                //The first in the list of ikHandle trash.

                if (setting.Key == "ikLeftHandOffsetF")
                    deleteRest = true;
                if (deleteRest)
                    sourceDict.Remove(setting.Key);
            }
            if (deleteRest || deletedAny)
            {
                anyUnsavedChanges = true;
                fileFormatL.Text = "CoD5";
                consoleOut("File successfully converted. Remember to check the playerAnimType and save your changes!");
            }
            else
            {
                consoleOut("Error: Not a BO1 weaponFile, conversion failed.");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (anyUnsavedChanges)
            {
                string message = "Changes to \"" + anyUnSavedChanges_text.Substring(0, anyUnSavedChanges_text.Length - 1) + "\" have not yet been saved. Are you sure you want to quit?";
                const string caption = "You have unsaved changes!";
                var result = MessageBox.Show(message, caption,
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);

                e.Cancel = (result == DialogResult.No);
            }
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "weaponFiles|*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            // Process input if the user clicked OK.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                if (openFileDialog1.CheckFileExists)
                    openFile(openFileDialog1.FileName);
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            saveButton_Click(sender, e);
        }

        private void menuFileSaveAs_Click(object sender, EventArgs e)
        {
            saveAsButton_Click(sender, e);
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuToolsMods_Click(object sender, EventArgs e)
        {
            modsButton_Click(sender, e);
        }

        private void menuToolsRaw_Click(object sender, EventArgs e)
        {
            spFolder_Click(sender, e);
        }

        private void menuToolsConvert_Click(object sender, EventArgs e)
        {
            convertCod5_Click(sender, e);
        }

        private void menuToolsUpdates_Click(object sender, EventArgs e)
        {
            _bw.RunWorkerAsync();
        }

        private void menuToolsReset_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\UGX\\WeaponsEditor", true);
            if (key != null)
                key.DeleteValue("OverwriteConfirmation");
            consoleOut("File Overwrite Warning preference reset. You will be asked next time you try to overwrite a file.");
        }

        private void menuToolsRepair_Click(object sender, EventArgs e)
        {
            RelocateCoDWaW();
        }

        private void menuToolsAbout_Click(object sender, EventArgs e)
        {
            About AboutDialog = new About();
            AboutDialog.ShowDialog();
        }

        private void menuItem6_Click(object sender, EventArgs e)
        { // Current File Instance
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = Application.ExecutablePath;
            p.StartInfo.Arguments = "\"" + loadedFileT.Text.ToString() + "\"";
            p.Start();
        }

        private void menuItem5_Click(object sender, EventArgs e)
        { //New Instance
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = Application.ExecutablePath;
            p.Start();
        }

        private void disableBOSettings(string option)
        {
            TextBox[] boExtras =
            {
                    dtp_empty_inT,
                    dtp_empty_loopT,
                    dtp_empty_outT,
                    dtp_inT,
                    dtp_loopT,
                    dtp_outT,
                    lowReadyInAnimT,
                    lowReadyLoopAnimT,
                    lowReadyOutAnimT,
                    parentWeaponNameT,
                    DualWieldWeaponT,
                    lowReadyInTimeT,
                    lowReadyLoopTimeT,
                    lowReadyOutTimeT,
                    dtpInTimeT,
                    dtpLoopTimeT,
                    dtpOutTimeT,
                    lowReadyOfsFT,
                    lowReadyOfsRT,
                    lowReadyOfsUT,
                    lowReadyRotPT,
                    lowReadyRotYT,
                    lowReadyRotRT,
                    sprintInEmptyAnimT,
                    sprintLoopEmptyAnimT,
                    sprintOutEmptyAnimT,
                    adsZoomFov1T,
                    adsZoomFov2T,
                    adsZoomFov3T,
                    adsZoomSoundT
            };

            if (option == "Disable")
            {
                groupBox12.Enabled = false;
                foreach (TextBox boSetting in boExtras)
                {
                    boSetting.Enabled = false;
                    boSetting.Text = "";
                }
            }
            else if(option == "Enable")
            {
                groupBox12.Enabled = true;
                foreach (TextBox boSetting in boExtras)
                {
                    boSetting.Enabled = true;
                    boSetting.Text = "";
                }
            }
        }

        private Version getVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}