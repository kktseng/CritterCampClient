using System;

namespace CritterCamp.Core.Lib {
    // A class to hold all the store data (which critters/upgrades are unlocked, available, pricing)
    public static class StoreData {
        public static string[] UnlockedCritters = {"pig"};
        public static StoreItem[] AvailableCritters = {};
        public static int[] GameUpgradePrices;

    }

    public class StoreItem {
        public string Name;
        public int Price;

        public StoreItem(string name, int price) {
            Name = name;
            Price = price;
        }
    }

    public class GameUpgradeStoreItem : StoreItem{
        public int Level;
        public GameUpgrade MyGameUpgrade;

        public GameUpgradeStoreItem(GameUpgrade gameUpgrade)
            : base(gameUpgrade.Name, 0) {
                Level = gameUpgrade.Level;
                MyGameUpgrade = gameUpgrade;

                if (Level < 5) { // 4 is the max level since levels are from 0-4 (5 levels max)
                    Price = StoreData.GameUpgradePrices[gameUpgrade.Level];
                } else { // level already reached the max
                    // we don't have an array entry for the level so just put 0 for the price
                    Price = 0;
                }
        }
    }
}