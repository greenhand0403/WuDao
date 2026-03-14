using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace WuDao.Content.Projectiles.Melee
{
	// 模仿原版的闪电珠弧射弹闪电效果
	public class StormwrathLightning : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CultistBossLightningOrbArc;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.penetrate = 3;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.extraUpdates = 4;
			Projectile.timeLeft = 120 * (Projectile.extraUpdates + 1);
			Projectile.alpha = 255;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero)
					break;

				Rectangle hitbox = projHitbox;
				hitbox.X = (int)Projectile.oldPos[i].X;
				hitbox.Y = (int)Projectile.oldPos[i].Y;

				if (hitbox.Intersects(targetHitbox))
					return true;
			}

			return false;
		}
		public override void AI()
		{
			// 原版闪电珠弧 vanilla AI 里面有这样的代码
			Projectile.frameCounter++;
			Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.5f);
			if (Projectile.velocity == Vector2.Zero)
			{
				if (Projectile.frameCounter >= Projectile.extraUpdates * 2)
				{
					Projectile.frameCounter = 0;
					bool flag35 = true;
					for (int num774 = 1; num774 < Projectile.oldPos.Length; num774++)
					{
						if (Projectile.oldPos[num774] != Projectile.oldPos[0])
						{
							flag35 = false;
						}
					}
					if (flag35)
					{
						Projectile.Kill();
						return;
					}
				}
				// 绘制粒子
				if (Main.rand.Next(Projectile.extraUpdates) == 0)
				{
					for (int num775 = 0; num775 < 2; num775++)
					{
						float num776 = Projectile.rotation + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
						float num777 = (float)Main.rand.NextDouble() * 0.8f + 1f;
						Vector2 vector89 = new Vector2((float)Math.Cos(num776) * num777, (float)Math.Sin(num776) * num777);
						int num778 = Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, vector89.X, vector89.Y);
						Main.dust[num778].noGravity = true;
						Main.dust[num778].scale = 1.2f;
					}
					if (Main.rand.Next(5) == 0)
					{
						Vector2 vector90 = Projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
						int num779 = Dust.NewDust(Projectile.Center + vector90 - Vector2.One * 4f, 8, 8, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
						Dust dust142 = Main.dust[num779];
						Dust dust3 = dust142;
						dust3.velocity *= 0.5f;
						Main.dust[num779].velocity.Y = 0f - Math.Abs(Main.dust[num779].velocity.Y);
					}
				}
			}
			else
			{
				if (Projectile.frameCounter < Projectile.extraUpdates * 2)
				{
					return;
				}
				Projectile.frameCounter = 0;
				float num780 = Projectile.velocity.Length();
				UnifiedRandom unifiedRandom = new UnifiedRandom((int)Projectile.ai[1]);
				int num781 = 0;
				Vector2 spinningpoint15 = -Vector2.UnitY;
				while (true)
				{
					int num782 = unifiedRandom.Next();
					Projectile.ai[1] = num782;
					num782 %= 100;
					float f = (float)num782 / 100f * ((float)Math.PI * 2f);
					Vector2 vector91 = f.ToRotationVector2();
					if (vector91.Y > 0f)
					{
						vector91.Y *= -1f;
					}
					bool flag36 = false;
					if (vector91.Y > -0.02f)
					{
						flag36 = true;
					}
					if (vector91.X * (float)(Projectile.extraUpdates + 1) * 2f * num780 + Projectile.localAI[0] > 40f)
					{
						flag36 = true;
					}
					if (vector91.X * (float)(Projectile.extraUpdates + 1) * 2f * num780 + Projectile.localAI[0] < -40f)
					{
						flag36 = true;
					}
					if (flag36)
					{
						if (num781++ >= 100)
						{
							Projectile.velocity = Vector2.Zero;
							Projectile.localAI[1] = 1f;
							break;
						}
						continue;
					}
					spinningpoint15 = vector91;
					break;
				}
				if (Projectile.velocity != Vector2.Zero)
				{
					Projectile.localAI[0] += spinningpoint15.X * (float)(Projectile.extraUpdates + 1) * 2f * num780;
					Projectile.velocity = spinningpoint15.RotatedBy(Projectile.ai[0] + (float)Math.PI / 2f) * num780;
					Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Vector2 end = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Texture2D laserTex = TextureAssets.Extra[ExtrasID.CultistLightingArc].Value;

			for (int layer = 0; layer < 3; layer++)
			{
				Vector2 scale;
				switch (layer)
				{
					case 0:
						scale = new Vector2(Projectile.scale) * 0.6f;
						DelegateMethods.c_1 = new Color(115, 204, 219, 0) * 0.5f;
						break;
					case 1:
						scale = new Vector2(Projectile.scale) * 0.4f;
						DelegateMethods.c_1 = new Color(113, 251, 255, 0) * 0.5f;
						break;
					default:
						scale = new Vector2(Projectile.scale) * 0.2f;
						DelegateMethods.c_1 = new Color(255, 255, 255, 0) * 0.5f;
						break;
				}

				DelegateMethods.f_1 = 1f;

				for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
				{
					if (Projectile.oldPos[i] == Vector2.Zero)
						continue;

					Vector2 start = Projectile.oldPos[i] + new Vector2(Projectile.width, Projectile.height) / 2f
						+ Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;

					Vector2 end2 = Projectile.oldPos[i - 1] + new Vector2(Projectile.width, Projectile.height) / 2f
						+ Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;

					Utils.DrawLaser(Main.spriteBatch, laserTex, start, end2, scale, DelegateMethods.LightningLaserDraw);
				}

				if (Projectile.oldPos[0] != Vector2.Zero)
				{
					Vector2 start = Projectile.oldPos[0] + new Vector2(Projectile.width, Projectile.height) / 2f
						+ Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;

					Utils.DrawLaser(Main.spriteBatch, laserTex, start, end, scale, DelegateMethods.LightningLaserDraw);
				}
			}

			return false;
		}
	}
}