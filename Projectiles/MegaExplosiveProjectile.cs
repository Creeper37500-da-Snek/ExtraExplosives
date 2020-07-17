﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static ExtraExplosives.GlobalMethods;

namespace ExtraExplosives.Projectiles
{
	public class MegaExplosiveProjectile : ModProjectile
	{
		private const int PickPower = 65;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("MegaExplosive");
		}

		public override void SetDefaults()
		{
			projectile.tileCollide = true;
			projectile.width = 32;
			projectile.height = 38;
			projectile.aiStyle = 16;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 500;
		}

		public override void Kill(int timeLeft)
		{
			//Create Bomb Sound
			Main.PlaySound(SoundID.Item14, (int)projectile.Center.X, (int)projectile.Center.Y);

			//Create Bomb Dust
			CreateDust(projectile.Center, 800);

			//Create Bomb Damage
			ExplosionDamage(40f * 1.5f, projectile.Center, 600, 70, projectile.owner);

			//Create Bomb Explosion
			CreateExplosion(projectile.Center, 40);
		}

		private void CreateExplosion(Vector2 position, int radius)
		{
			for (int x = -radius; x <= radius; x++) //Starts on the X Axis on the left
			{
				for (int y = -radius; y <= radius; y++) //Starts on the Y Axis on the top
				{
					int xPosition = (int)(x + position.X / 16.0f);
					int yPosition = (int)(y + position.Y / 16.0f);

					if (Math.Sqrt(x * x + y * y) <= radius + 0.5 && (WorldGen.InWorld(xPosition, yPosition))) //Circle
					{
						ushort tile = Main.tile[xPosition, yPosition].type;
						if (!CanBreakTile(tile, PickPower)) //Unbreakable CheckForUnbreakableTiles(tile) ||
						{
						}
						else //Breakable
						{
							WorldGen.KillTile(xPosition, yPosition, false, false, false); //This destroys Tiles
							if (CanBreakWalls) WorldGen.KillWall(xPosition, yPosition, false); //This destroys Walls
						}
					}
				}
			}
		}

		private void CreateDust(Vector2 position, int amount)
		{
			Dust dust;
			Vector2 updatedPosition;

			for (int i = 0; i <= amount; i++)
			{
				if (Main.rand.NextFloat() < DustAmount)
				{
					//---Dust 1---
					if (Main.rand.NextFloat() < 1f)
					{
						updatedPosition = new Vector2(position.X - 900 / 2, position.Y - 900 / 2);

						dust = Main.dust[Terraria.Dust.NewDust(updatedPosition, 900, 900, 6, 0f, 0.5263162f, 0, new Color(255, 0, 0), 15f)];
						dust.noGravity = true;
						dust.fadeIn = 2.486842f;
					}
					//------------

					//---Dust 2---
					if (Main.rand.NextFloat() < 1f)
					{
						updatedPosition = new Vector2(position.X - 900 / 2, position.Y - 900 / 2);

						dust = Main.dust[Terraria.Dust.NewDust(updatedPosition, 900, 900, 203, 0f, 0f, 0, new Color(255, 255, 255), 15f)];
						dust.noGravity = true;
						dust.noLight = true;
					}
					//------------

					//---Dust 3---
					if (Main.rand.NextFloat() < 1f)
					{
						updatedPosition = new Vector2(position.X - 900 / 2, position.Y - 900 / 2);

						dust = Main.dust[Terraria.Dust.NewDust(updatedPosition, 900, 900, 31, 0f, 0f, 0, new Color(255, 255, 255), 15f)];
						dust.noGravity = true;
						dust.noLight = true;
					}
					//------------
				}
			}
		}
	}
}