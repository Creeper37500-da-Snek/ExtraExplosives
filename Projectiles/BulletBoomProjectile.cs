using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExtraExplosives.Projectiles
{
    public class BulletBoomProjectile : ModProjectile
    {
        public override bool CloneNewInstances => true;    // DONT CHANGE
		public override string Texture => "ExtraExplosives/Items/Explosives/BulletBoomItem";    // texture, change if needed
        
		// Variables
		private int _projectileID;
		//internal static bool CanBreakWalls;    // doesn't seem necessary but left alone just in case

		/*public TestProjectile(int projectileId)
		{
			_projectileID = projectileId;
		}*/
		
		public void SetProjectile(int projectileID)
		{
			_projectileID = projectileID;
		}

		public int GetProjectile() => _projectileID;
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Test Projectile");    // internal name only, will not have a space for the projName piece
		}

		public override void SetDefaults()
		{
			projectile.tileCollide = true; //checks to see if the projectile can go through tiles
			projectile.width = 22;   //This defines the hitbox width
			projectile.height = 22;	//This defines the hitbox height
			projectile.aiStyle = 16;  //How the projectile works, 16 is the aistyle Used for: Grenades, Dynamite, Bombs, Sticky Bomb.
			projectile.friendly = true; //Tells the game whether it is friendly to players/friendly npcs or not
			projectile.penetrate = 1; //Tells the game how many enemies it can hit before being destroyed
			projectile.timeLeft = 40; //The amsadount of time the projectile is alive for
			projectile.knockBack = _projectileID;
		}

		public override bool OnTileCollide(Vector2 old)
		{
			projectile.position.X = projectile.position.X + (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y + (float)(projectile.height / 2);
			projectile.width = 20;
			projectile.height = 64;
			projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
			projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);

			projectile.velocity.X = 0;
			projectile.velocity.Y = 0;
			projectile.aiStyle = 0;
			return true;
		}

		public override void Kill(int timeLeft)
		{
			_projectileID = projectile.damage;
			Player player = Main.player[projectile.owner];

			Vector2 position = projectile.Center;
			Main.PlaySound(SoundID.Item14, (int)position.X, (int)position.Y);
			int radius = 5;	 //this is the explosion radius, the highter is the value the bigger is the explosion

			Vector2 vel;
			int spedX;
			int spedY;
			int cntr = 0;

			for (int x = -radius; x <= radius; x++)
			{
				for (int y = -radius; y <= radius; y++)
				{
					int xPosition = (int)(x + position.X / 16.0f);
					int yPosition = (int)(y + position.Y / 16.0f);

					if (Math.Sqrt(x * x + y * y) <= radius + 0.5)   //this make so the explosion radius is a circle
					{
						mod.Logger.Debug(projectile.damage);
						if (WorldGen.TileEmpty(xPosition, yPosition))
						{
							spedX = Main.rand.Next(15) - 7;
							spedY = Main.rand.Next(15) - 7;
							if (spedX == 0) spedX = 1;
							if (spedY == 0) spedY = 1;
							if (++cntr <= 100) Projectile.NewProjectile(position.X + x, position.Y + y, spedX, spedY, (int)projectile.knockBack, projectile.damage, 20, projectile.owner, 0.0f, 0);
						}
						else
						{
							spedX = Main.rand.Next(15) - 7;
							spedY = Main.rand.Next(15) - 7;
							if (spedX == 0) spedX = 1;
							if (spedY == 0) spedY = 1;
							if (++cntr <= 100) Projectile.NewProjectile(position.X + x, position.Y + y, spedX, spedY, (int)projectile.knockBack, projectile.damage, 20, projectile.owner, 0.0f, 0);
						}
					}
				}
			}

			Dust dust1;
			Dust dust2;
			// You need to set position depending on what you are doing. You may need to subtract width/2 and height/2 as well to center the spawn rectangle.
			for (int i = 0; i < 100; i++)
			{
				if (Main.rand.NextFloat() < ExtraExplosives.dustAmount)
				{
					Vector2 position1 = new Vector2(position.X - 100 / 2, position.Y - 100 / 2);
					dust1 = Main.dust[Terraria.Dust.NewDust(position1, 100, 100, 0, 0f, 0f, 171, new Color(33, 0, 255), 4.0f)];
					dust1.noGravity = true;
					dust1.noLight = true;
				}
			}

			for (int i = 0; i < 100; i++)
			{
				if (Main.rand.NextFloat() < ExtraExplosives.dustAmount)
				{
					Vector2 position2 = new Vector2(position.X - 80 / 2, position.Y - 80 / 2);
					dust2 = Main.dust[Terraria.Dust.NewDust(position2, 80, 80, 6/*35*/, 0f, 0f, 0, new Color(240, 240, 240), 4.0f)];
					dust2.noGravity = true;
				}
			}
		}
    }
}