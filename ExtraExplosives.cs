
using ExtraExplosives.Items;
using ExtraExplosives.Items.Accessories;
using ExtraExplosives.Items.Accessories.AnarchistCookbook;
using ExtraExplosives.Items.Accessories.BombardierClassAccessories;
using ExtraExplosives.Items.Accessories.ChaosBomb;
using ExtraExplosives.Items.Armors.Asteroid;
using ExtraExplosives.Items.Armors.CorruptedAnarchy;
using ExtraExplosives.Items.Armors.CrimsonAnarchy;
using ExtraExplosives.Items.Armors.DungeonBombard;
using ExtraExplosives.Items.Armors.Hazard;
using ExtraExplosives.Items.Armors.HeavyAutomated;
using ExtraExplosives.Items.Armors.Lizhard;
using ExtraExplosives.Items.Armors.Meltbomber;
using ExtraExplosives.Items.Armors.Nova;
using ExtraExplosives.Items.Armors.SpaceDemolisher;
using ExtraExplosives.Items.Armors.TunnelRat;
using ExtraExplosives.Items.Explosives;
using ExtraExplosives.NPCs.CaptainExplosiveBoss;
using ExtraExplosives.NPCs.CaptainExplosiveBoss.BossProjectiles;
using ExtraExplosives.Projectiles;
using ExtraExplosives.Projectiles.Weapons.DutchmansBlaster;
using ExtraExplosives.Projectiles.Weapons.NovaBuster;
using ExtraExplosives.Projectiles.Weapons.TrashCannon;
using ExtraExplosives.UI.AnarchistCookbookUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ExtraExplosives
{
    public class ExtraExplosives : Mod
    {
        //Hotkeys
        internal static ModHotKey TriggerExplosion;
        internal static ModHotKey TriggerUIReforge;
        internal static ModHotKey ToggleCookbookUI;
        internal static ModHotKey TriggerBoost;
        internal static ModHotKey TriggerNovaBomb;
        internal static ModHotKey TriggerLizhard;

        //nuke
        public static bool NukeActivated;
        public static bool NukeActive;
        public static Vector2 NukePos;
        public static bool NukeHit;

        //boss
        public static int bossDropDynamite;

        //dust
        internal static float dustAmount;

        //UI
        internal UserInterface ExtraExplosivesUserInterface;
        internal UserInterface ExtraExplosivesReforgeBombInterface;
        internal UserInterface CEBossInterface;
        internal UserInterface CEBossInterfaceNonOwner;

        //Mod instance
        public static Mod Instance;

        //Boss checks
        public static int CheckUIBoss = 0;
        public static bool CheckBossBreak;
        public static bool firstTick;
        public static float bossDirection;
        public static bool removeUIElements;

        //Github
        public static string GithubUserName => "VolcanicMG";
        public static string GithubProjectName => "ExtraExplosives";

        //Mod version checking
        public static string ModVersion;
        public static string CurrentVersion;

        //Cookbook ui
        internal UserInterface cookbookInterface;
        internal UserInterface buttonInterface;
        internal ButtonUI ButtonUI;
        internal CookbookUI CookbookUI;

        //config
        internal static ExtraExplosivesConfig EEConfig;

        //Boombox
        public static bool boomBoxMusic = false;
        public static int randomMusicID = 0;
        public static int boomBoxTimer = 0;

        //Arrays and Lists
        internal static HashSet<int> avoidList;
        internal static int[] _doNotDuplicate;
        internal static HashSet<int> _tooltipWhitelist;
        internal static HashSet<int> disclaimerTooltip;

        // Create the item to item id reference (used with cpt explosive) Needs to stay loaded
        public ExtraExplosives()
        {

        }

        public override void Unload()
        {
            base.Unload();
            ExtraExplosivesUserInterface = null;
            ModVersion = null;
            Instance = null;
            CurrentVersion = null;
            ModVersion = null;
        }

        internal enum EEMessageTypes : byte
        {
            checkNukeActivated,
            nukeDeactivate,
            checkNukeHit,
            nukeHit,
            nukeNotActive,
            nukeActive,
            checkBossUIYes,
            checkBossUINo,
            BossCheckDynamite,
            bossCheckRocket,
            boolBossCheck,
            checkBossActive,
            setBossInactive,
            bossMovment,
            removeUI
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            EEMessageTypes msgType = (EEMessageTypes)reader.ReadByte();

            switch (msgType)
            {
                //Nuke stuff ------------------------
                case EEMessageTypes.checkNukeActivated:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket myPacket = GetPacket();
                        myPacket.Write((byte)ExtraExplosives.EEMessageTypes.checkNukeActivated);
                        myPacket.Send(ignoreClient: whoAmI);
                    }
                    else
                    {
                        NukeActivated = true;
                    }
                    break;

                case EEMessageTypes.nukeDeactivate:

                    NukeActivated = false;
                    break;

                case EEMessageTypes.checkNukeHit:

                    NukeHit = false;
                    break;

                case EEMessageTypes.nukeHit:

                    NukeHit = true;
                    break;

                case EEMessageTypes.nukeNotActive:

                    NukeActive = false;
                    break;

                case EEMessageTypes.nukeActive:

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket myPacket = GetPacket();
                        myPacket.Write((byte)ExtraExplosives.EEMessageTypes.nukeActive);
                        myPacket.Send(ignoreClient: whoAmI);
                    }
                    else
                    {
                        NukeActive = true;
                    }
                    break;
                //Nuke stuff ------------------------

                case EEMessageTypes.BossCheckDynamite:

                    int randomNumber = reader.ReadVarInt();

                    bossDropDynamite = randomNumber;
                    break;

                case EEMessageTypes.bossCheckRocket:

                    int randomNumber2 = reader.ReadVarInt();

                    bossDropDynamite = randomNumber2;
                    break;

                case EEMessageTypes.bossMovment:

                    float randomFloat = reader.ReadSingle();

                    bossDirection = randomFloat;
                    break;

                case EEMessageTypes.checkBossUIYes:

                    CheckUIBoss = 2;
                    CheckBossBreak = true;


                    break;

                case EEMessageTypes.checkBossUINo:

                    CheckUIBoss = 2;
                    CheckBossBreak = false;


                    break;

                case EEMessageTypes.checkBossActive:

                    CheckUIBoss = 1;
                    break;

                case EEMessageTypes.setBossInactive:

                    CheckUIBoss = 3;
                    break;

                case EEMessageTypes.removeUI:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket myPacket = GetPacket();
                        myPacket.Write((byte)ExtraExplosives.EEMessageTypes.removeUI);
                        myPacket.Send(ignoreClient: whoAmI);
                    }
                    else
                    {
                        removeUIElements = true;
                    }

                    //removeUIElements = true;
                    break;
            }

        }

        public override void PostSetupContent()
        {
            Mod censusMod = ModLoader.GetMod("Census");
            Mod bossChecklist = ModLoader.GetMod("BossChecklist");

            if (censusMod != null)
            {
                // Here I am using Chat Tags to make my condition even more interesting.
                // If you localize your mod, pass in a localized string instead of just English.
                // Additional lines for additional town npc that your mod adds
                // Simpler example:
                censusMod.Call("TownNPCCondition", NPCType("CaptainExplosive"), $"Kill King Slime or Eye Of Cthulhu, then you can buy the[i:{ModContent.ItemType<Unhinged_Letter>()}] from the demolitionist");
            }

            if (bossChecklist != null)
            {
                // AddBoss, bossname, order or value in terms of vanilla bosses, inline method for retrieving downed value.
                bossChecklist.Call("AddBoss", 6, ModContent.NPCType<CaptainExplosiveBoss>(), this, "Captain Explosive", (Func<bool>)(() => ExtraExplosivesWorld.BossCheckDead), ModContent.ItemType<Unhinged_Letter>(), ModContent.ItemType<BombHat>(), ModContent.ItemType<CaptainExplosiveTreasureBag>(), $"Kill King Slime or Eye Of Cthulhu, then you can buy the[i:{ModContent.ItemType<Unhinged_Letter>()}] from the demolitionist");
            }

            _tooltipWhitelist = new HashSet<int> //Whitelist for the (Bombard Item) tag at the end of bombard items.
			{
                //armors
                ModContent.ItemType<AsteroidMiner_B>(),
                ModContent.ItemType<AsteroidMiner_B_O>(),
                ModContent.ItemType<AsteroidMiner_H>(),
                ModContent.ItemType<AsteroidMiner_H_O>(),
                ModContent.ItemType<AsteroidMiner_L>(),
                ModContent.ItemType<AsteroidMiner_L_O>(),

                ModContent.ItemType<Nova_B>(),
                ModContent.ItemType<Nova_H>(),
                ModContent.ItemType<Nova_L>(),

                ModContent.ItemType<CorruptedAnarchy_B>(),
                ModContent.ItemType<CorruptedAnarchy_H>(),
                ModContent.ItemType<CorruptedAnarchy_L>(),

                ModContent.ItemType<CrimsonAnarchy_B>(),
                ModContent.ItemType<CrimsonAnarchy_H>(),
                ModContent.ItemType<CrimsonAnarchy_L>(),

                ModContent.ItemType<DungeonBombard_B>(),
                ModContent.ItemType<DungeonBombard_H>(),
                ModContent.ItemType<DungeonBombard_L>(),
                ModContent.ItemType<DungeonBombard_B>(),

                ModContent.ItemType<Hazard_B>(),
                ModContent.ItemType<Hazard_B_T>(),
                ModContent.ItemType<Hazard_H>(),
                ModContent.ItemType<Hazard_H_T>(),
                ModContent.ItemType<Hazard_L>(),
                ModContent.ItemType<Hazard_L_T>(),

                ModContent.ItemType<HeavyAutomated_B>(),
                ModContent.ItemType<HeavyAutomated_H>(),
                ModContent.ItemType<HeavyAutomated_L>(),

                ModContent.ItemType<Lizhard_B>(),
                ModContent.ItemType<Lizhard_H>(),
                ModContent.ItemType<Lizhard_L>(),

                ModContent.ItemType<Meltbomber_B>(),
                ModContent.ItemType<Meltbomber_H>(),
                ModContent.ItemType<Meltbomber_L>(),

                ModContent.ItemType<SpaceDemolisher_B>(),
                ModContent.ItemType<SpaceDemolisher_B_C>(),
                ModContent.ItemType<SpaceDemolisher_H>(),
                ModContent.ItemType<SpaceDemolisher_H_C>(),
                ModContent.ItemType<SpaceDemolisher_L>(),
                ModContent.ItemType<SpaceDemolisher_L_C>(),

                ModContent.ItemType<Tunnelrat_B>(),
                ModContent.ItemType<Tunnelrat_H>(),
                ModContent.ItemType<Tunnelrat_L>(),

                //Accessories
                ModContent.ItemType<NovaBooster>(),
                ModContent.ItemType<BombardierEmblem>(),
                ModContent.ItemType<BombardsLaurels>(),
                ModContent.ItemType<BombardsPouch>(),
                ModContent.ItemType<BombCloak>(),
                ModContent.ItemType<BombersCap>(),
                ModContent.ItemType<CertificateOfDemolition>(),
                ModContent.ItemType<FleshyBlastingCaps>(),
                ModContent.ItemType<RavenousBomb>(),

                ModContent.ItemType<AlienExplosive>(),
                ModContent.ItemType<Bombshroom>(),
                ModContent.ItemType<ChaosBomb>(),
                ModContent.ItemType<EclecticBomb>(),
                ModContent.ItemType<LihzahrdFuzeset>(),
                ModContent.ItemType<SupernaturalBomb>(),
                ModContent.ItemType<WyrdBomb>(),

                ModContent.ItemType<AnarchistCookbook>(),
                ModContent.ItemType<BlastShielding>(),
                ModContent.ItemType<BombBag>(),
                ModContent.ItemType<CrossedWires>(),
                ModContent.ItemType<GlowingCompound>(),
                ModContent.ItemType<HandyNotes>(),
                ModContent.ItemType<LightweightBombshells>(),
                ModContent.ItemType<MysteryBomb>(),
                ModContent.ItemType<RandomFuel>(),
                ModContent.ItemType<RandomNotes>(),
                ModContent.ItemType<ReactivePlating>(),
                ModContent.ItemType<ResourcefulNotes>(),
                ModContent.ItemType<SafetyNotes>(),
                ModContent.ItemType<ShortFuse>(),
                ModContent.ItemType<StickyGunpowder>(),
                ModContent.ItemType<UtilityNotes>()


            };


            _doNotDuplicate = new int[]
            {
                ModContent.ProjectileType<HouseBombProjectile>(),
                ModContent.ProjectileType<TheLevelerProjectile>(),
                ModContent.ProjectileType<ArenaBuilderProjectile>(),
                ModContent.ProjectileType<ReforgeBombProjectile>(),
                ModContent.ProjectileType<HellavatorProjectile>(),
                ModContent.ProjectileType<RainboomProjectile>(),
                ModContent.ProjectileType<BulletBoomProjectile>(),
                ModContent.ProjectileType<AtomBombProjectile>()
            };

            avoidList = new HashSet<int>
            {
                        ModContent.ProjectileType<BossArmorBreakBombProjectile>(),
                        ModContent.ProjectileType<BossChillBombProjectile>(),
                        ModContent.ProjectileType<BossDazedBombProjectile>(),
                        ModContent.ProjectileType<BossFireBombProjectile>(),
                        ModContent.ProjectileType<BossGooBombProjectile>(),
                        ModContent.ProjectileType<ExplosionDamageProjectileEnemy>(),
                        ProjectileID.BombSkeletronPrime,
                        ProjectileID.DD2GoblinBomb,
                        ProjectileID.HappyBomb,
                        ProjectileID.SantaBombs,
                        ProjectileID.SmokeBomb,
                        ModContent.ProjectileType<HouseBombProjectile>(),
                        ModContent.ProjectileType<CritterBombProjectile>(),
                        ModContent.ProjectileType<BunnyiteProjectile>(),
                        ModContent.ProjectileType<BreakenTheBankenProjectile>(),
                        ModContent.ProjectileType<BreakenTheBankenChildProjectile>(),
                        ModContent.ProjectileType<DaBombProjectile>(),
                        ModContent.ProjectileType<ArenaBuilderProjectile>(),
                        ModContent.ProjectileType<ReforgeBombProjectile>(),
                        ModContent.ProjectileType<TornadoBombProjectile>(),
                        ModContent.ProjectileType<HellavatorProjectile>(),
						//ModContent.ProjectileType<InfinityBombProjectile>(),
						ModContent.ProjectileType<LandBridgeProjectile>(),
                        ModContent.ProjectileType<BoomBoxProjectile>(),
                        ModContent.ProjectileType<FlashbangProjectile>(),
                        ProjectileID.RocketI,
                        ProjectileID.RocketII,
                        ProjectileID.RocketIII,
                        ProjectileID.RocketIV,
                        ProjectileID.RocketSnowmanI,
                        ProjectileID.RocketSnowmanII,
                        ProjectileID.RocketSnowmanIII,
                        ProjectileID.RocketSnowmanIV,
                        ProjectileID.ProximityMineI,
                        ProjectileID.ProximityMineII,
                        ProjectileID.ProximityMineIII,
                        ProjectileID.ProximityMineIV,
                        ProjectileID.Grenade, //Might come back later -----------
						ProjectileID.GrenadeI,
                        ProjectileID.GrenadeII,
                        ProjectileID.GrenadeIII,
                        ProjectileID.GrenadeIV,
                        ProjectileID.BouncyGrenade,
                        ProjectileID.PartyGirlGrenade,
                        ProjectileID.StickyGrenade,//----------------------------
						ModContent.ProjectileType<DynaglowmiteProjectile>(),
                        ModContent.ProjectileType<CleanBombProjectile>(),
                        ModContent.ProjectileType<CleanBombExplosionProjectile>(),
                        ModContent.ProjectileType<RainboomProjectile>(),
                        ModContent.ProjectileType<TrollBombProjectile>(),
                        ModContent.ProjectileType<TorchBombProjectile>(),
                        ModContent.ProjectileType<HydromiteProjectile>(),
                        ModContent.ProjectileType<LavamiteProjectile>(),
                        ModContent.ProjectileType<DeliquidifierProjectile>(),
                        ModContent.ProjectileType<BulletBoomProjectile>(),
                        ModContent.ProjectileType<NPCProjectile>(),
                        ProjectileID.Beenade,
                        ProjectileID.Explosives,
                        ProjectileID.DD2GoblinBomb,
                        ModContent.ProjectileType<DutchmansBlasterProjectile>(),
                        ModContent.ProjectileType<NovaBusterProjectile>(),
                        ModContent.ProjectileType<HealBombProjectile>(),
                        ModContent.ProjectileType<BiomeCleanerProjectile>(),
                        ModContent.ProjectileType<HotPotatoProjectile>(),
                        ModContent.ProjectileType<TrashCannonProjectile>(),
						//ModContent.ProjectileType<WallBombProjectile>()
			};

            disclaimerTooltip = new HashSet<int>
            {
                ModContent.ItemType<HouseBombItem>(),
                ModContent.ItemType<CritterBombItem>(),
                ModContent.ItemType<BunnyiteItem>(),
                ModContent.ItemType<BreakenTheBankenItem>(),
                ModContent.ItemType<DaBombItem>(),
                ModContent.ItemType<ArenaBuilderItem>(),
                ModContent.ItemType<ReforgeBombItem>(),
                ModContent.ItemType<TornadoBombItem>(),
                ModContent.ItemType<HellavatorItem>(),
                ModContent.ItemType<InfinityBombItem>(),
                ModContent.ItemType<LandBridgeItem>(),
                ModContent.ItemType<BoomBoxItem>(),
                ModContent.ItemType<FlashbangItem>(),
                ModContent.ItemType<DynaglowmiteItem>(),
                ModContent.ItemType<CleanBombItem>(),
                ModContent.ItemType<RainboomItem>(),
                ModContent.ItemType<TrollBombItem>(),
                ModContent.ItemType<TorchBombItem>(),
                ModContent.ItemType<HydromiteItem>(),
                ModContent.ItemType<LavamiteItem>(),
                ModContent.ItemType<DeliquidifierItem>(),
                ModContent.ItemType<BulletBoomItem>(),
                ModContent.ItemType<HealBomb>(),
                ModContent.ItemType<BiomeCleanerItem>(),
				//ModContent.ItemType<WallBombItem>()
			};

            base.PostSetupContent();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ExtraExplosivesUserInterface?.Update(gameTime);
            CEBossInterface?.Update(gameTime);
            CEBossInterfaceNonOwner?.Update(gameTime);
            //ExtraExplosivesReforgeBombInterface?.Update(gameTime);

            buttonInterface?.Update(gameTime);
            cookbookInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ExtraExplosives: UI",
                    delegate
                    {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        ExtraExplosivesUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ExtraExplosives: ReforgeBombUI",
                    delegate
                    {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        ExtraExplosivesReforgeBombInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ExtraExplosives: CEBossUI",
                    delegate
                    {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        CEBossInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ExtraExplosives: CEBossUINonOwner",
                    delegate
                    {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        CEBossInterfaceNonOwner.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                "ExtraExplosives: CookbookButton",
                    delegate
                    {
                        if (Main.playerInventory)
                        {
                            buttonInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "ExtraExplosives: CookbookUI",
                    delegate
                    {

                        cookbookInterface.Draw(Main.spriteBatch, new GameTime());

                        return true;

                    },
                    InterfaceScaleType.UI));
            }

            if (mouseTextIndex != -1)
            {

            }


        }

        public override void Load()
        {
            Instance = this;

            Logger.InfoFormat($"{0} Extra Explosives logger", Name);

            //UI stuff
            ExtraExplosivesUserInterface = new UserInterface();
            ExtraExplosivesReforgeBombInterface = new UserInterface();
            CEBossInterface = new UserInterface();
            CEBossInterfaceNonOwner = new UserInterface();
            cookbookInterface = new UserInterface();

            //Hotkey stuff
            TriggerExplosion = RegisterHotKey("Explode", "Mouse2");
            TriggerUIReforge = RegisterHotKey("Open Reforge Bomb UI", "P");
            ToggleCookbookUI = RegisterHotKey("UIToggle", "\\");
            TriggerBoost = RegisterHotKey("TriggerBoost", "S");
            TriggerNovaBomb = RegisterHotKey("TriggerNovaSetBonus", "X");
            TriggerLizhard = RegisterHotKey("TriggerMissleFireLizhard", "Z");

            if (!Main.dedServ)
            {
                buttonInterface = new UserInterface();

                ButtonUI = new ButtonUI();
                ButtonUI.Activate();

                buttonInterface.SetState(ButtonUI);
            }

            //shaders
            if (Main.netMode != NetmodeID.Server)
            {
                //load in the shaders
                Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/Shader")); // The path to the compiled shader file.
                Filters.Scene["Bang"] = new Filter(new ScreenShaderData(screenRef, "Bang"), EffectPriority.VeryHigh); //float4 name
                Filters.Scene["Bang"].Load();

                Ref<Effect> screenRef2 = new Ref<Effect>(GetEffect("Effects/NukeShader")); // The path to the compiled shader file.
                Filters.Scene["BigBang"] = new Filter(new ScreenShaderData(screenRef2, "BigBang"), EffectPriority.VeryHigh); //float4 name
                Filters.Scene["BigBang"].Load();

                // Hot Potato Shader
                Ref<Effect> burningScreenFilter = new Ref<Effect>(GetEffect("Effects/HPScreenFilter"));
                Filters.Scene["BurningScreen"] = new Filter(new ScreenShaderData(burningScreenFilter, "BurningScreen"), EffectPriority.Medium); // Shouldnt override more important shaders
                Filters.Scene["BurningScreen"].Load();

                //Bomb shader
                Ref<Effect> bombShader = new Ref<Effect>(GetEffect("Effects/bombshader"));
                GameShaders.Misc["bombshader"] = new MiscShaderData(bombShader, "BombShader");
            }

            ModVersion = "v" + Version.ToString().Trim();

            //Goes out and grabs the version that the mod browser has
            using (WebClient client = new WebClient())
            {
                if (CheckForInternetConnection())
                {
                    //Parsing the data we need
                    var json = client.DownloadString("https://raw.githubusercontent.com/VolcanicMG/ExtraExplosives/master/Version.txt");
                    json.ToString().Trim();
                    CurrentVersion = "v" + json;
                    client.Dispose();
                }
            }

            Mod yabhb = ModLoader.GetMod("FKBossHealthBar");
            if (yabhb != null)
            {
                yabhb.Call("hbStart");
                yabhb.Call("hbSetTexture",
                 GetTexture("NPCs/CaptainExplosiveBoss/healtbar_left"),
                 GetTexture("NPCs/CaptainExplosiveBoss/healtbar_frame"),
                 GetTexture("NPCs/CaptainExplosiveBoss/healtbar_right"),
                 GetTexture("NPCs/CaptainExplosiveBoss/healtbar_fill"));
                //yabhb.Call("hbSetMidBarOffset", 20, 12);
                //yabhb.Call("hbSetBossHeadCentre", 22, 34);
                yabhb.Call("hbFinishSingle", ModContent.NPCType<CaptainExplosiveBoss>());
            }
        }

        //so if the Internet is out the client won't crash on loading
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (boomBoxMusic)
            {
                if (Main.myPlayer == -1 || Main.gameMenu || !Main.LocalPlayer.active)
                {
                    return;
                }
                music = randomMusicID;
                priority = MusicPriority.BossHigh;
            }
        }

        public override void PostUpdateEverything()
        {
            if (boomBoxMusic)
            {
                boomBoxTimer++;
                if (boomBoxTimer > (1200 + Main.rand.Next(600)))
                {
                    boomBoxMusic = false;
                    boomBoxTimer = 0;
                }
            }

            //Still needs work and most likely reworked(MP issues)
            if (Main.LocalPlayer.dead)
            {
                if (NovaBooster.EngineSoundInstance != null && NovaBooster.EngineSoundInstance.State == SoundState.Playing)
                    NovaBooster.EngineSoundInstance.Stop();
                if (NovaBooster.EndSoundInstance != null && NovaBooster.EndSoundInstance.State == SoundState.Playing)
                    NovaBooster.EndSoundInstance.Stop();
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Instance);
            recipe.AddIngredient(ModContent.ItemType<BombardierEmblem>(), 1);
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddIngredient(ItemID.SoulofSight, 5);
            recipe.AddIngredient(ItemID.SoulofFright, 5);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.SetResult(ItemID.AvengerEmblem);
            recipe.AddRecipe();
            base.AddRecipes();
        }
    }
}
