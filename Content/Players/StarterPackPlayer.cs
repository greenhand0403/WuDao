using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Items;

namespace WuDao.Content.Players
{
    public class StarterPackPlayer : ModPlayer
    {
        // 1.4+：当“创建角色”时（以及中度硬核死亡后重新生成背包时）调用
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            // 这里返回要添加到背包的 Item 列表
            var list = new List<Item>();

            // 示例：给 1 个你自定义的物品 + 50 木头
            // （A）自定义物品
            var myItem = new Item();
            myItem.SetDefaults(ModContent.ItemType<Cookbook>());
            myItem.stack = 1;
            list.Add(myItem);
            myItem = new Item();
            myItem.SetDefaults(ModContent.ItemType<FoodLogItem>()); 
            myItem.stack = 1;
            list.Add(myItem);
            // （B）原版物品
            // var wood = new Item(ItemID.Wood) { stack = 50 };
            // list.Add(wood);

            // // （可选）只在创建角色时给，高级玩法：判断 mediumCoreDeath
            // if (mediumCoreDeath)
            // {
            //     // 中度角色死亡后重生背包，若你不想重复给特殊物品，可以删掉或更换
            //     // 例如只给少量木头做保底
            //     return new List<Item> { new Item(ItemID.Wood) { stack = 10 } };
            // }

            return list;
        }
    }
}