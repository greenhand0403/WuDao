using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;

namespace WuDao.Content.Projectiles.Melee
{
    public class OceanBlueSwordProjectile : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/OceanBlueSword";
        private SpriteSheet _sheet;
        private SpriteAnimator _anim = new SpriteAnimator();

        // 选用哪一列（0..N-1），每列就是一个不同外观的弹幕
        public int ColumnIndex = 2;
        public int RowsPerColumn = 11; // 你的纵向帧数
        public int ColumnWidth = 64;   // 你的列宽（像素）
        public bool EqualCellHeight = true; // 若高度平均分整张图就设 false
        public override void SetStaticDefaults()
        {
            // 告诉 tML：我们自己画，所以不需要 Main.projFrames
        }

        public override void SetDefaults()
        {
            Projectile.width = 25;   // 碰撞箱
            Projectile.height = 25;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;

            _sheet = SpriteSheets.Get(SpriteAtlasId.Effect1);
            // _anim = new SpriteAnimator();
            // int[] framesPerCol = { 11, 8, 9, 7, 12, 6, 9, 8, 5 }; // 每列的帧数

            // _sheet = SpriteSheet
            //     .FromTexture("WuDao/Assets/Effect1")
            //     .BuildVerticalColumns(
            //         columns: 9,
            //         framesPerColumn: framesPerCol,
            //         columnWidth: 64,
            //         frameHeight: 64,
            //         start: new Point(0, 0),
            //         colSpacing: Point.Zero
            //     );
        }
        private int noTileTimer;

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            noTileTimer = 6;               // 约 0.1 秒
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (noTileTimer > 0)
            {
                noTileTimer--;
                if (noTileTimer == 0)
                    Projectile.tileCollide = true;
            }

            // 取当前列的帧数来更新动画
            int frameCount = _sheet.Sprites[ColumnIndex].FrameCount;
            _anim.Update(ticksPerFrame: 4, frameCount: frameCount, loop: true);

            Projectile.rotation = Projectile.velocity.ToRotation();

            // var center = Projectile.Center;
            // Projectile.width = 50;
            // Projectile.height = 50;
            // Projectile.Center = center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            _sheet.Draw(ColumnIndex, _anim.Frame, Projectile.Center, lightColor, Projectile.rotation, Projectile.scale*2);
            return false;
        }

        // public override void ModifyDamageHitbox(ref Rectangle hitbox)
        // {
        //     // 以当前中心为基准，重设为 50×50
        //     Point center = hitbox.Center;
        //     hitbox.Width = 6;
        //     hitbox.Height = 6;
        //     hitbox.X = center.X - hitbox.Width / 2;
        //     hitbox.Y = center.Y - hitbox.Height / 2;
        // }
    }
}