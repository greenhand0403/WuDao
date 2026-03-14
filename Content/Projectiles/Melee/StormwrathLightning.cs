using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Melee
{
	/// <summary>
	/// 模仿原版 466（CultistBossLightningOrbArc）的自定义闪电珠弧。
	/// 行为上仍然是“从出生点不断向前随机折线延伸”，
	/// 只是绘制与命中逻辑抽到 LightningHelper 中，提升可读性与复用性。
	/// </summary>
	public class StormwrathLightning : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CultistBossLightningOrbArc;

		// ===== 常量区：把魔法数字变成有名字的参数 =====
		private const int TrailCacheLength = 10;
		private const int TrailingMode = 1;
		private const int ExtraUpdates = 4;

		/// <summary> 每隔多少个额外更新步，重新随机一次前进方向。 </summary>
		private const int DirectionUpdateInterval = ExtraUpdates * 2;

		/// <summary> localAI[0] 允许累积的横向偏移范围。 </summary>
		private const float MaxLateralOffset = 40f;

		/// <summary> 随机选新方向时，最多尝试多少次。 </summary>
		private const int MaxDirectionAttempts = 100;

		/// <summary> 停住后，多久检查一次轨迹是否已完全收缩。 </summary>
		private const int CollapseCheckInterval = ExtraUpdates * 2;

		/// <summary> 金色雷光环境光。 </summary>
		private static readonly Vector3 ZenitsuLightColor = new(0.95f, 0.78f, 0.18f);

		// ===== 语义化访问器：把 ai/localAI 槽位命名 =====
		/// <summary> 闪电主轴角度。原版用于把“局部方向”旋转到世界方向。 </summary>
		private ref float BaseAngle => ref Projectile.ai[0];

		/// <summary> 随机种子。每次重新选方向后会写回，用于保持轨迹随机但稳定。 </summary>
		private ref float RandomSeed => ref Projectile.ai[1];

		/// <summary> 横向累计偏移，用来限制闪电不要向两侧飘得太夸张。 </summary>
		private ref float LateralOffset => ref Projectile.localAI[0];

		/// <summary> 终止标记。>= 1 时表示已经进入“停住/收缩”阶段。 </summary>
		private ref float StopState => ref Projectile.localAI[1];

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = TrailCacheLength;
			ProjectileID.Sets.TrailingMode[Type] = TrailingMode;
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

			Projectile.extraUpdates = ExtraUpdates;
			Projectile.timeLeft = 120 * (Projectile.extraUpdates + 1);
			Projectile.alpha = 255;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void OnSpawn(IEntitySource source)
		{
			// 保证有一个稳定的随机种子。
			if (RandomSeed == 0f)
				RandomSeed = Main.rand.Next(int.MinValue, int.MaxValue);

			// 初始朝向与速度一致。
			if (Projectile.velocity != Vector2.Zero)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			// 音效
			// SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Thunder_0"), Projectile.Center);
			SoundEngine.PlaySound(SoundID.Thunder, Projectile.Center);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// 原版 466/580/686 本质上就是按 oldPos 上的矩形逐个判。
			return LightningHelper.CheckOldPosCollision(Projectile, projHitbox, targetHitbox);
		}

		public override void AI()
		{
			UpdateFrameCounter();
			AddLightningLight();

			if (Projectile.velocity == Vector2.Zero)
			{
				HandleStoppedState();
				return;
			}

			HandleMovingState();
		}

		private void UpdateFrameCounter()
		{
			Projectile.frameCounter++;
		}

		private void AddLightningLight()
		{
			Lighting.AddLight(Projectile.Center, ZenitsuLightColor);
		}

		/// <summary>
		/// 闪电已经停止延伸时的逻辑：
		/// 1. 周期性检查 oldPos 是否已全部收缩到同一点
		/// 2. 继续喷少量电火花/烟尘，维持残留感
		/// </summary>
		private void HandleStoppedState()
		{
			if (ShouldKillAfterTrailCollapse())
			{
				Projectile.Kill();
				return;
			}

			SpawnStoppedDusts();
		}

		private bool ShouldKillAfterTrailCollapse()
		{
			if (Projectile.frameCounter < CollapseCheckInterval)
				return false;

			Projectile.frameCounter = 0;

			bool trailCollapsed = true;
			for (int trailIndex = 1; trailIndex < Projectile.oldPos.Length; trailIndex++)
			{
				if (Projectile.oldPos[trailIndex] != Projectile.oldPos[0])
				{
					trailCollapsed = false;
					break;
				}
			}

			return trailCollapsed;
		}

		private void SpawnStoppedDusts()
		{
			if (Main.rand.Next(Projectile.extraUpdates) != 0)
				return;

			// 左右两侧喷少量金色电火花。
			for (int sparkIndex = 0; sparkIndex < 2; sparkIndex++)
			{
				float sideAngle = Projectile.rotation + (Main.rand.NextBool() ? -1f : 1f) * MathHelper.PiOver2;
				float sparkSpeed = (float)Main.rand.NextDouble() * 0.8f + 1f;
				Vector2 sparkVelocity = sideAngle.ToRotationVector2() * sparkSpeed;

				int dustIndex = Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, sparkVelocity.X, sparkVelocity.Y);
				Main.dust[dustIndex].noGravity = true;
				Main.dust[dustIndex].scale = 1.2f;
				Main.dust[dustIndex].color = new Color(255, 220, 90);
			}

			// 偶尔补一点上扬烟雾，增强放电残留感。
			if (Main.rand.NextBool(5))
			{
				Vector2 smokeOffset = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2)
					* ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;

				int smokeIndex = Dust.NewDust(
					Projectile.Center + smokeOffset - Vector2.One * 4f,
					8, 8, DustID.Smoke, 0f, 0f, 100, default, 1.5f);

				Main.dust[smokeIndex].velocity *= 0.5f;
				Main.dust[smokeIndex].velocity.Y = -Math.Abs(Main.dust[smokeIndex].velocity.Y);
			}
		}

		/// <summary>
		/// 闪电仍在延伸时：
		/// 每隔一段时间重新随机方向一次，并限制其横向摆动范围。
		/// </summary>
		private void HandleMovingState()
		{
			if (Projectile.frameCounter < DirectionUpdateInterval)
				return;

			Projectile.frameCounter = 0;

			float speed = Projectile.velocity.Length();
			UnifiedRandom random = new UnifiedRandom((int)RandomSeed);

			int attemptCount = 0;
			Vector2 localDirection = -Vector2.UnitY;

			while (true)
			{
				int nextSeed = random.Next();
				RandomSeed = nextSeed;

				// 取 0~99 的随机值，再映射成一组离散局部方向。
				int randomDirectionIndex = nextSeed % 100;

				Vector2 candidateLocalDirection = GetCandidateLocalDirection(randomDirectionIndex);

				// 不能向“下方”生长；局部 Y 必须 <= 0。
				if (candidateLocalDirection.Y > 0f)
					goto RejectDirection;

				// 限制横向累计偏移，防止闪电无限向左右散开。
				float nextLateralOffset = LateralOffset +
					candidateLocalDirection.X * (Projectile.extraUpdates + 1) * 2f * speed;

				if (nextLateralOffset > MaxLateralOffset || nextLateralOffset < -MaxLateralOffset)
					goto RejectDirection;

				localDirection = candidateLocalDirection;
				break;

			RejectDirection:
				attemptCount++;
				if (attemptCount >= MaxDirectionAttempts)
				{
					// 100 次都没找到合法方向，就终止延伸。
					Projectile.velocity = Vector2.Zero;
					StopState = 1f;
					break;
				}
			}

			if (Projectile.velocity == Vector2.Zero)
				return;

			LateralOffset += localDirection.X * (Projectile.extraUpdates + 1) * 2f * speed;
			Projectile.velocity = localDirection.RotatedBy(BaseAngle + MathHelper.PiOver2) * speed;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		/// <summary>
		/// 这个方法本质上就是把原版那组离散方向表语义化。
		/// 你后续如果想微调“折线感”，只需要改这里。
		/// </summary>
		private static Vector2 GetCandidateLocalDirection(int index)
		{
			Vector2 direction = index switch
			{
				< 20 => new Vector2(-1f, -1f),
				< 40 => new Vector2(-0.5f, -1f),
				< 60 => new Vector2(0f, -1f),
				< 80 => new Vector2(0.5f, -1f),
				_ => new Vector2(1f, -1f),
			};

			direction.Normalize();
			return direction;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			LightningHelper.DrawLightningFromOldPos(
				projectile: Projectile,
				spriteBatch: Main.spriteBatch,
				palette: LightningPalettes.ZenitsuGold);

			return false;
		}
	}
}