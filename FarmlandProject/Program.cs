using System;
using System.ComponentModel.Design;
using static FarmlandProject.Player;

namespace FarmlandProject
{
    internal class Program
    {
        static void Main()
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
                            player.crop.Plant();
                            break;
                        case 2:
                            player.crop.Harvest();
                            break;
                        case 3:
                            player.animals.BuyAnimal();
                            break;
                        case 4:
                            player.animals.SellAnimal();
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
        public Crop crop { get; private set; }
        public Money money { get; private set; }
        public Animal animals { get; private set; }
        public Farm farm { get; private set; }


        public Player()
        {
            time = new Time();
            crop = new Crop(time, "Carrot");
            money = new Money();
            animals = new Animal(money);
            farm = new Farm(crop, money, animals, time);

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
            private readonly Crop _crop;
            private readonly Animal _animals;
            private readonly Money _totalGold;
            private readonly Time _time1;
            public Farm(Crop crop, Money money, Animal animals, Time time2)
            {
                _crop = crop ?? throw new ArgumentNullException(nameof(crop));
                _animals = animals ?? throw new ArgumentNullException(nameof(animals));
                _totalGold = money ?? throw new ArgumentNullException(nameof(money));
                _time1 = time2 ?? throw new ArgumentNullException(nameof(time2));
            }

            public void ShowStatus()
            {
                Console.WriteLine("Showing farm status...");
                Console.WriteLine($"Planted = {_crop.Planted}");
                Console.WriteLine($"Harvested = {_crop.Harvested}");
                Console.WriteLine($"Animals = {_animals.Animals}");
                Console.WriteLine($"Money = {_totalGold._balance} Gold");
                Console.WriteLine($"Day = {_time1.Day}");

            }
        }

        public class Crop 
        {
            private int _planted = 0;
            private int _harvested = 0;
            private bool _grown = false;

            public int _dayPlanted;
            public int Planted => _planted;
            public int Harvested => _harvested;
            public bool Grown => _grown;
            private readonly Time _time;
            public string _type;

            public Crop(Time time, String T)
            {
                _time = time ?? throw new ArgumentNullException(nameof(time));
                this._type = T;
            }  // TIME SYSTEM

            public void Plant()
            {
                _planted++;
                Console.WriteLine("You have planted a crop.");
                _dayPlanted = _time.Day; // assigning to a global day so the game remembers
                _grown = false;
            }

            public void Harvest()
            {
                if (_planted < 1)
                {
                    Console.WriteLine("You have no planted crops to harvest.");
                    return;
                }
                if (_time.Day <= _dayPlanted + 3)
                {
                    Console.WriteLine("The plant has not grown yet.");
                    return;
                }


                _planted--;
                _harvested++;
                Console.WriteLine("You have harvested a crop.");
            }


        }
        public class Animal 
        {
            private readonly Money _money;
            private int _animals = 0;
            public int Animals => _animals;
            private int _animalPrice = 150;
            public int AnimalPrice => _animalPrice;

            public Animal(Money money)
            {
                _money = money ?? throw new ArgumentNullException(nameof(money));
            }

            public void BuyAnimal()
            {
                if (_money.TryBuy(_animalPrice))
                {
                    _animals++;
                    Console.WriteLine("An animal has been bought.");
                }
                }


            public void SellAnimal()
            {
                if (_animals > 0)
                {
                    _money._balance+= 150;
                    _animals--;
                    Console.WriteLine("An animal has been sold");
                }
                else
                {
                    Console.WriteLine("You have no Animals to sell.");
                }
            }





        }
        public class Money
        {
            public int _balance = 500;
           

           public bool TryBuy(int price)
            {
                if (_balance < price)
                {
                    Console.WriteLine("You don't have enough money");
                    return false;
                }
                _balance -= price;
                Console.WriteLine("You have bought the animal");
                return true;
            }
           
        }
    }
}
