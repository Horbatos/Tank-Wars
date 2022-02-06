using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace OTTER
{
    /// <summary>  
    ///  Pomocne metode za rad igre  
    /// </summary> 
    static class GameMethods
    {
        /// <summary>  
        ///  Provjerava dira li tenk rub ekrana  
        /// </summary> 
        public static void OutOfBounds(Tank sprite) //zbog problema sa SetHeading() metodom provjera se vrsi ovjde
        {
            //ako se nalazi u koordinatama za skrivanje moramo ga tamo i ostavit
            if (sprite.X == GameOptions.HideX && sprite.Y == GameOptions.HideY) return;

            if (sprite.X <= GameOptions.LeftEdge) sprite.X = GameOptions.LeftEdge;
            else if (sprite.X >= (GameOptions.RightEdge - sprite.Width)) sprite.X = GameOptions.RightEdge - sprite.Width;

            if (sprite.Y <= GameOptions.UpEdge) sprite.Y = GameOptions.UpEdge;
            else if (sprite.Y >= (GameOptions.DownEdge - sprite.Heigth)) sprite.Y = GameOptions.DownEdge - sprite.Heigth;
        }
        /// <summary>  
        ///  Provjerava dira li oruzje rub ekrana  
        /// </summary> 
        public static void OutOfBounds(Weapon sprite)
        {
            if (sprite.X == GameOptions.HideX && sprite.Y == GameOptions.HideY) return;

            if (sprite.X <= GameOptions.LeftEdge || sprite.X >= (GameOptions.RightEdge - sprite.Width)) { Hide(sprite); sprite.Fired = false; }
            else if (sprite.Y <= GameOptions.UpEdge || sprite.Y >= (GameOptions.DownEdge - sprite.Heigth)) { Hide(sprite); sprite.Fired = false; }
        }
        /// <summary>  
        ///  Ponovno stavlja lika na nasumicnu poziciju na ekranu  
        /// </summary> 
        public static void Respawn(PlayerTank player, Tank enemy, EnemyTank e1, EnemyTank e2, TigerTank t1, TigerTank t2, Sprite[] b, Sprite[] l)
        {
            Random rng = new Random();
            enemy.SetVisible(false);
            do
            {
                enemy.Y = rng.Next(0, GameOptions.DownEdge - enemy.Heigth);
                enemy.X = rng.Next(0, GameOptions.RightEdge - enemy.Width);
            } while ((Math.Abs(enemy.Y - player.Y) < GameOptions.MinDistance) || (Math.Abs(enemy.X - player.X) < GameOptions.MinDistance) || !NoCollisions(enemy, e1, e2, t1, t2, b, l));
            
            enemy.IsDead = false;
            enemy.SetVisible(true);
            enemy.PointToSprite(player);
        }
        /// <summary>  
        ///  Pozicionira kupolu na tenk  
        /// </summary> 
        public static void TurretPosition(Sprite body, Sprite turret)
        {
            if (body.X == GameOptions.HideX) { turret.X = GameOptions.HideX; turret.Y = GameOptions.HideY; return; }
            turret.X = body.X + body.Width / 2 - turret.Width / 2;
            turret.Y = body.Y + body.Heigth / 2 - turret.Heigth / 2;
        }
        /// <summary>  
        ///  Stavlja oruzje na mjesto i smjer kupole  
        /// </summary> 
        public static void WeaponPosition(Weapon weap, Sprite turret)
        {
            weap.SetHeading(turret.GetHeading());
            weap.X = turret.X + turret.Width / 2 - weap.Width / 2;
            weap.Y = turret.Y + turret.Heigth / 2 - weap.Width / 2;
            weap.MoveSteps(turret.Width/2);
        }
        /// <summary>  
        ///  Skriva spriteove na mjesto van ekrana
        /// </summary> 
        public static void Hide(Sprite sprite)
        {
            sprite.X = GameOptions.HideX;
            sprite.Y = GameOptions.HideY;
        }
        /// <summary>  
        ///  Priprema i stavlja sprite u igru 
        /// </summary> 
        public static void SetupSprite(Sprite sprite)
        {
            Random rng = new Random();
            sprite.SetSize(40);
            sprite.RotationStyle = "AllAround";
            sprite.AddCostumes("sprites\\explosion1.png");
            sprite.SetHeading(rng.Next(0,361));
            Game.AddSprite(sprite);
        }
        /// <summary>  
        ///  Provjerava je li pogoden igrac
        /// </summary> 
        public static void EnemyHit(Weapon weap, PlayerTank enemy, Shield shield)
        {
            if (weap.TouchingSprite(enemy) && enemy.X!=GameOptions.HideX)
            {
                SoundPlayer sp = new SoundPlayer("sounds\\hit.wav");
                sp.Play();
                Hide(weap);
                weap.Fired = false;
                if (shield.IsEquiped) shield.IsEquiped = false;
                else enemy.Health-=weap.Damage;
            }
        }
        /// <summary>  
        ///  Provjerava je li pogoden neprijatelj  
        /// </summary> 
        public static void EnemyHit(Weapon weap, EnemyTank enemy)
        {
            if (weap.TouchingSprite(enemy) && enemy.X != GameOptions.HideX)
            {
                SoundPlayer sp = new SoundPlayer("sounds\\hit.wav");
                sp.Play();
                Hide(weap);
                weap.Fired = false;
                enemy.Health -= weap.Damage;
            }
        }
        /// <summary>  
        ///  Provjerava je li pogoden kamen  
        /// </summary> 
        public static void BoulderHit(Weapon weap, Sprite[] boulders)
        {
            for (int i = 0; i < limitBoulder; i++)
            {
                if (weap.TouchingSprite(boulders[i]) && boulders[i].X != GameOptions.HideX)
                {
                    Hide(weap);
                    SoundPlayer sp = new SoundPlayer("sounds\\rockHit.wav");
                    sp.Play();
                    weap.Fired = false;
                }
            }
        }

        private static int limitBoulder, limitLake;// broj kamena i jezera
        /// <summary>  
        ///  Postavlja jezera i kamenje na ekran  
        /// </summary> 
        public static void PlacaLakesBoulders(Sprite[] lakes, Sprite[] boulders)
        {
            Random rng = new Random();
            limitBoulder = rng.Next(0, 5);
            limitLake = rng.Next(0, 5);
            for (int i = 0; i < limitBoulder; i++)
            {
                //na ovaj nacin se kamenja ne mogu djelomicno preklapati
                boulders[i].X = rng.Next(0, 15) * boulders[i].Width;
                boulders[i].Y = rng.Next(0, 10) * boulders[i].Heigth;
            }
            for (int i = 0; i < limitLake; i++)
            {
                do
                {//da se jezera ne preklapaju s kamenjima
                    lakes[i].X = rng.Next(0, 14) * lakes[i].Width;
                    lakes[i].Y = rng.Next(0, 7) * lakes[i].Heigth;
                } while (!NoCollisions(boulders, lakes[i]));
            }
        }
        /// <summary>  
        ///  Provjerava dira li igrac jezero ili kamen te ga pomice u drugom smjeru ako dira  
        /// </summary> 
        public static bool HittingObject(Sprite[] boulders, Sprite[] lakes, PlayerTank tank)
        {
            for (int i = 0; i < limitBoulder; i++)
            {
                if (tank.TouchingSprite(boulders[i]))
                {
                    return true;
                }
            }
            for (int i = 0; i < limitLake; i++)
            {
                if (tank.TouchingSprite(lakes[i]))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>  
        ///  Provjerava dira li tenk jezero ili kamen te ga okrece ako dira   
        /// </summary> 
        public static void HittingObject(Sprite[] boulders, Sprite[] lakes, EnemyTank tank)
        {
            for (int i = 0; i < limitBoulder; i++)
            {
                if (tank.TouchingSprite(boulders[i]))
                {
                    tank.SetHeading(tank.GetHeading() - 180);
                }
            }
            for (int i = 0; i < limitLake; i++)
            {
                if (tank.TouchingSprite(lakes[i]))
                {
                    tank.SetHeading(tank.GetHeading() - 180);
                }
            }
        }
        /// <summary>  
        ///  Provjerava dira li tenk ikoji sprite   
        /// </summary> 
        private static bool NoCollisions(Tank tank, EnemyTank e1,EnemyTank e2, TigerTank t1, TigerTank t2, Sprite[] b, Sprite[] l)
        {
            if (tank.TouchingSprite(e1)) return false;
            if (tank.TouchingSprite(e2)) return false;
            if (tank.TouchingSprite(t1)) return false;
            if (tank.TouchingSprite(t2)) return false;
            for (int i = 0; i < limitBoulder; i++)
            {
                if (tank.TouchingSprite(b[i])) return false;
            }
            for (int i = 0; i < limitLake; i++)
            {
                if (tank.TouchingSprite(l[i])) return false;
            }
            return true;
        }
        /// <summary>  
        ///  Provjerava dira li tenk ikoji sprite   
        /// </summary> 
        public static bool NoCollisions(Tank tank, Sprite[] b, Sprite[] l)
        {
            for (int i = 0; i < limitBoulder; i++)
            {
                if (tank.TouchingSprite(b[i])) return false;
            }
            for (int i = 0; i < limitLake; i++)
            {
                if (tank.TouchingSprite(l[i])) return false;
            }
            return true;
        }

        public static bool NoCollisions(Sprite[] boulders, Sprite lake)
        {
            for (int i = 0; i < limitBoulder; i++)
            {
                if (lake.TouchingSprite(boulders[i])) return false;
            }
            return true;
        }
        /// <summary>  
        ///  Otvara datoteku s pravilima i opisom igre  
        /// </summary> 
        public static void OpenRules(string file)
        {
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string line, text = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        text += line + "\n";
                    }
                    MessageBox.Show(text);
                }
            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("There was a problem reading rules.txt\nChek if it is located in the game folder");
            }
        }
        /// <summary>  
        ///  Otvara datoteku s najboljim rezultatima  
        /// </summary> 
        public static void OpenHighscores(string file)
        {
            try
            {
                using (StreamReader sr = new StreamReader(file))
            {
                string line;
                string text = "HIGHSCORES:\n----------\n";
                int brojac = 1;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] player = line.Split(';');
                    text+=brojac+". "+player[0]+" ---> "+player[1]+"\n";
                    if (brojac == 10) break;
                    brojac++;
                }
                
                MessageBox.Show(text);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("There was a problem reading highscores.txt\nChek if it is located in the game folder");
            }
        }
        private class Player//klasa za lakse spremanje rezultata
        {
            public string Name;
            public int Points;
            public Player(string name, int points)
            {
                Name = name;
                Points = points;
            }
        }
        /// <summary>  
        ///  Sprema rezultat u datoteku  
        /// </summary> 
        public static void SaveHighscore(int points, string name , string file)
        {
            try
            {
                Player new_player;
            List<Player> list = new List<Player>();
            new_player = new Player(name, points);
            list.Add(new_player);
            using (StreamReader sr = new StreamReader(file))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] player = line.Split(';');
                    new_player = new Player(player[0], int.Parse(player[1]));
                    list.Add(new_player);
                }
            }
            list = list.OrderByDescending(o => o.Points).ToList();
            File.WriteAllText(file, String.Empty);
            using (StreamWriter sw = new StreamWriter(file))
            {
                foreach (Player player in list)
                {
                    sw.WriteLine(player.Name + ";" + player.Points);
                }
            }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("There was a problem reading and/or writting to highscores.txt\nChek if it is located in the game folder");
            }
        }
    }
}
