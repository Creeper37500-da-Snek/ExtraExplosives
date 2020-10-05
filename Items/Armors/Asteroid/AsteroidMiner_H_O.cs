using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExtraExplosives.Items.Armors.Asteroid
{
    [AutoloadEquip(EquipType.Head)]
    public class AsteroidMiner_H_O : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orichalcum Asteroid Miner Helm");
            Tooltip.SetDefault("\n" +
                "3% Bomb Damage and " +
                "6% Blast Radius");
        }

        public override void SetDefaults()
        {
            item.height = 18;
            item.width = 18;
            item.value = Item.buyPrice(0, 0, 70, 50);
            item.rare = ItemRarityID.LightRed;
            item.defense = 12;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<AsteroidMiner_B_O>() && legs.type == ModContent.ItemType<AsteroidMiner_L_O>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "\n" +
                "6% Bomb Damage\n" +
                "6% Blast Radius\n" +
                "5% damage\n" +
                "3% critical strike chance\n" +
                "20% chance to drop ores twice on bomb explosion (EE bombs only)";
            player.EE().RadiusMulti += .06f;
            player.EE().DamageMulti += .06f;
            player.allDamage += .05f;
            player.EE().ExplosiveCrit += 3;
            player.EE().DropOresTwice = true;
            player.EE().dropChanceOre = .2f;
        }

        public override void UpdateEquip(Player player)
        {
            player.EE().RadiusMulti += .06f;
            player.EE().DamageMulti += .03f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.OrichalcumBar, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

    }
}