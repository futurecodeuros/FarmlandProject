using System;
using System.Collections.Generic;

namespace FarmlandProject
{
    internal class Program
    {
        static void Main()
        {
            using var game = new Game1();
            game.Run();
        }

        static void RunConsole()
        {
            try
            {
                var player = new Player();
               
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("===MY FARM===");
                    Console.WriteLine();
                    Console.WriteLine("1. Plant crop");
                    Console.WriteLine("2. Harvest crop");
                    Console.WriteLine("3. Buy animal");
                    Console.WriteLine("4. Sell");
                    Console.WriteLine("5. Show status");
                    Console.WriteLine("6. Exit");
                    Console.WriteLine("7. Sleep");
                    Console.WriteLine();
                    Console.Write("Enter choice (1-7): ");

                    string input = Console.ReadLine() ?? string.Empty;
                    if (!int.TryParse(input, out int userInput))
                    {
                        Console.WriteLine("Invalid input. Please enter a number between 1 and 7.");
                        Console.WriteLine();
                        continue;
                    }

                    switch (userInput)
                    {
                        case 1:
                            Console.WriteLine("Which type of crop do you want to plant?");
                            Console.WriteLine();
                            Console.WriteLine("1. Carrot");
                            Console.WriteLine("2. Potato");
                            Console.WriteLine("3. Tomato");
                            string input1 = Console.ReadLine() ?? string.Empty;
                            if (!int.TryParse(input1, out int userInput1))
                            {
                                Console.WriteLine("Invalid input. Please enter a number between 1 and 3.");
                                Console.WriteLine();
                                continue;
                            }
                            
                            switch (userInput1)
                            {
                                case 1:
                                    player.farm.Plant("Carrot");
                                    break;
                                case 2:
                                    player.farm.Plant("Potato");
                                    break;
                                case 3:
                                    player.farm.Plant("Tomato");
                                    break;
                                default:
                                    Console.WriteLine("Invalid crop choice.");
                                    break;
                            }
                            break;
                        case 2:
                            Console.WriteLine("Which type of crop do you want to harvest?");
                            Console.WriteLine();
                            Console.WriteLine("1. Carrot");
                            Console.WriteLine("2. Potato");
                            Console.WriteLine("3. Tomato");
                            Console.WriteLine("4. Harvest all grown crops");
                            string input2 = Console.ReadLine() ?? string.Empty;
                            if (!int.TryParse(input2, out int userInput2))
                            {
                                Console.WriteLine("Invalid input. Please enter a number between 1 and 4.");
                                Console.WriteLine();
                                continue;
                            }

                            switch (userInput2)
                            {
                                case 1:
                                    player.farm.Harvest("Carrot");
                                    break;
                                case 2:
                                    player.farm.Harvest("Potato");
                                    break;
                                case 3:
                                    player.farm.Harvest("Tomato");
                                    break;
                                case 4:
                                    player.farm.HarvestAll();
                                    break;
                                default:
                                    Console.WriteLine("Invalid crop choice.");
                                    break;
                            }
                            break;
                        case 3:
                            Console.WriteLine("Which animal do you want to buy?");
                            Console.WriteLine("1. Chicken - 100 Gold");
                            Console.WriteLine("2. Cow - 150 Gold");
                            Console.WriteLine("3. Pig - 200 Gold");
                            if (int.TryParse(Console.ReadLine(), out int animalChoice))
                            {
                                switch (animalChoice)
                                {
                                    case 1:
                                        player.farm.BuyAnimal("Chicken");
                                        break;
                                    case 2:
                                        player.farm.BuyAnimal("Cow");
                                        break;
                                    case 3:
                                        player.farm.BuyAnimal("Pig");
                                        break;
                                    default:
                                        Console.WriteLine("Invalid animal choice.");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid animal choice.");
                            }
                            break;
                        case 4:
                            Console.WriteLine("What do you want to sell?");
                            Console.WriteLine("1. Harvested crop");
                            Console.WriteLine("2. Animal");
                            if (!int.TryParse(Console.ReadLine(), out int sellChoice))
                            {
                                Console.WriteLine("Invalid sell choice.");
                                break;
                            }

                            Console.WriteLine("Choose a type:");
                            Console.WriteLine("1. Carrot / Chicken");
                            Console.WriteLine("2. Potato / Cow");
                            Console.WriteLine("3. Tomato / Pig");
                            if (!int.TryParse(Console.ReadLine(), out int sellType))
                            {
                                Console.WriteLine("Invalid type choice.");
                                break;
                            }

                            if (sellChoice == 1)
                            {
                                switch (sellType)
                                {
                                    case 1:
                                        player.farm.SellCrop("Carrot");
                                        break;
                                    case 2:
                                        player.farm.SellCrop("Potato");
                                        break;
                                    case 3:
                                        player.farm.SellCrop("Tomato");
                                        break;
                                    default:
                                        Console.WriteLine("Invalid crop choice.");
                                        break;
                                }
                            }
                            else if (sellChoice == 2)
                            {
                                switch (sellType)
                                {
                                    case 1:
                                        player.farm.SellAnimal("Chicken");
                                        break;
                                    case 2:
                                        player.farm.SellAnimal("Cow");
                                        break;
                                    case 3:
                                        player.farm.SellAnimal("Pig");
                                        break;
                                    default:
                                        Console.WriteLine("Invalid animal choice.");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid sell choice.");
                            }
                            break;
                        case 5:
                            player.farm.ShowStatus();
                            break;
                        case 6:
                            Console.WriteLine("Exiting program. Goodbye!");
                            exit = true;
                            break;
                        case 7:
                            player.time.Tick();
                            Console.WriteLine("Good night, sweet dreams..");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please select a number between 1 and 7.");
                            break;
                    }

                    if (!exit)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        Console.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
   
    public class Player
    {
        public Time time { get; private set; }
        public List<Crop> crops { get; private set; }
        public List<Crop> harvestedCrops { get; private set; }
        public List<Animal> animals { get; private set; }
        public Money money { get; private set; }
        public Farm farm { get; private set; }


        public Player()
        {
            time = new Time();
            crops = new List<Crop>();
            harvestedCrops = new List<Crop>();
            animals = new List<Animal>();
            money = new Money();
            farm = new Farm(crops, harvestedCrops, animals, money, time);

        }
   

         public class Time // also sleep
    {
        private int _day = 0;
        public int Day => _day;
        public void Tick()
        {
            _day += 1;
        }

    }
        public class Farm // the farm class with the showstatus
        {
            private readonly List<Crop> _crops;
            private readonly List<Crop> _harvestedCrops;
            private readonly List<Animal> _animals;
            private readonly Money _money;
            private readonly Time _time1;
            public Farm(List<Crop> crops, List<Crop> harvestedCrops, List<Animal> animals, Money money, Time time2)
            {
                _crops = crops ?? throw new ArgumentNullException(nameof(crops));
                _harvestedCrops = harvestedCrops ?? throw new ArgumentNullException(nameof(harvestedCrops));
                _animals = animals ?? throw new ArgumentNullException(nameof(animals));
                _money = money ?? throw new ArgumentNullException(nameof(money));
                _time1 = time2 ?? throw new ArgumentNullException(nameof(time2));
            }

            public void Plant(string type)
            {
                _crops.Add(new Crop(type, _time1.Day));
                Console.WriteLine($"You have planted a {type}.");
            }

            public void Harvest(string type)
            {
                Crop? crop = _crops.Find(c =>
                    c.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

                if (crop is null)
                {
                    Console.WriteLine($"You have no planted {type}s to harvest.");
                    return;
                }

                if (!crop.IsGrown(_time1.Day))
                {
                    Console.WriteLine($"The {type} has not grown yet.");
                    return;
                }

                _crops.Remove(crop);
                _harvestedCrops.Add(crop);
                Console.WriteLine($"You have harvested a {type}.");
            }

            public void HarvestAll()
            {
                List<Crop> grownCrops = _crops.FindAll(crop => crop.IsGrown(_time1.Day));
                _crops.RemoveAll(crop => crop.IsGrown(_time1.Day));
                _harvestedCrops.AddRange(grownCrops);
                int harvestedCount = grownCrops.Count;

                if (harvestedCount == 0)
                {
                    Console.WriteLine("There are no grown crops to harvest.");
                    return;
                }

                Console.WriteLine($"You have harvested {harvestedCount} crop(s).");
            }

            public void BuyAnimal(string type)
            {
                Animal animal = new Animal(type);
                if (!_money.Spend(animal.Price))
                {
                    Console.WriteLine($"You need {animal.Price} Gold to buy a {type}.");
                    return;
                }

                _animals.Add(animal);
                Console.WriteLine($"You bought a {type} for {animal.Price} Gold.");
                animal.MakeSound();
            }

            public void SellAnimal(string type)
            {
                Animal? animal = _animals.Find(item =>
                    item.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

                if (animal is null)
                {
                    Console.WriteLine($"You do not have a {type} to sell.");
                    return;
                }

                _animals.Remove(animal);
                _money.Add(animal.Price);
                Console.WriteLine($"You sold a {type} for {animal.Price} Gold.");
            }

            public int KillAllAnimals()
            {
                int killedCount = _animals.Count;
                _animals.Clear();
                return killedCount;
            }

            public void SellCrop(string type)
            {
                Crop? crop = _harvestedCrops.Find(item =>
                    item.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

                if (crop is null)
                {
                    Console.WriteLine($"You do not have a harvested {type} to sell.");
                    return;
                }

                _harvestedCrops.Remove(crop);
                _money.Add(crop.Price);
                Console.WriteLine($"You sold a {type} for {crop.Price} Gold.");
            }

            public void ShowStatus()
            {
                Console.WriteLine("Showing farm status...");
                Console.WriteLine($"Planted = {_crops.Count}");
                Console.WriteLine($"Harvested inventory = {_harvestedCrops.Count}");
                Console.WriteLine($"Animals = {_animals.Count}");
                Console.WriteLine($"Money = {_money.TotalGold} Gold");
                Console.WriteLine($"Day = {_time1.Day}");

                foreach (Crop crop in _crops)
                {
                    string status = crop.IsGrown(_time1.Day) ? "grown" : "growing";
                    Console.WriteLine($"- {crop.Type}: {status}");
                }

                Console.WriteLine("Harvested crops:");
                foreach (Crop crop in _harvestedCrops)
                {
                    Console.WriteLine($"- {crop.Type}: {crop.Price} Gold");
                }

                Console.WriteLine("Animals:");
                foreach (Animal animal in _animals)
                {
                    Console.WriteLine($"- {animal.Type}: {animal.Price} Gold");
                    animal.MakeSound();
                }

            }
        }

        public class Crop 
        {
            public string Type { get; }
            public int DayPlanted { get; }
            public int Price { get; }

            public Crop(string type, int dayPlanted)
            {
                Type = type;
                DayPlanted = dayPlanted;
                Price = type switch
                {
                    "Carrot" => 20,
                    "Potato" => 35,
                    "Tomato" => 50,
                    _ => 0
                };
            }

            public bool IsGrown(int currentDay)
            {
                return currentDay >= DayPlanted + 3;
            }


        }

        public class Animal
        {
            public string Type { get; }
            public int Price { get; }
            public string Sound { get; }

            public Animal(string type)
            {
                Type = type;
                Price = type switch
                {
                    "Chicken" => 100,
                    "Cow" => 150,
                    "Pig" => 200,
                    _ => 0
                };
                Sound = type switch
                {
                    "Chicken" => "Cluck!",
                    "Cow" => "Moo!",
                    "Pig" => "Oink!",
                    _ => "..."
                };
            }

            public void MakeSound()
            {
                Console.WriteLine($"{Type}: {Sound}");

                try
                {
                    Console.Beep();
                }
                catch (PlatformNotSupportedException)
                {
                }
            }
        }

        public class Money
        {
            private int _totalGold = 500;
            public int TotalGold => _totalGold;

            public bool Spend(int amount)
            {
                if (amount < 0 || _totalGold < amount)
                {
                    return false;
                }

                _totalGold -= amount;
                return true;
            }

            public void Add(int amount)
            {
                if (amount > 0)
                {
                    _totalGold += amount;
                }
            }


        }
    }
}
