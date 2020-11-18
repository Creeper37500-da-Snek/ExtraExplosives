﻿using ExtraExplosives.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExtraExplosives
{
    /// <summary>
    /// This class contains global variables and functions that are used by all other classes
    /// </summary>
    internal class GlobalMethods
    {
        //========================| List of npcs for damage reduction |===============================================\\
        /// <summary>
        /// List of npcs that get damage reduction from explosives by 50% if its in expert mode
        /// </summary>
        public static List<int> DamageReducedNps = new List<int>
        {
            NPCID.EaterofWorldsBody,
            NPCID.EaterofWorldsHead,
            NPCID.EaterofWorldsTail,

        };


        //============================================================================================================\\


        //========================| List of Mods for Mod Integration |========================\\

        /// <summary>
        /// Used Clamaity Mod Integration
        /// </summary>
        public static Mod CalamityMod = ModLoader.GetMod("CalamityMod");

        /// <summary>
        /// Thorium Mod Integration
        /// </summary>
        public static Mod ThoriumMod = ModLoader.GetMod("ThoriumMod");

        //====================================================================================\\

        //========================| Config Variables |========================\\

        /// <summary>
        /// Can be changed in mod config editor - This determines if bombs break walls
        /// </summary>
        public static bool CanBreakWalls;

        /// <summary>
        /// Can be changed in mod config editor - This determines if bombs break tiles
        /// </summary>
        public static bool CanBreakTiles;

        /// <summary>
        /// Can be changed in mod config editor - This determines how much dust is allowed to spawn
        /// </summary>
        public static float DustAmount;

        //====================================================================\\


        //========================| List of Global Functions |========================\\

        /// <summary>
        /// This function spawns in mini-projectiles that are used to "damage" entites when an explosion happens
        /// </summary>
        /// <param name="DamageRadius"> Determines the radius of the damage projectiles explosion </param>
        /// <param name="DamagePosition"> Determines the center point of the damage projectiles explosion </param>
        /// <param name="Damage"> Stores the damage projectiles damage amount </param>
        /// <param name="Knockback"> Stores the damage projectiles knockback amount </param>
        /// <param name="ProjectileOwner"> Stores the owner who called the damage projectile </param>
        public static void ExplosionDamage(float DamageRadius, Vector2 DamagePosition, int Damage, float Knockback, int ProjectileOwner)
        {
            ExplosionDamageProjectile.DamageRadius = DamageRadius; //Sets the radius of the explosion
            Projectile.NewProjectile(DamagePosition, Vector2.Zero, ProjectileType<ExplosionDamageProjectile>(), Damage, Knockback, ProjectileOwner, 0.0f, 0); //Spawns the damage projectile
        }

        /// <summary>
        /// This function deals damage within an area to the player when an explosion happens
        /// </summary>
        /// <param name="DamageRadius"> Determines the radius of the damage projectiles explosion </param>
        /// <param name="DamagePosition"> Determines the center point of the damage projectiles explosion </param>
        /// <param name="Damage"> Stores the damage projectiles damage amount </param>
        /// <param name="npc"> Stores the npc who spawned the projectile </param>
        public static void ExplosionDamageEnemy(int DamageRadius, Vector2 DamagePosition, int Damage, int npc)
        {
            foreach (Player player in Main.player)
            {
                if (player == null || player.whoAmI == 255 || !player.active) return;
                if (player.EE().BlastShielding &&
                    player.EE().BlastShieldingActive) continue;
                float dist = Vector2.Distance(player.Center, DamagePosition);
                int dir = (dist > 0) ? 1 : -1;
                if (dist / 16f <= DamageRadius && Main.netMode == NetmodeID.SinglePlayer)
                {
                    player.Hurt(PlayerDeathReason.ByNPC(npc), Damage, dir);
                    player.hurtCooldowns[0] += 15;
                }
                else if (Main.netMode != NetmodeID.MultiplayerClient && dist / 16f <= DamageRadius)
                {
                    NetMessage.SendPlayerHurt(Main.myPlayer, PlayerDeathReason.ByNPC(npc), Damage, dir, false, pvp: true, 0);
                }
            }
        }

        /// <summary>
        /// This function controls the explosion dust type being produced when bombs explode.
        /// </summary>
        /// <param name="Radius"> Determines the radius of the damage projectiles explosion </param>
        /// <param name="Center"> Determines the center point of the damage projectiles explosion </param>
        /// <param name="type"> The type of dust effect, 1 = Standard, 2 = Rocket, 3 = Special, Default = 1, anything other than 1, 2, 3 will default to 1.</param>
        /// <param name="color"> Color of main part of the dusts </param>
        /// <param name="lightingColor"> Color of light when produced from an explosion </param>
        //Tie in more effects bases off of the radius?
        public static void ExplosionDust(int Radius, Vector2 Center, int type = 1, Color color = default, Color lightingColor = default)
        {
            //Check to see if the type is 1, 2 or 3, else default to 1
            List<int> types = new List<int> { 1, 2, 3 };

            if (!types.Contains(type))
            {
                type = 1;
            }

            //What to preform once the type has been found
            switch (type)
            {
                case 1:
                    Type1Dust(Radius, Center, color, lightingColor);
                    break;
                case 2:

                    break;
                case 3:

                    break;
                default:

                    break;
            }


        }

        //from CosmivengeonMod:
        //spawns in a Projectile that is synced with the server
        public static void SpawnProjectileSynced(Vector2 position, Vector2 velocity, int type, int damage, float knockback, float ai0 = 0f, float ai1 = 0f, int owner = 255)
        {
            if (owner == 255)
                owner = Main.myPlayer;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int proj = Projectile.NewProjectile(position, velocity, type, damage, knockback, owner, ai0, ai1);

                NetMessage.SendData(MessageID.SyncProjectile, number: proj);
            }
        }

        public static void InflictDubuff(int id, int radius, Vector2 position, bool ownerImmune = false, int owner = 255, int? dust = null, int time = 300)
        {
            foreach (NPC npc in Main.npc) // Get each npc
            {
                if (Vector2.Distance(position, npc.Center) / 16f < radius)
                {
                    npc.AddBuff(id, time);
                }
            }

            if (Vector2.Distance(position, Main.player[owner].Center) / 16f < radius &&
                !Main.player[owner].EE().BlastShielding &&
                !ownerImmune)
            {
                Main.player[Main.myPlayer].AddBuff(id, time);
            }

            if (dust == null) return;
            float projX = position.X;
            float projY = position.Y;
            for (float i = projX - radius * 16; i <= projX + radius * 16; i += radius / 4) // Cycle X cords
            {
                for (float j = projY - radius * 16; j <= projY + radius * 16; j += radius / 4) // Cycle Y cords
                {
                    //float dist = Vector2.Distance(new Vector2(i, j), projectile.Center);
                    if (Main.rand.Next(400) == 0) // Random Scattering
                    {
                        Dust.NewDust(new Vector2(i, j), 1, 1, (int)dust);
                    }
                }
            }
        }


        //============================================================================\\

        //check if the tile can be broken or not
        public static bool CanBreakTile(int tileId, int pickPower)
        {
            // Dynamic mod tile functionality at the bottom
            if (pickPower == -1)
                return true; // Override so an item can be set to ignore pickaxe power and destory everything
            if (pickPower <= -2)
                return false; // Override so an item can be set to not damage anything ever also catches invalid garbage
            if (tileId < 470)
            {
                // this is for all blocks which can be destroyed by any pickaxe
                if (Main.tileNoFail[tileId])
                {
                    return true;
                }

                if (tileId == TileID.DefendersForge || tileId == TileID.Containers || tileId == TileID.Containers2 || tileId == TileID.DemonAltar || tileId == TileID.FakeContainers || tileId == TileID.TrashCan || tileId == TileID.Dressers)
                {
                    return false;
                }

                if (tileId == TileID.DesertFossil && pickPower < 65)
                {
                    return false;
                }

                // Meteorite (Power 50)
                if (tileId == 37 && pickPower < 50)
                {
                    return false;
                }

                // Demonite & Crimtane Ores (Power 55)
                if ((tileId == TileID.Demonite || tileId == TileID.Crimtane) && pickPower < 55)
                {
                    return false;
                }

                // Obsidian & Ebonstone Hellstone Pearlstone and Crimstone Blocks (Power 65)
                if ((tileId == TileID.Obsidian || tileId == TileID.Ebonstone || tileId == TileID.Hellstone || tileId == TileID.Pearlstone || tileId == TileID.Crimstone) &&
                    pickPower < 65)
                {
                    return false;
                }

                // Dungeon Bricks (Power 65)
                // Separate from Obsidian block to allow for future functionality to better reflect base game mechanics
                if ((tileId == TileID.BlueDungeonBrick || tileId == TileID.GreenDungeonBrick || tileId == TileID.PinkDungeonBrick)
                    && pickPower < 65)
                {
                    return false;
                }

                // Cobalt & Palladium (Power 100)
                if ((tileId == TileID.Cobalt || tileId == TileID.Palladium)
                    && pickPower < 100)
                {
                    return false;
                }

                // Mythril & Orichalcum (Power 110)
                if ((tileId == TileID.Mythril || tileId == TileID.Orichalcum)
                    && pickPower < 110)
                {
                    return false;
                }

                // Adamantite & Titanium (Power 150)
                if ((tileId == TileID.Adamantite || tileId == TileID.Titanium)
                    && pickPower < 150)
                {
                    return false;
                }

                // Chlorophyte Ore (Power 200)
                if (tileId == TileID.Chlorophyte
                    && pickPower < 200)
                {
                    return false;
                }

                // Lihzahrd Brick (Power 210) todo add additional checks for Lihzahrd traps and the locked temple door
                if ((tileId == TileID.LihzahrdBrick || tileId == TileID.LihzahrdAltar || tileId == TileID.Traps)
                    && pickPower < 210)
                {
                    return false;
                }
            }
            // If the tile is modded, will need updating when tml is updated
            if (tileId > 469)
            {
                int tileResistance = GetModTile(tileId).minPick;
                if (tileResistance <= pickPower) return true;
                return false;
            }
            return true;    // Catch for anything which slipped through, defaults to true
        }

        private static void Type1Dust(int Radius, Vector2 Center, Color color = default, Color lightingColor = default)
        {
            int dustAmount = (int)(Radius * 7);
            if (dustAmount > 40) dustAmount = 40;
            float scale = (100f / Radius) + (Radius / 100f);
            scale = scale / 10 + (Radius / 4);
            Vector2 startpoint = new Vector2(0f, -1f);
            startpoint *= 10; //How fast they all shoot out
            startpoint = startpoint.RotatedByRandom(MathHelper.Pi); //circle
            Vector2 fastBlast = startpoint;

            Vector2 startpoint2 = new Vector2(0f, -1f);
            startpoint2 *= 20;
            startpoint2 = startpoint2.RotatedByRandom(MathHelper.Pi);

            //SPARKEL------------------------------------------------------------------------------------------------------------
            for (int i = 0; i < dustAmount + Radius; i++)
            {
                Dust dust = Dust.NewDustPerfect(Center, 6, startpoint, newColor: color, Scale: scale);
                Dust dustTrail = Dust.NewDustPerfect(Center, 6, startpoint, newColor: color, Scale: scale / 1.9f);
                dust.noGravity = true;
                dustTrail.noGravity = true;
                dust.velocity *= Main.rand.NextFloat((Radius / 6f) + 1);
                dustTrail.velocity *= Main.rand.NextFloat((Radius / 6f) + .5f);
                dust.velocity.SafeNormalize(Center);

                startpoint = startpoint.RotatedBy(MathHelper.ToRadians(360 / dustAmount));
                dust.fadeIn = .8f;
                dustTrail.fadeIn = .4f;

                dust.velocity.Y -= 3f * 0.5f;
                dustTrail.velocity.Y -= 3f * 0.5f;
            }

            //smaller faster
            for (int i = 0; i < dustAmount + (Radius / 2); i++)
            {
                Dust dust;
                if (i % 2 == 0)
                {
                    dust = Dust.NewDustPerfect(Center, 6, fastBlast, newColor: color, Scale: scale / 3);
                }
                else
                {
                    dust = Dust.NewDustPerfect(Center, 6, new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(360)), newColor: color, Scale: scale / 3);
                }

                dust.noGravity = true;
                dust.velocity *= Main.rand.NextFloat(10);
                dust.fadeIn = .2f;

                fastBlast = fastBlast.RotatedBy(MathHelper.ToRadians(360 / dustAmount));
            }

            //More circular (Has issues on smaller explosives)
            for (int i = 0; i < dustAmount + (Radius / 2); i++)
            {
                Dust dust;
                if (i % 2 == 0)
                {
                    dust = Dust.NewDustPerfect(Center, 6, startpoint2, newColor: color, Scale: scale * .8f);
                }
                else
                {
                    dust = Dust.NewDustPerfect(Center, 6, new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(360)), newColor: color, Scale: scale * .5f);
                    dust.velocity *= Main.rand.NextFloat(8);
                }

                dust.noGravity = true;
                dust.fadeIn = .01f;

                startpoint2 = startpoint2.RotatedBy(MathHelper.ToRadians(360 / dustAmount - 20));
            }
            //SPARKEL-------------------------------------------------------------------------------------------------------------

        }

    }
}