using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OTTER
{
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";

        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {                
                foreach (Sprite sprite in allSprites)
                {                    
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Heigth));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.Transparent);
            string text = ISPIS;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Stencil", 20);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(GameOptions.RightEdge/2-100, 0, stringSize.Width, stringSize.Height);
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                e.Graphics.DrawString(text, font1, Brushes.Black, rectF1);
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;            
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                //START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }
        

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */


        /* ------------ GAME CODE START ------------ */

        /* Game variables */
        PlayerTank Player1, Player2;
        EnemyTank Enemy1, Enemy2, Enemy3;
        TigerTank Tiger1, Tiger2, Tiger3;

        //botuni i kupole nemaju neke posebne interakcije te mogu ostati spriteovi
        Sprite Player1_turret, Player2_turret, Enemy1_turret, Enemy2_turret, Enemy3_turret;
        Sprite Tiger1_turret, Tiger2_turret, Tiger3_turret;
        Sprite btnSingle, btnMulti, btnRules, btnHigh, btnExit;
        Weapon shell_player1, shell_player2, shell_enemy1, shell_enemy2, shell_enemy3;
        Weapon shell2_player1, shell2_player2, shell2_enemy1, shell2_enemy2, shell2_enemy3;

        //jezera i kamenja koja ce bit prepraka tenkovima
        Sprite[] boulders = new Sprite[4];
        Sprite[] lakes = new Sprite[4];

        //Pickupovi koje ce igrac moci skupiti
        Map map;
        Shield shield;
        Life life;

        Random rng=new Random();
        //varijable koje ce oznacavati kad pocinje koja igra
        bool MMenuStart=false, SingleStart = false, MultiStart = false;
        //konacni bodovi igraca i varijable za trajanje reloada tenka
        int FinalPoints, reloadP1 = 0, reloadP2 = 0, reloadE1 = 0, reloadE2 = 0, reloadE3 = 0;
        
        /* Initialization */

        private void SetupGame()
        {
            SetStageTitle("TANK WARS");

            //dodavanje jezera i kamenja
            for (int i = 0; i < 4; i++)
            {
                lakes[i] = new Sprite("sprites\\water.png", GameOptions.HideX, GameOptions.HideY);
                Game.AddSprite(lakes[i]);;
            }
            for (int i = 0; i < 4; i++)
            {
                boulders[i] = new Sprite("sprites\\boulder.png", GameOptions.HideX, GameOptions.HideY);
                Game.AddSprite(boulders[i]);
            }
            loadSound(0,"sounds\\prologue.wav");
            loadSound(1, "sounds\\DeathFlash.wav");
            loadSound(2, "sounds\\itempick.wav");
            loadSound(3, "sounds\\fire.wav");
            
            //botuni
            btnSingle = new Sprite("sprites\\singleplayer.png", 50, 100);
            Game.AddSprite(btnSingle);
            btnMulti = new Sprite("sprites\\multiplayer.png", 50, 150);
            Game.AddSprite(btnMulti);
            btnRules = new Sprite("sprites\\rules.png", 50, 200);
            Game.AddSprite(btnRules);
            btnHigh = new Sprite("sprites\\highscores.png", 50, 250);
            Game.AddSprite(btnHigh);
            btnExit = new Sprite("sprites\\exit.png", 50, 300);
            Game.AddSprite(btnExit);

            //pickupi
            life =new Life("sprites\\tools.png", GameOptions.HideX, GameOptions.HideY);
            life.SetSize(40);
            Game.AddSprite(life);
            shield = new Shield("sprites\\shield.png", GameOptions.HideX, GameOptions.HideY);
            shield.SetSize(40);
            Game.AddSprite(shield);
            map = new Map("sprites\\map.png", GameOptions.HideX, GameOptions.HideY);
            map.SetSize(40);
            Game.AddSprite(map);

            //dodavanje igraca i neprijatelja
            Player1 = new PlayerTank("sprites\\player.png", GameOptions.HideX, GameOptions.HideY);
            Player1.SetVisible(false);
            GameMethods.SetupSprite(Player1);
            Player1_turret = new Sprite("sprites\\player_turret.png", GameOptions.HideX, GameOptions.HideY);
            Player1_turret.SetVisible(false);
            GameMethods.SetupSprite(Player1_turret);
            shell_player1 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell_player1);
            Player2 = new PlayerTank("sprites\\player2.png", GameOptions.HideX, GameOptions.HideY);
            Player2.SetVisible(false);
            GameMethods.SetupSprite(Player2);
            Player2_turret = new Sprite("sprites\\player2_turret.png", GameOptions.HideX, GameOptions.HideY);
            Player2_turret.SetVisible(false);
            GameMethods.SetupSprite(Player2_turret);
            Player2.ReloadSpeed = 40;
            shell_player2 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell_player2);
            Enemy1 = new EnemyTank("sprites\\enemy.png", GameOptions.HideX, GameOptions.HideY);
            Enemy1.SetVisible(false);
            GameMethods.SetupSprite(Enemy1);
            Enemy1_turret = new Sprite("sprites\\enemy_turret.png", GameOptions.HideX, GameOptions.HideY);
            Enemy1_turret.SetVisible(false);
            GameMethods.SetupSprite(Enemy1_turret);
            shell_enemy1 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell_enemy1);
            Enemy2 = new EnemyTank("sprites\\enemy.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Enemy2);
            Enemy2_turret = new Sprite("sprites\\enemy_turret.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Enemy2_turret);
            shell_enemy2 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell_enemy2);
            Enemy3 = new EnemyTank("sprites\\enemy.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Enemy3);
            Enemy3_turret = new Sprite("sprites\\enemy_turret.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Enemy3_turret);
            shell_enemy3 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell_enemy3);
            
            //dodavanje 2. oruzja
            shell2_enemy1 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell2_enemy1);
            shell2_enemy2 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell2_enemy2);
            shell2_enemy3 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell2_enemy3);
            shell2_player1 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell2_player1);
            shell2_player2 = new Weapon("sprites\\shot.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(shell2_player2);

            //dodavanje tigrova
            Tiger1 = new TigerTank("sprites\\tiger.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger1);
            Tiger1_turret = new Sprite("sprites\\tiger_turret.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger1_turret);
            Tiger2 = new TigerTank("sprites\\tiger.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger2);
            Tiger2_turret = new Sprite("sprites\\tiger_turret.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger2_turret);
            Tiger3 = new TigerTank("sprites\\tiger.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger3);
            Tiger3_turret = new Sprite("sprites\\tiger_turret.png", GameOptions.HideX, GameOptions.HideY);
            GameMethods.SetupSprite(Tiger3_turret);


            Player1._died += new EventHandler(IncreseKillCount);
            Player2._died += new EventHandler(IncreseKillCount);
            Enemy1._died += new EventHandler(IncreseKillCount);
            Enemy2._died += new EventHandler(IncreseKillCount);
            Enemy3._died += new EventHandler(IncreseKillCount);
            Tiger1._died += new EventHandler(IncreseKillCount);
            Tiger2._died += new EventHandler(IncreseKillCount);
            Tiger3._died += new EventHandler(IncreseKillCount);

            MainMenu();
        }

        /* Event handlers - metode*/

        delegate void DelegateTypeVoid(string text);

        /// <summary>  
        ///  ispis poruke za zavrsetak igre   
        /// </summary>
        private void ShowLblTxt(string t)
        {  
            if (this.label1.InvokeRequired)
            {
                DelegateTypeVoid d = new DelegateTypeVoid(ShowLblTxt);
                this.Invoke(d, new object[] { t });
            }
            else
            {
                this.label1.Text = t;
                this.label1.Visible = true;
                this.textBox1.Enabled = true;
                this.textBox1.Show();
            }
        }

        /// <summary>  
        ///  Ako neki lik u igri umre, poziva se ovo metoda   
        /// </summary>
        private void IncreseKillCount(Object sender, EventArgs e)
        {
            playSound(1);//zvuk eksplozije
            if(sender == Player1)
            {
                if (SingleStart)
                {
                    FinalPoints = Player1.Points;
                    SingleStart = false;
                    ShowLblTxt("GAME OVER\nScore: " + FinalPoints + "\nEnter your name:");
                }
                else ISPIS="PLAYER 2 WINS";
                MultiStart = false;
                Wait(2);
                MainMenu();
            }
            else if (sender == Player2)
            {
                ISPIS="PLAYER 1 WINS";
                MultiStart = false;
                Wait(2);
                MainMenu();
            }
            //svaki put kad neprijatelj umre igrac dobiva bodove
            Player1.KillCount++;
            Player1.Points += 100;
            if (sender == Enemy1) Enemy1.DeathCount++;
            else if (sender == Enemy2) Enemy2.DeathCount++;
            else if (sender == Enemy3) Enemy3.DeathCount++;

            if (sender == Enemy1 && Enemy1.DeathCount == 5)//dolazak prvog tigra
            {
                Enemy1.IsTiger = true;
                Enemy1_turret.NextCostume();
                Wait(1);
                Enemy1.NextCostume();
                Wait(1);
                Enemy1_turret.NextCostume();
                Enemy1.NextCostume();
                GameMethods.Hide(Enemy1);
                GameMethods.Hide(Enemy1_turret);
                shell_enemy1.Damage = 3;
                GameMethods.Respawn(Player1, Tiger1, Enemy2, Enemy3,Tiger2,Tiger3,boulders,lakes);
                Game.StartScript(Tiger_moving1);
                Game.StartScript(Tiger1Dead);
            }

            else if (sender == Enemy2 && Enemy2.DeathCount == 5)//dolazak drugog tigra
            {
                Enemy2.IsTiger = true;
                Enemy2_turret.NextCostume();
                Wait(1);
                Enemy2.NextCostume();
                Wait(1);
                Enemy2_turret.NextCostume();
                Enemy2.NextCostume();
                GameMethods.Hide(Enemy2);
                GameMethods.Hide(Enemy2_turret);
                shell_enemy2.Damage = 3;
                GameMethods.Respawn(Player1, Tiger2, Enemy1, Enemy3, Tiger1, Tiger3, boulders, lakes);
                Game.StartScript(Tiger_moving2);
                Game.StartScript(Tiger2Dead);
            }
            else if (sender == Enemy3 && Enemy3.DeathCount == 5)//dolazak treceg tigra
            {
                Enemy3.IsTiger = true;
                Enemy3_turret.NextCostume();
                Wait(1);
                Enemy3.NextCostume();
                Wait(1);
                Enemy3_turret.NextCostume();
                Enemy3.NextCostume();
                GameMethods.Hide(Enemy3);
                GameMethods.Hide(Enemy3_turret);
                shell_enemy3.Damage = 3;
                GameMethods.Respawn(Player1, Tiger3, Enemy2, Enemy1, Tiger2, Tiger1, boulders, lakes);
                Game.StartScript(Tiger_moving3);
                Game.StartScript(Tiger3Dead);
            }

            if (Player1.KillCount == 4)//aktiviranje drugog neprijatelja
            {
                GameMethods.Respawn(Player1, Enemy2, Enemy1, Enemy3, Tiger1, Tiger3, boulders, lakes);
                Game.StartScript(Enemy_moving2);
                Game.StartScript(Enemy2Dead);
            }
            else if(Player1.KillCount == 9)//aktiviranje treceg neprijatelja
            {
                GameMethods.Respawn(Player1, Enemy3, Enemy2, Enemy1, Tiger2, Tiger1, boulders, lakes);
                Game.StartScript(Enemy_moving3);
                Game.StartScript(Enemy3Dead);
            }
        }

        /// <summary>  
        ///  unos imena igraca za highscore   
        /// </summary>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GameMethods.SaveHighscore(FinalPoints, textBox1.Text,"highscores.txt");
                this.label1.Text = "";
                this.label1.Visible = false;
                this.textBox1.Text="";
                this.textBox1.Visible = false;
                this.textBox1.Enabled = false;
                this.Focus();
            }
        }

        /* Scripts */

        //Skripte za pickupe
        /// <summary>  
        ///  Postavljenje pickupa po mapi   
        /// </summary>
        private int PlacePickUps()
        {
            while (SingleStart)
            {
                    if (rng.Next(0, 10) == 0)
                    {
                        shield.X = rng.Next(0, GameOptions.RightEdge - shield.Width);
                        shield.Y = rng.Next(0, GameOptions.RightEdge - shield.Heigth);
                        Wait(10);
                        GameMethods.Hide(shield);
                    }
                    else if (rng.Next(0, 5) == 0)
                    {
                        life.X = rng.Next(0, GameOptions.RightEdge - life.Width);
                        life.Y = rng.Next(0, GameOptions.RightEdge - life.Heigth);
                        Wait(10);
                        GameMethods.Hide(life);
                    }
                    else
                    {
                        map.X = rng.Next(0, GameOptions.RightEdge - map.Width);
                        map.Y = rng.Next(0, GameOptions.RightEdge - map.Heigth);
                        Wait(10);
                        GameMethods.Hide(map);
                    }
            }
            return 0;
        }
       
        /// <summary>  
        ///  Sto se dogada ako igrac skupi odredeni pickup   
        /// </summary>
        private int TouchPickUps()
        {
            while (SingleStart)
            {
                if (map.TouchingSprite(Player1)) { Player1.Points += map.Value;playSound(2); GameMethods.Hide(map); }
                else if (shield.TouchingSprite(Player1)) { shield.IsEquiped = true; playSound(2); GameMethods.Hide(shield); }
                else if (life.TouchingSprite(Player1)) { Player1.Health += life.Heal; playSound(2); GameMethods.Hide(life); }
                Wait(0.01);
            }
            return 0;
        }
        
        //Skripte za igraca
        /// <summary>  
        ///  Kretanje Player1u  
        /// </summary>
        private int Player_movingP1()
        {
            while (SingleStart)
            {
                reloadP1++;
                ISPIS = "HEALTH: " + Player1.Health + "   SCORE: " + Player1.Points;
                if (shield.IsEquiped) ISPIS += "   <SHILD READY>";
                if (sensing.KeyPressed(Keys.A)) Player1.SetHeading(Player1.GetHeading() - 2);
                else if (sensing.KeyPressed(Keys.D)) Player1.SetHeading(Player1.GetHeading() + 2);
                else if (sensing.KeyPressed(Keys.W))
                {
                    if (!GameMethods.HittingObject(boulders, lakes, Player1)) Player1.MoveSteps(Player1.Speed);
                    else { Player1.MoveSteps(-2 * Player1.Speed); GameMethods.TurretPosition(Player1, Player1_turret); Wait(0.4); }
                }

                else if (sensing.KeyPressed(Keys.S))
                {
                    if (!GameMethods.HittingObject(boulders, lakes, Player1)) Player1.MoveSteps(-Player1.Speed);
                    else { Player1.MoveSteps(2 * Player1.Speed); GameMethods.TurretPosition(Player1, Player1_turret); Wait(0.4); }
                }
                else if (sensing.MouseDown)
                {
                    if (!shell_player1.Fired && reloadP1 > Player1.ReloadSpeed)
                    {
                        reloadP1 = 0;
                        playSound(3);
                        shell_player1.Fired = true;
                        GameMethods.WeaponPosition(shell_player1, Player1_turret);
                        Game.StartScript(ShootingP1);
                    }
                    else if (!shell2_player1.Fired && reloadP1>Player1.ReloadSpeed)
                    {
                        reloadP1 = 0;
                        playSound(3);
                        shell2_player1.Fired = true;
                        GameMethods.WeaponPosition(shell2_player1, Player1_turret);
                        Game.StartScript(ShootingP1_Shell2);
                    }
                }
                Player1_turret.PointToMouse(sensing.Mouse);
                GameMethods.OutOfBounds(Player1);
                GameMethods.TurretPosition(Player1, Player1_turret);
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje Igrac1   
        /// </summary>
        private int ShootingP1()
        {
            while (shell_player1.Fired)
            {
                shell_player1.MoveSteps(shell_player1.Speed);
                GameMethods.EnemyHit(shell_player1,Enemy1);
                GameMethods.EnemyHit(shell_player1, Enemy2);
                GameMethods.EnemyHit(shell_player1, Enemy3);

                GameMethods.EnemyHit(shell_player1, Tiger1);
                GameMethods.EnemyHit(shell_player1, Tiger2);
                GameMethods.EnemyHit(shell_player1, Tiger3);
                GameMethods.EnemyHit(shell_player1, Player2, shield);
                GameMethods.BoulderHit(shell_player1, boulders);
                GameMethods.OutOfBounds(shell_player1);
                Wait(0.03);
            }

            return 0;
        }
        /// <summary>  
        ///  Pucanje Igrac1 2. raketa
        /// </summary>
        private int ShootingP1_Shell2()
        {
            while (shell2_player1.Fired)
            {
                shell2_player1.MoveSteps(shell2_player1.Speed);
                GameMethods.EnemyHit(shell2_player1, Enemy1);
                GameMethods.EnemyHit(shell2_player1, Enemy2);
                GameMethods.EnemyHit(shell2_player1, Enemy3);

                GameMethods.EnemyHit(shell2_player1, Tiger1);
                GameMethods.EnemyHit(shell2_player1, Tiger2);
                GameMethods.EnemyHit(shell2_player1, Tiger3);
                GameMethods.EnemyHit(shell2_player1, Player2, shield);
                GameMethods.BoulderHit(shell2_player1, boulders);
                GameMethods.OutOfBounds(shell2_player1);
                Wait(0.03);
            }

            return 0;
        }

        //Skripte za neprijateljske tenkove
        /// <summary>  
        ///  Ako neprijatelj1 umre, stvoriti ga ponovno   
        /// </summary>
        private int Enemy1Dead()
        {
            while ( SingleStart && !Enemy1.IsTiger)
            {
                if (Enemy1.IsDead)
                {
                    Enemy1_turret.NextCostume();
                    Wait(1);
                    Enemy1.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Enemy1);
                    GameMethods.TurretPosition(Enemy1, Enemy1_turret);
                    Enemy1.Health = 1;
                    GameMethods.Respawn(Player1, Enemy1, Enemy2, Enemy3, Tiger2, Tiger3, boulders, lakes);
                    Enemy1.NextCostume();
                    Enemy1_turret.NextCostume();
                    Game.StartScript(Enemy_moving1);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje neprijatelj1   
        /// </summary>
        private int Enemy_moving1()
        {
            while (!Enemy1.IsDead && SingleStart && !Enemy1.IsTiger)
            {
                reloadE1++;
                GameMethods.OutOfBounds(Enemy1);
                GameMethods.TurretPosition(Enemy1, Enemy1_turret);
                GameMethods.HittingObject(boulders, lakes, Enemy1);
                Enemy1.MoveSteps(Enemy1.Speed);
                if (rng.Next(1, 15) == 1) 
                    Enemy1.SetHeading(Enemy1.GetHeading() + rng.Next(-40, 30));
                Enemy1_turret.PointToSprite(Player1);
                if (!shell_enemy1.Fired && reloadE1>Enemy1.ReloadSpeed)
                {
                    reloadE1 = 0;
                    Wait(1);
                    playSound(3);
                    shell_enemy1.Fired = true;
                    GameMethods.WeaponPosition(shell_enemy1, Enemy1_turret);
                    Game.StartScript(ShootingE1);
                }
                else if (!shell2_enemy1.Fired && reloadE1 > Enemy1.ReloadSpeed)
                {
                    reloadE1 = 0;
                    Wait(1);
                    playSound(3);
                    shell2_enemy1.Fired = true;
                    GameMethods.WeaponPosition(shell2_enemy1, Enemy1_turret);
                    Game.StartScript(ShootingE1_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj1   
        /// </summary>
        private int ShootingE1()
        {
            while (shell_enemy1.Fired && SingleStart)
            {
                shell_enemy1.MoveSteps(shell_enemy1.Speed);
                GameMethods.EnemyHit(shell_enemy1, Player1, shield);
                GameMethods.BoulderHit(shell_enemy1, boulders);
                GameMethods.OutOfBounds(shell_enemy1);
                Wait(0.04);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj1 2 raketa   
        /// </summary>
        private int ShootingE1_Shell2()
        {
            while (shell2_enemy1.Fired && SingleStart)
            {
                shell2_enemy1.MoveSteps(shell2_enemy1.Speed);
                GameMethods.EnemyHit(shell2_enemy1, Player1, shield);
                GameMethods.BoulderHit(shell2_enemy1, boulders);
                GameMethods.OutOfBounds(shell2_enemy1);
                Wait(0.04);
            }
            return 0;
        }
        /// <summary>  
        ///  Provjera smrti neprijatelj2   
        /// </summary>
        private int Enemy2Dead()
        {
            while (SingleStart && !Enemy2.IsTiger)
            {
                if (Enemy2.IsDead)
                {
                    Enemy2_turret.NextCostume();
                    Wait(1);
                    Enemy2.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Enemy2);
                    GameMethods.TurretPosition(Enemy2, Enemy2_turret);
                    Enemy2.Health = 1;
                    GameMethods.Respawn(Player1, Enemy2, Enemy1, Enemy3, Tiger1, Tiger3, boulders, lakes);
                    Enemy2.NextCostume();
                    Enemy2_turret.NextCostume();
                    Game.StartScript(Enemy_moving2);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje neprijatelj2    
        /// </summary>
        private int Enemy_moving2()
        {
            while (!Enemy2.IsDead && SingleStart && !Enemy2.IsTiger)
            {
                reloadE2++;
                GameMethods.OutOfBounds(Enemy2);
                GameMethods.TurretPosition(Enemy2, Enemy2_turret);
                GameMethods.HittingObject(boulders, lakes, Enemy2);
                Enemy2.MoveSteps(Enemy2.Speed);
                if (rng.Next(1, 15) == 1)
                    Enemy2.SetHeading(Enemy2.GetHeading() + rng.Next(-40, 30));
                Enemy2_turret.PointToSprite(Player1);
                if (!shell_enemy2.Fired && reloadE2>Enemy2.ReloadSpeed)
                {
                    reloadE2 = 0;
                    Wait(1);
                    playSound(3);
                    shell_enemy2.Fired = true;
                    GameMethods.WeaponPosition(shell_enemy2, Enemy2_turret);
                    Game.StartScript(ShootingE2);
                }
                else if (!shell2_enemy2.Fired && reloadE2 > Enemy2.ReloadSpeed)
                {
                    reloadE2 = 0;
                    Wait(1);
                    playSound(3);
                    shell2_enemy2.Fired = true;
                    GameMethods.WeaponPosition(shell2_enemy2, Enemy2_turret);
                    Game.StartScript(ShootingE2_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj2   
        /// </summary>
        private int ShootingE2()
        {
            while (shell_enemy2.Fired && SingleStart)
            {
                shell_enemy2.MoveSteps(shell_enemy2.Speed);
                GameMethods.EnemyHit(shell_enemy2, Player1, shield);
                GameMethods.BoulderHit(shell_enemy2, boulders);
                GameMethods.OutOfBounds(shell_enemy2);
                Wait(0.04);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj2 2 raketa   
        /// </summary>
        private int ShootingE2_Shell2()
        {
            while (shell2_enemy2.Fired && SingleStart)
            {
                shell2_enemy2.MoveSteps(shell2_enemy2.Speed);
                GameMethods.EnemyHit(shell2_enemy2, Player1, shield);
                GameMethods.BoulderHit(shell2_enemy2, boulders);
                GameMethods.OutOfBounds(shell2_enemy2);
                Wait(0.04);
            }
            return 0;
        }

        /// <summary>  
        ///  Provjera smrti neprijatelj3   
        /// </summary>
        private int Enemy3Dead()
        {
            while (SingleStart && !Enemy3.IsTiger)
            {
                if (Enemy3.IsDead)
                {
                    Enemy3_turret.NextCostume();
                    Wait(1);
                    Enemy3.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Enemy3);
                    GameMethods.TurretPosition(Enemy3, Enemy3_turret);
                    Enemy3.Health = 1;
                    GameMethods.Respawn(Player1, Enemy3, Enemy2, Enemy1, Tiger2, Tiger1, boulders, lakes);
                    Enemy3.NextCostume();
                    Enemy3_turret.NextCostume();
                    Game.StartScript(Enemy_moving3);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje neprijatelj3   
        /// </summary>
        private int Enemy_moving3()
        {
            while (!Enemy3.IsDead && SingleStart && !Enemy3.IsTiger)
            {
                reloadE3++;
                GameMethods.OutOfBounds(Enemy3);
                GameMethods.TurretPosition(Enemy3, Enemy3_turret);
                GameMethods.HittingObject(boulders, lakes, Enemy3);
                Enemy3.MoveSteps(Enemy3.Speed);
                if (rng.Next(1, 15) == 1)
                    Enemy3.SetHeading(Enemy3.GetHeading() + rng.Next(-40, 30));
                Enemy3_turret.PointToSprite(Player1);
                if (!shell_enemy3.Fired && reloadE3>Enemy3.ReloadSpeed)
                {
                    reloadE3 = 0;
                    shell_enemy3.Fired = true;
                    Wait(1);
                    playSound(3);
                    GameMethods.WeaponPosition(shell_enemy3, Enemy3_turret);
                    Game.StartScript(ShootingE3);
                }
                else if(!shell2_enemy3.Fired && reloadE3 > Enemy3.ReloadSpeed)
                {
                    reloadE3 = 0;
                    shell2_enemy3.Fired = true;
                    Wait(1);
                    playSound(3);
                    GameMethods.WeaponPosition(shell2_enemy3, Enemy3_turret);
                    Game.StartScript(ShootingE3_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj3  
        /// </summary>
        private int ShootingE3()
        {
            while (shell_enemy3.Fired && SingleStart)
            {
                shell_enemy3.MoveSteps(shell_enemy3.Speed);
                GameMethods.EnemyHit(shell_enemy3, Player1, shield);
                GameMethods.BoulderHit(shell_enemy3, boulders);
                GameMethods.OutOfBounds(shell_enemy3);
                Wait(0.04);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje neprijatelj3 2 raketa   
        /// </summary>
        private int ShootingE3_Shell2()
        {
            while (shell2_enemy3.Fired && SingleStart)
            {
                shell2_enemy3.MoveSteps(shell2_enemy3.Speed);
                GameMethods.EnemyHit(shell2_enemy3, Player1, shield);
                GameMethods.BoulderHit(shell2_enemy3, boulders);
                GameMethods.OutOfBounds(shell2_enemy3);
                Wait(0.04);
            }
            return 0;
        }

        //Tiger skripte
        /// <summary>  
        ///  Provjera smrti Tiger1    
        /// </summary>
        private int Tiger1Dead()
        {
            while (SingleStart)
            {
                if (Tiger1.IsDead)
                {
                    Tiger1_turret.NextCostume();
                    Wait(1);
                    Tiger1.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Tiger1);
                    GameMethods.TurretPosition(Tiger1, Tiger1_turret);
                    Tiger1.Health = 3;
                    GameMethods.Respawn(Player1, Tiger1, Enemy2, Enemy3, Tiger2, Tiger3, boulders, lakes);
                    Tiger1.NextCostume();
                    Tiger1_turret.NextCostume();
                    Game.StartScript(Tiger_moving1);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje Tiger1   
        /// </summary>
        private int Tiger_moving1()
        {
            while (!Tiger1.IsDead && SingleStart)
            {
                reloadE1++;
                GameMethods.OutOfBounds(Tiger1);
                GameMethods.TurretPosition(Tiger1, Tiger1_turret);
                GameMethods.HittingObject(boulders, lakes, Tiger1);
                Tiger1.MoveSteps(Tiger1.Speed);
                if (rng.Next(1, 15) == 1)
                    Tiger1.SetHeading(Tiger1.GetHeading() + rng.Next(-40, 30));
                Tiger1_turret.PointToSprite(Player1);
                if (!shell_enemy1.Fired && reloadE1 > Tiger1.ReloadSpeed)
                {
                    reloadE1 = 0;
                    Wait(1);
                    playSound(3);
                    shell_enemy1.Fired = true;
                    GameMethods.WeaponPosition(shell_enemy1, Tiger1_turret);
                    Game.StartScript(ShootingE1);
                }
                else if (!shell2_enemy1.Fired && reloadE1 > Tiger1.ReloadSpeed)
                {
                    reloadE1 = 0;
                    Wait(1);
                    playSound(3);
                    shell2_enemy1.Fired = true;
                    GameMethods.WeaponPosition(shell2_enemy1, Tiger1_turret);
                    Game.StartScript(ShootingE1_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }

        /// <summary>  
        ///  Provjera smrti Tiger2    
        /// </summary>
        private int Tiger2Dead()
        {
            while (SingleStart)
            {
                if (Tiger2.IsDead)
                {
                    Tiger2_turret.NextCostume();
                    Wait(1);
                    Tiger2.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Tiger2);
                    GameMethods.TurretPosition(Tiger2, Tiger2_turret);
                    Tiger2.Health = 3;
                    GameMethods.Respawn(Player1, Tiger2, Enemy1, Enemy3, Tiger1, Tiger3, boulders, lakes);
                    Tiger2.NextCostume();
                    Tiger2_turret.NextCostume();
                    Game.StartScript(Tiger_moving2);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje Tiger2   
        /// </summary>
        private int Tiger_moving2()
        {
            while (!Tiger2.IsDead && SingleStart)
            {
                reloadE2++;
                GameMethods.OutOfBounds(Tiger2);
                GameMethods.TurretPosition(Tiger2, Tiger2_turret);
                GameMethods.HittingObject(boulders, lakes, Tiger2);
                Tiger2.MoveSteps(Tiger2.Speed);
                if (rng.Next(1, 15) == 1)
                    Tiger2.SetHeading(Tiger2.GetHeading() + rng.Next(-40, 30));
                Tiger2_turret.PointToSprite(Player1);
                if (!shell_enemy2.Fired && reloadE2>Tiger1.ReloadSpeed)
                {
                    reloadE2 = 0;
                    Wait(1);
                    playSound(3);
                    shell_enemy2.Fired = true;
                    GameMethods.WeaponPosition(shell_enemy2, Tiger2_turret);
                    Game.StartScript(ShootingE2);
                }
                else if (!shell2_enemy2.Fired && reloadE2 > Tiger1.ReloadSpeed)
                {
                    reloadE2 = 0;
                    Wait(1);
                    playSound(3);
                    shell2_enemy2.Fired = true;
                    GameMethods.WeaponPosition(shell2_enemy2, Tiger2_turret);
                    Game.StartScript(ShootingE2_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }
        
        /// <summary>  
        ///  Provjera smrti Tiger3   
        /// </summary>
        private int Tiger3Dead()
        {
            while (SingleStart)
            {
                if (Tiger3.IsDead)
                {
                    Tiger3_turret.NextCostume();
                    Wait(1);
                    Tiger3.NextCostume();
                    Wait(1);
                    GameMethods.Hide(Tiger3);
                    GameMethods.TurretPosition(Tiger3, Tiger3_turret);
                    Tiger3.Health = 3;
                    GameMethods.Respawn(Player1, Tiger3, Enemy2, Enemy1, Tiger2, Tiger1, boulders, lakes);
                    Tiger3.NextCostume();
                    Tiger3_turret.NextCostume();
                    Game.StartScript(Tiger_moving3);
                }
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretanje Tiger3   
        /// </summary>
        private int Tiger_moving3()
        {
            while (!Tiger3.IsDead && SingleStart)
            {
                reloadE3++;
                GameMethods.OutOfBounds(Tiger3);
                GameMethods.TurretPosition(Tiger3, Tiger3_turret);
                GameMethods.HittingObject(boulders, lakes, Tiger3);
                Tiger3.MoveSteps(Tiger3.Speed);
                if (rng.Next(1, 15) == 1)
                    Tiger3.SetHeading(Tiger3.GetHeading() + rng.Next(-40, 30));
                Tiger3_turret.PointToSprite(Player1);
                if (!shell_enemy3.Fired && reloadE3>Tiger1.ReloadSpeed)
                {
                    reloadE3 = 0;
                    Wait(1);
                    playSound(3);
                    shell_enemy3.Fired = true;
                    GameMethods.WeaponPosition(shell_enemy3, Tiger3_turret);
                    Game.StartScript(ShootingE3);
                }
                else if (!shell2_enemy3.Fired && reloadE3 > Tiger1.ReloadSpeed)
                {
                    reloadE3 = 0;
                    Wait(1);
                    playSound(3);
                    shell2_enemy3.Fired = true;
                    GameMethods.WeaponPosition(shell2_enemy3, Tiger3_turret);
                    Game.StartScript(ShootingE3_Shell2);
                }
                Wait(0.05);
            }
            return 0;
        }
        
        //Multiplayer skripte
        /// <summary>  
        ///  Kretanje Player1 u multiplayeru   
        /// </summary>
        private int Player_movingP1_Multy()
        {
            while (MultiStart)
            {
                reloadP1++;
                ISPIS = "P1 HEALTH: " + Player1.Health + "\nP2 HEALTH: " + Player2.Health;
                if (sensing.KeyPressed(Keys.A)) Player1.SetHeading(Player1.GetHeading() - 2);
                else if (sensing.KeyPressed(Keys.D)) Player1.SetHeading(Player1.GetHeading() + 2);
                else if (sensing.KeyPressed(Keys.Q)) Player1_turret.SetHeading(Player1_turret.GetHeading() - 2);
                else if (sensing.KeyPressed(Keys.E)) Player1_turret.SetHeading(Player1_turret.GetHeading() + 2);
                else if (sensing.KeyPressed(Keys.Space))
                {
                    if (!shell_player1.Fired && reloadP1 > Player1.ReloadSpeed)
                    {
                        reloadP1 = 0;
                        playSound(3);
                        shell_player1.Fired = true;
                        GameMethods.WeaponPosition(shell_player1, Player1_turret);
                        Game.StartScript(ShootingP1);
                    }
                    else if (!shell2_player1.Fired && reloadP1 > Player1.ReloadSpeed)
                    {
                        reloadP1 = 0;
                        playSound(3);
                        shell2_player1.Fired = true;
                        GameMethods.WeaponPosition(shell2_player1, Player1_turret);
                        Game.StartScript(ShootingP1_Shell2);
                    }
                }
               else  if (sensing.KeyPressed(Keys.W)) {
                    if (!GameMethods.HittingObject(boulders, lakes, Player1)) Player1.MoveSteps(Player1.Speed);
                    else { Player1.MoveSteps(-2 * Player1.Speed); GameMethods.TurretPosition(Player1, Player1_turret); Wait(0.4); }
                }

                else if (sensing.KeyPressed(Keys.S))
                {
                    if (!GameMethods.HittingObject(boulders, lakes, Player1)) Player1.MoveSteps(-Player1.Speed);
                    else { Player1.MoveSteps(2 * Player1.Speed); GameMethods.TurretPosition(Player1, Player1_turret); Wait(0.4); }
                }
                GameMethods.OutOfBounds(Player1);
                GameMethods.TurretPosition(Player1, Player1_turret);
                Wait(0.01);
            }
            return 0;
        }
        /// <summary>  
        ///  Kretnje za Player2   
        /// </summary>
        private int Player_movingP2_Multy()
        {
            while (MultiStart)
            {
                reloadP2++;
                GameMethods.TurretPosition(Player2, Player2_turret);
                Player2.PointToMouse(sensing.Mouse);
                if (!GameMethods.HittingObject(boulders, lakes, Player2)) Player2.MoveSteps(Player2.Speed);
                else { Player2.MoveSteps(-2 * Player2.Speed); GameMethods.TurretPosition(Player2, Player2_turret); Wait(0.4); }
                if (sensing.MouseDown)
                {
                    if (!shell_player2.Fired && reloadP2>Player2.ReloadSpeed)
                    {
                        reloadP2 = 0;
                        shell_player2.Fired = true;
                        playSound(3);
                        GameMethods.WeaponPosition(shell_player2, Player2_turret);
                        Game.StartScript(ShootingP2);
                    }
                    else if (!shell2_player2.Fired && reloadP2 > Player2.ReloadSpeed)
                    {
                        reloadP2 = 0;
                        shell2_player2.Fired = true;
                        playSound(3);
                        GameMethods.WeaponPosition(shell2_player2, Player2_turret);
                        Game.StartScript(ShootingP2_Shell2);
                    }
                }
                Player2_turret.PointToSprite(Player1);
                GameMethods.OutOfBounds(Player2);
                Wait(0.07);
            }
            return 0;
        }
        /// <summary>  
        ///  Pucanje Player2   
        /// </summary>
        private int ShootingP2()
        {
            while (shell_player2.Fired)
            {
                shell_player2.MoveSteps(shell_player2.Speed);
                GameMethods.EnemyHit(shell_player2, Player1, shield);
                GameMethods.BoulderHit(shell_player2, boulders);
                GameMethods.OutOfBounds(shell_player2);
                Wait(0.04);
            }

            return 0;
        }
        /// <summary>  
        ///  Pucanje igrac2 2 raketa   
        /// </summary>
        private int ShootingP2_Shell2()
        {
            while (shell2_player2.Fired)
            {
                shell2_player2.MoveSteps(shell2_player2.Speed);
                GameMethods.EnemyHit(shell2_player2, Player1, shield);
                GameMethods.BoulderHit(shell2_player2, boulders);
                GameMethods.OutOfBounds(shell2_player2);
                Wait(0.04);
            }

            return 0;
        }

        //Skripta za Menu
        /// <summary>  
        ///  Skripta  za botune 
        /// </summary>
        private int Buttons() 
        {
            while (MMenuStart)
            {
                if (sensing.MouseDown)
                {
                    if (btnSingle.Clicked(sensing.Mouse) && !textBox1.Visible)
                    {
                        GameMethods.Hide(btnSingle);
                        GameMethods.Hide(btnMulti);
                        GameMethods.Hide(btnRules);
                        GameMethods.Hide(btnHigh);
                        GameMethods.Hide(btnExit);
                        MMenuStart = false;
                        StartSinglePlayer();
                        break;
                    }
                    else if (btnExit.Clicked(sensing.Mouse)) Application.Exit();
                    else if (btnMulti.Clicked(sensing.Mouse) && !textBox1.Visible)
                    {
                        GameMethods.Hide(btnSingle);
                        GameMethods.Hide(btnMulti);
                        GameMethods.Hide(btnRules);
                        GameMethods.Hide(btnHigh);
                        GameMethods.Hide(btnExit);
                        MMenuStart = false;
                        StartMultiPlayer();
                        break;
                    }
                    else if (btnRules.Clicked(sensing.Mouse)) GameMethods.OpenRules("rules.txt");
                    else if (btnHigh.Clicked(sensing.Mouse)) GameMethods.OpenHighscores("highscores.txt");
                }
                Wait(0.05);
            }
            return 0;
        }

        //---Pomocne metode-----

        /// <summary>  
        ///  Main Menu metoda   
        /// </summary>
        public void MainMenu()
        {

            EndGame();
            MMenuStart = true;
            loopSound(0);
            setBackgroundPicture("backgrounds\\background.jpg");
            setPictureLayout("stretch");
            btnSingle.X = 50;
            btnSingle.Y = 100;
            btnMulti.X = 50;
            btnMulti.Y = 150;
            btnRules.X = 50;
            btnRules.Y = 200;
            btnHigh.X = 50;
            btnHigh.Y = 250;
            btnExit.X = 50;
            btnExit.Y = 300;
            Game.StartScript(Buttons);
        }
        /// <summary>  
        ///  Metoda za pokretanje Singleplayer dijela igre   
        /// </summary>
        public void StartSinglePlayer()
        {
            stopSound(0);
            SingleStart = true;
            if(rng.Next(0,2)==0) setBackgroundPicture("backgrounds\\grass.jpg");
            else setBackgroundPicture("backgrounds\\desert.jpg");
            setPictureLayout("stretch");
            Player1.SetVisible(false);
            Player1_turret.SetVisible(false);
            Enemy1.SetVisible(false);
            Enemy1_turret.SetVisible(false);
            GameMethods.PlacaLakesBoulders(lakes, boulders);
            do
            {
                Player1.X = rng.Next(0, GameOptions.RightEdge / 2);
                Player1.Y = rng.Next(0, GameOptions.DownEdge / 2);
            } while (!GameMethods.NoCollisions(Player1, boulders, lakes));
            Wait(1);
            GameMethods.Respawn(Player1, Enemy1, Enemy2, Enemy3, Tiger2, Tiger3, boulders, lakes);
            Game.StartScript(Player_movingP1);
            Game.StartScript(Enemy_moving1);
            Game.StartScript(Enemy1Dead);
            Game.StartScript(PlacePickUps);
            Game.StartScript(TouchPickUps);
            Enemy1.SetVisible(true);
            Enemy1_turret.SetVisible(true);
            Player1.SetVisible(true);
            Player1_turret.SetVisible(true);
        }
        /// <summary>  
        ///  Metoda za pokretanje Multiplayer igre   
        /// </summary>
        public void StartMultiPlayer()
        {
            stopSound(0);
            MultiStart = true;
            //U multiplayeru igraci imaju vise healtha
            Player1.Health = 5;
            Player2.Health = 5;
            if (rng.Next(0, 2) == 0) setBackgroundPicture("backgrounds\\grass.jpg");
            else setBackgroundPicture("backgrounds\\desert.jpg");
            setPictureLayout("stretch");
            Player1.SetVisible(false);
            Player1_turret.SetVisible(false);
            Player2.SetVisible(false);
            Player2_turret.SetVisible(false);
            GameMethods.PlacaLakesBoulders(lakes, boulders);
            do
            {
                Player1.X = rng.Next(0, GameOptions.RightEdge / 2);
                Player1.Y = rng.Next(0, GameOptions.DownEdge / 2);
            } while (!GameMethods.NoCollisions(Player1, boulders, lakes));
            Wait(1);
            GameMethods.Respawn(Player1, Player2, Enemy2, Enemy3, Tiger2, Tiger3, boulders, lakes);
            Game.StartScript(Player_movingP1_Multy);
            Game.StartScript(Player_movingP2_Multy);
            Player1.SetVisible(true);
            Player1_turret.SetVisible(true);
            Player2.SetVisible(true);
            Player2_turret.SetVisible(true);
        }
        /// <summary>  
        ///  Metoda za pospremanje svih likova i vracanje na prvobitno stanje   
        /// </summary>
        public void EndGame()
        {
            ISPIS = "";
            GameMethods.Hide(Player1);
            GameMethods.Hide(Player2);
            GameMethods.Hide(Enemy1);
            GameMethods.Hide(Enemy2);
            GameMethods.Hide(Enemy3);
            GameMethods.Hide(Player1_turret);
            GameMethods.Hide(Player2_turret);
            GameMethods.Hide(Enemy1_turret);
            GameMethods.Hide(Enemy2_turret);
            GameMethods.Hide(Enemy3_turret);
            GameMethods.Hide(shell_enemy1);
            GameMethods.Hide(shell2_enemy1);
            GameMethods.Hide(shell_enemy2);
            GameMethods.Hide(shell2_enemy2);
            GameMethods.Hide(shell_enemy3);
            GameMethods.Hide(shell2_enemy3);
            GameMethods.Hide(shell_player1);
            GameMethods.Hide(shell2_player1);
            GameMethods.Hide(shell_player2);
            GameMethods.Hide(shell2_player2);
            GameMethods.Hide(Tiger1);
            GameMethods.Hide(Tiger2);
            GameMethods.Hide(Tiger3);
            GameMethods.Hide(Tiger1_turret);
            GameMethods.Hide(Tiger2_turret);
            GameMethods.Hide(Tiger3_turret);
            GameMethods.Hide(map); GameMethods.Hide(life); GameMethods.Hide(shield);
            shield.IsEquiped = false;
            for (int i = 0; i < 4; i++)
            {
                GameMethods.Hide(boulders[i]);
                GameMethods.Hide(lakes[i]);
            }
            Player1.KillCount = 0;
            Enemy1.DeathCount = 0;
            Enemy2.DeathCount = 0;
            Enemy3.DeathCount = 0;
            Player1.Health = 3;
            Player2.Health = 3;
            Player1.Points = 0;
            Tiger1.Health = 3; Tiger2.Health = 3;  Tiger3.Health = 3;
            Enemy1.Health = 1; Enemy2.Health = 1; Enemy3.Health = 1;
            Enemy1.IsTiger = false; Enemy2.IsTiger = false; Enemy3.IsTiger = false;
            shell_enemy1.Fired = false; shell_enemy2.Fired = false; shell_enemy2.Fired = false;
            shell_enemy1.Damage = 1; shell_enemy2.Damage = 1; shell_enemy2.Damage = 1;

        }
        
        /* ------------ GAME CODE END ------------ */


    }
}
