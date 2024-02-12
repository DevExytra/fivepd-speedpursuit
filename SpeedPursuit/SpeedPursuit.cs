using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;

namespace SpeedPursuit
{
    [CalloutProperties("Speed Pursuit", "ERLS Team", "1.2")]
    public class SpeedPursuit : Callout
    {
        private static Random _rnd = new Random();
        private Vehicle _car;
        private Ped _driver;
        private Ped _driver2;

        public SpeedPursuit()
        {
            float offsetX = _rnd.Next(100, 700);
            float offsetY = _rnd.Next(100, 700);
            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));
            ShortName = "High Speed Pursuit";
            CalloutDescription = "Speed";
            ResponseCode = 3;
            StartDistance = 100f;
        }

        // List of Random Vehicles to get for the callout
        private VehicleHash GetRandomVehicle()
        {
            List<VehicleHash> vehicle = new List<VehicleHash>
            {
                VehicleHash.Adder,
                VehicleHash.T20,
                VehicleHash.Comet2,
                VehicleHash.ItaliGTB,
                VehicleHash.ItaliGTB2,
                VehicleHash.Zentorno
            };
            return vehicle[_rnd.Next(vehicle.Count)];
        }

        // List of random suspects to get for that callout
        private PedHash GetRandomDriver()
        {
            List<PedHash> ped = new List<PedHash>
            {
                PedHash.Michael,
                PedHash.CrisFormage,
                PedHash.Azteca01GMY,
                PedHash.SiemonYetarian,
                PedHash.Bankman01,
                PedHash.Bankman,
                PedHash.Ballasog,
                PedHash.ONeil,
                PedHash.OldMan2,
                PedHash.Beach02AMY
            };
            return ped[_rnd.Next(ped.Count)];
        }

        // List of random shooting suspects
        private PedHash GetRandomDriver2()
        {
            List<PedHash> ped = new List<PedHash>
            {
                PedHash.Agent14,
                PedHash.Abigail,
                PedHash.Ashley,
                PedHash.Jesus01,
                PedHash.DaveNorton,
                PedHash.Tanisha,
                PedHash.JayNorris,
                PedHash.Vagos01GFY,
                PedHash.Stripper01SFY,
                PedHash.FibSec01SMM
            };
            return ped[_rnd.Next(ped.Count)];
        }

        private WeaponHash GetRandomWeapon()
        {
            List<WeaponHash> weapon = new List<WeaponHash>
            {
                WeaponHash.APPistol
            };
            return weapon[_rnd.Next(weapon.Count)];
        }

        // Main Event
        public async override void OnStart(Ped player)
        {
            double probability = 0.75;
            bool spawnchance = RandomizeChance(probability);

            if (spawnchance)
            {
                // Creating Suspect 1
                _driver = await World.CreatePed(GetRandomDriver(), Location + 1);
                _driver.AlwaysKeepTask = true;
                _driver.BlockPermanentEvents = true;
                
                // Creating Suspect 2
                _driver2 = await World.CreatePed(GetRandomDriver2(), Location + 1);
                _driver2.AlwaysKeepTask = true;
                _driver2.BlockPermanentEvents = true;
                _driver2.Weapons.Give(GetRandomWeapon(), 100, true, true);
                
                // Creating the Vehicle
                _car = await World.CreateVehicle(GetRandomVehicle(), Location);
                _driver.SetIntoVehicle(_car, VehicleSeat.Driver);
                _driver2.SetIntoVehicle(_car, VehicleSeat.Passenger);
                
                //Sets the Tasks for both suspects
                API.SetDriveTaskDrivingStyle(_driver.GetHashCode(), 524852);
                _driver.Task.FleeFrom(player);
                _driver2.Task.VehicleShootAtPed(player);
                
                // Registers Both Suspects
                Pursuit.RegisterPursuit(_driver);
                Pursuit.RegisterPursuit(_driver2);
                
                // Attaches the blip to the car
                _car.AttachBlip();
            }
            else
            {
                // Creates the Suspect
                _driver = await World.CreatePed(GetRandomDriver(), Location + 1);
                _driver.AlwaysKeepTask = true;
                _driver.BlockPermanentEvents = true;
                
                // Creates the Vehicle
                _car = await World.CreateVehicle(GetRandomVehicle(), Location);
                _driver.SetIntoVehicle(_car, VehicleSeat.Driver);
                
                // Sets the suspect to flee 
                API.SetDriveTaskDrivingStyle(_driver.GetHashCode(), 524852);
                _driver.Task.FleeFrom(player);
                
                // Registers suspect 1
                Pursuit.RegisterPursuit(_driver);
                
                // Attach blips to suspects and the police car
                _car.AttachBlip();
            }
        }

        private static bool RandomizeChance(double probability)
        {
            double randomNumber = _rnd.NextDouble();
            return randomNumber < probability;
        }

        // OnAccept Init
        public  override Task OnAccept()
        {
            InitBlip();
            UpdateData();
            return base.OnAccept();
        }

        // OnCancelAfter method
        public override void OnCancelAfter()
        {
            base.OnCancelAfter();

            try
            {
                if (_driver == null || (_driver2 == null || !_driver2.IsAlive && !_driver.IsAlive) || _driver.IsCuffed && (_driver2 == null || _driver2.IsCuffed)) return;
                // Clears Suspect 1
                _driver.Task.WanderAround();
                _driver.AlwaysKeepTask = false;
                _driver.BlockPermanentEvents = false;
                
                // Clears Suspect 2
                _driver2.Task.WanderAround();
                _driver2.AlwaysKeepTask = false;
                _driver2.BlockPermanentEvents = false;
            }
            catch
            {
                {
                    EndCallout();
                }
            }

        }

        // OnCancel Method
        public override void OnCancelBefore()
        {
            base.OnCancelBefore();
            try
            {
                if (_driver == null || (_driver2 == null || !_driver2.IsAlive && !_driver.IsAlive) || _driver.IsCuffed && (_driver2 == null || _driver2.IsCuffed))
                    return;

                // Clears suspect 1
                _driver.Task.WanderAround();
                _driver.AlwaysKeepTask = false;
                _driver.BlockPermanentEvents = false;

                // Clears suspect 2
                if (_driver2 != null)
                {
                    _driver2.Task.WanderAround();
                    _driver2.AlwaysKeepTask = false;
                    _driver2.BlockPermanentEvents = false;
                }
            }
            catch
            {
                EndCallout();
            }
        }
    }
}