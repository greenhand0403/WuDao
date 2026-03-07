using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;

namespace WuDao.Content.Players
{
    public class SheRaSwordPlayer : ModPlayer
    {
        public bool IsTransformed;
        public int TransformTimer;
        public int TransformCooldown;
        public bool CanTransform => TransformCooldown <= 0 && !IsTransformed;
        // 记录当前使用哪套手动注册的贴图
        private string currentTransformSetName;

        public void StartTransformation(int duration, string equipSetName, int cooldown)
        {
            IsTransformed = true;
            TransformTimer = duration;
            TransformCooldown = cooldown;
            currentTransformSetName = equipSetName;
        }

        public override void PostUpdate()
        {
            if (TransformCooldown > 0)
                TransformCooldown--;

            if (IsTransformed)
            {
                if (TransformTimer > 0)
                {
                    TransformTimer--;
                    Player.AddBuff(ModContent.BuffType<SheRaTransformBuff>(), 2);
                }

                if (TransformTimer <= 0)
                {
                    IsTransformed = false;
                    currentTransformSetName = null;
                }
            }
        }

        public override void FrameEffects()
        {
            if (!IsTransformed || string.IsNullOrEmpty(currentTransformSetName))
                return;

            int head = EquipLoader.GetEquipSlot(Mod, currentTransformSetName, EquipType.Head);
            int body = EquipLoader.GetEquipSlot(Mod, currentTransformSetName, EquipType.Body);
            int legs = EquipLoader.GetEquipSlot(Mod, currentTransformSetName, EquipType.Legs);

            Player.head = head;
            Player.body = body;
            Player.legs = legs;
        }

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            health = StatModifier.Default;
            mana = StatModifier.Default;

            if (IsTransformed)
            {
                health *= 1.5f;
            }
        }

        public override void PostUpdateEquips()
        {
            if (!IsTransformed)
                return;

            Player.GetDamage(DamageClass.Generic) += 5f; // +500%
            Player.statDefense += 50;
            Player.endurance += 0.5f;
            Player.moveSpeed += 0.5f;
            Player.maxRunSpeed += 3f;
            Player.accRunSpeed += 3f;
            Player.lifeRegen += 30;
        }

        public override void UpdateDead()
        {
            IsTransformed = false;
            TransformTimer = 0;
            TransformCooldown = 0;
            currentTransformSetName = null;
        }

        public override void OnRespawn()
        {
            IsTransformed = false;
            TransformTimer = 0;
            TransformCooldown = 0;
            currentTransformSetName = null;
        }
    }
}