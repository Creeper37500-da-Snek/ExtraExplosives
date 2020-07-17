﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExtraExplosives.Projectiles
{
	internal class FlashbangProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flashbang");
		}

		public override void SetDefaults()
		{
			projectile.tileCollide = true;
			projectile.width = 12;
			projectile.height = 32;
			projectile.aiStyle = 16;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 100;
			projectile.damage = 0;
		}

		public override void Kill(int timeLeft)
		{
			//add lighting
			Lighting.AddLight(projectile.position, new Vector3(255f, 255f, 255f));
			Lighting.maxX = 100;
			Lighting.maxY = 100;

			Main.PlaySound(SoundID.Item14, (int)projectile.Center.X, (int)projectile.Center.Y); //Sound Effect
			Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/Flashbang"), (int)projectile.Center.X, (int)projectile.Center.Y); //Custom Sound Effect

			Projectile.NewProjectile(projectile.Center.X - 450, projectile.Center.Y, 0, 0, ModContent.ProjectileType<InvisFlashbangProjectile>(), 1, 0, projectile.owner, 0.0f, 0); //Left
			Projectile.NewProjectile(projectile.Center.X + 450, projectile.Center.Y, 0, 0, ModContent.ProjectileType<InvisFlashbangProjectile>(), 1, 1, projectile.owner, 0.0f, 0); //Right
		}
	}
}