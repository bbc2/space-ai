using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal partial class Program
    {
        private enum Res
        {
            Cobalt,
            Gold,
            Gravel,
            Ice,
            Iron,
            Magnesium,
            Nickel,
            Platinum,
            Scrap,
            Silicon,
            Silver,
            Stone,
            Uranium,
        };

        private class Product
        {
            public Res Id { get; }
            public double Yield { get; }

            public Product(Res id, double yield)
            {
                Id = id;
                Yield = yield;
            }
        }

        private class ResourceInfo
        {
            private readonly List<Res> _ores = new List<Res>
            {
                Res.Cobalt,
                Res.Gold,
                Res.Ice,
                Res.Iron,
                Res.Magnesium,
                Res.Nickel,
                Res.Platinum,
                Res.Silicon,
                Res.Silver,
                Res.Scrap,
                Res.Stone,
                Res.Uranium,
            };

            private readonly List<Res> _ingots = new List<Res>
            {
                Res.Cobalt,
                Res.Gold,
                Res.Iron,
                Res.Magnesium,
                Res.Nickel,
                Res.Platinum,
                Res.Silicon,
                Res.Silver,
                Res.Uranium,
            };

            public string Name { get; }
            public MyItemType? OreType { get; }
            public MyItemType? IngotType { get; }
            internal List<Product> Products { get; }

            public ResourceInfo(Res id, string name, List<Product> products)
            {
                Name = name;
                Products = products;

                if (_ores.Contains(id))
                {
                    OreType = MyItemType.MakeOre(name);
                }

                if (_ingots.Contains(id))
                {
                    IngotType = MyItemType.MakeIngot(name);
                }
            }
        }

        private static void InitScreen(IMyTextSurface screen)
        {
            screen.ContentType = ContentType.TEXT_AND_IMAGE;
            screen.WriteText("");
            screen.FontColor = new Color(83, 228, 37, 200);
            screen.Font = "Monospace";
        }

        public class OreManager
        {
            private readonly MyGridProgram _grid;

            private static readonly Dictionary<Res, ResourceInfo> Info = new Dictionary<Res, ResourceInfo>
            {
                {
                    Res.Cobalt,
                    new ResourceInfo(
                        id: Res.Cobalt,
                        name: "Cobalt",
                        products: new List<Product> {new Product(id: Res.Cobalt, yield: .3)}
                    )
                },
                {
                    Res.Gold,
                    new ResourceInfo(
                        id: Res.Gold,
                        name: "Gold",
                        products: new List<Product> {new Product(id: Res.Gold, yield: .01)}
                    )
                },
                {
                    Res.Ice,
                    new ResourceInfo(
                        id: Res.Ice,
                        name: "Ice",
                        products: new List<Product>()
                    )
                },
                {
                    Res.Iron,
                    new ResourceInfo(
                        id: Res.Iron,
                        name: "Iron",
                        products: new List<Product> {new Product(id: Res.Iron, yield: .7)}
                    )
                },
                {
                    Res.Magnesium,
                    new ResourceInfo(
                        id: Res.Magnesium,
                        name: "Magnesium",
                        products: new List<Product> {new Product(id: Res.Magnesium, yield: .007)}
                    )
                },
                {
                    Res.Nickel,
                    new ResourceInfo(
                        id: Res.Nickel,
                        name: "Nickel",
                        products: new List<Product> {new Product(id: Res.Nickel, yield: .4)}
                    )
                },
                {
                    Res.Platinum,
                    new ResourceInfo(
                        id: Res.Platinum,
                        name: "Platinum",
                        products: new List<Product> {new Product(id: Res.Platinum, yield: .005)}
                    )
                },
                {
                    Res.Scrap,
                    new ResourceInfo(
                        id: Res.Scrap,
                        name: "Scrap",
                        products: new List<Product> {new Product(id: Res.Iron, yield: .7)}
                    )
                },
                {
                    Res.Stone,
                    new ResourceInfo(
                        id: Res.Stone,
                        name: "Stone",
                        products: new List<Product>
                        {
                            new Product(id: Res.Gravel, yield: .014),
                            new Product(id: Res.Iron, yield: .03),
                            new Product(id: Res.Nickel, yield: .0024),
                            new Product(id: Res.Silicon, yield: .004),
                        }
                    )
                },
                {
                    Res.Silicon,
                    new ResourceInfo(
                        id: Res.Silicon,
                        name: "Silicon",
                        products: new List<Product> {new Product(id: Res.Silicon, yield: .7)}
                    )
                },
                {
                    Res.Silver,
                    new ResourceInfo(
                        id: Res.Silver,
                        name: "Silver",
                        products: new List<Product> {new Product(id: Res.Silver, yield: .1)}
                    )
                },
                {
                    Res.Uranium,
                    new ResourceInfo(
                        id: Res.Uranium,
                        name: "Uranium",
                        products: new List<Product> {new Product(id: Res.Uranium, yield: .01)}
                    )
                },
            };

            public OreManager(MyGridProgram grid)
            {
                _grid = grid;
            }

            internal void Init()
            {
            }

            private static void AddInventoryCounts(
                IEnumerable<Res> resources,
                IMyInventory inventory,
                IDictionary<Res, MyFixedPoint> counts,
                bool ore)
            {
                foreach (var resource in resources)
                {
                    var typeOpt = ore ? Info[resource].OreType : Info[resource].IngotType;
                    if (!typeOpt.HasValue)
                    {
                        continue;
                    }

                    var type = typeOpt.Value;
                    MyFixedPoint value;
                    counts.TryGetValue(resource, out value);
                    counts[resource] = value + inventory.GetItemAmount(type);
                }
            }

            private class Counts
            {
                public Dictionary<Res, MyFixedPoint> Ores { get; }
                public Dictionary<Res, MyFixedPoint> Ingots { get; }
                public MyFixedPoint Volume { get; }
                public MyFixedPoint VolumeOccupied { get; }

                public Counts(Dictionary<Res, MyFixedPoint> ores, Dictionary<Res, MyFixedPoint> ingots,
                    MyFixedPoint volume, MyFixedPoint volumeOccupied)
                {
                    Ores = ores;
                    Ingots = ingots;
                    Volume = volume;
                    VolumeOccupied = volumeOccupied;
                }
            }

            private Counts GetCounts(IEnumerable<Res> resources)
            {
                var ores = new Dictionary<Res, MyFixedPoint>();
                var ingots = new Dictionary<Res, MyFixedPoint>();
                MyFixedPoint volume = 0;
                MyFixedPoint volumeOccupied = 0;
                {
                    var blocks = new List<IMyTerminalBlock>();
                    _grid.GridTerminalSystem.GetBlocks(blocks);

                    foreach (var block in blocks.Where(block => block.IsSameConstructAs(_grid.Me)))
                    {
                        if (block.InventoryCount == 1)
                        {
                            AddInventoryCounts(resources: resources, inventory: block.GetInventory(), counts: ores,
                                ore: true);
                            AddInventoryCounts(resources: resources, inventory: block.GetInventory(),
                                counts: ingots,
                                ore: false);
                            volume += block.GetInventory().MaxVolume;
                            volumeOccupied += block.GetInventory().CurrentVolume;
                        }
                        else if (block is IMyProductionBlock)
                        {
                            var prod = (IMyProductionBlock) block;
                            AddInventoryCounts(resources: resources, inventory: prod.InputInventory, counts: ores,
                                ore: true);
                            AddInventoryCounts(resources: resources, inventory: prod.OutputInventory, counts: ores,
                                ore: true);
                            AddInventoryCounts(resources: resources, inventory: prod.InputInventory, counts: ingots,
                                ore: false);
                            AddInventoryCounts(resources: resources, inventory: prod.OutputInventory,
                                counts: ingots,
                                ore: false);
                            volume += prod.InputInventory.MaxVolume;
                            volume += prod.OutputInventory.MaxVolume;
                            volumeOccupied += prod.InputInventory.CurrentVolume;
                            volumeOccupied += prod.OutputInventory.CurrentVolume;
                        }
                    }
                }
                return new Counts(ores: ores, ingots: ingots, volume: volume, volumeOccupied: volumeOccupied);
            }

            private static void ShowCounts(IMyTextSurface screen, string title, Dictionary<Res, MyFixedPoint> counts)
            {
                screen.ContentType = ContentType.TEXT_AND_IMAGE;
                screen.WriteText($"{title}\n");
                foreach (var entry in counts)
                {
                    screen.WriteText($"{Info[entry.Key].Name,-10} {Float(entry.Value)}\n", append: true);
                }
            }

            private static void ShowSummary(IMyTextSurface screen, Counts counts)
            {
                screen.ContentType = ContentType.TEXT_AND_IMAGE;
                screen.WriteText("INVENTORY\n");
                const string name = "Volume";
                screen.WriteText($"{name,-10} {Float(counts.VolumeOccupied)} / {Float(counts.Volume)}\n",
                    append: true);

                var projected = new Dictionary<Res, MyFixedPoint>();
                foreach (var entry in counts.Ores)
                {
                    var oreCount = entry.Value;

                    foreach (var product in Info[entry.Key].Products)
                    {
                        MyFixedPoint productCount;
                        projected.TryGetValue(product.Id, out productCount);
                        projected[product.Id] = productCount + (MyFixedPoint) product.Yield * oreCount;
                    }
                }

                screen.WriteText("\nREFINED\n", append: true);

                foreach (var entry in Info)
                {
                    var resource = entry.Key;
                    var info = entry.Value;

                    if (!info.IngotType.HasValue)
                    {
                        continue;
                    }

                    MyFixedPoint projectedCount;
                    projected.TryGetValue(resource, out projectedCount);
                    MyFixedPoint currentCount;
                    counts.Ingots.TryGetValue(resource, out currentCount);

                    var totalCount = projectedCount + currentCount;
                    screen.WriteText($"{info.Name,-10} {Float(currentCount)} / {Float(totalCount)}\n",
                        append: true);
                }

                screen.WriteText("\nOTHER\n", append: true);

                foreach (var entry in Info)
                {
                    var resource = entry.Key;
                    var info = entry.Value;

                    if (!info.OreType.HasValue || info.Products.Count > 0)
                    {
                        continue;
                    }

                    MyFixedPoint count;
                    counts.Ores.TryGetValue(resource, out count);
                    screen.WriteText($"{info.Name,-10} {Float(count)}\n", append: true);
                }
            }

            internal void Update()
            {
                var blocks = new List<IMyTerminalBlock>();
                _grid.GridTerminalSystem.SearchBlocksOfName("(bim#ores)", blocks,
                    block => block.IsSameConstructAs(_grid.Me) && block is IMyTextSurface);
                var oreScreens = blocks.Cast<IMyTextSurface>().ToList();

                blocks = new List<IMyTerminalBlock>();
                _grid.GridTerminalSystem.SearchBlocksOfName("(bim#summary)", blocks,
                    block => block.IsSameConstructAs(_grid.Me) && block is IMyTextSurface);
                var summaryScreens = blocks.Cast<IMyTextSurface>().ToList();

                var counts = GetCounts(Info.Keys);

                foreach (var screen in oreScreens)
                {
                    InitScreen(screen);
                    ShowCounts(screen, "ORES", counts.Ores);
                }

                foreach (var screen in summaryScreens)
                {
                    InitScreen(screen);
                    ShowSummary(screen, counts);
                }
            }
        }
    }
}