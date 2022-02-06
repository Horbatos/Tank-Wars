using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    /// <summary>  
    ///  Klasa za sve vrste tenkova: Igrac, Neprijatelj i Tigar  
    /// </summary>  
    abstract class Tank: Sprite
    {
        public event EventHandler _died;

        protected int _health;

        public int Health
        {
            get { return _health; }
            set
            {
                if (value <= 0) { IsDead = true; _died(this, null); }
                else _health = value;
            }
        }

        private int _reloadSpeed;

        public int ReloadSpeed
        {
            get { return _reloadSpeed; }
            set { _reloadSpeed = value; }
        }

        private int _speed;
        
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        public bool IsDead;
        public Tank(string picture, int k_x, int k_y): base(picture,k_x,k_y)
        {
        }
    }

    class PlayerTank: Tank
    {
        private int _points;

        public int Points
        {
            get { return _points; }
            set { _points = value; }
        }


        private bool _hasPickup;

        public bool HasPickup
        {
            get { return _hasPickup; }
            set { _hasPickup = value;}
        }
        
        private int _killCount;

        public int KillCount
        {
            get { return _killCount; }
            set { _killCount = value; }
        }

        public PlayerTank(string picture, int k_x, int k_y): base(picture,k_x,k_y)
        {
            this.Health = 3;
            this.Speed = 4;
            this.KillCount = 0;
            this.IsDead = false;
            this.HasPickup = false;
            this.ReloadSpeed = 120;
        }
    }

    class EnemyTank: Tank
    {
        private int _deathCount;
        public bool IsTiger;
        public int DeathCount
        {
            get { return _deathCount; }
            set { _deathCount = value; }
        }
        public EnemyTank(string picture, int k_x, int k_y): base(picture,k_x,k_y)
        {
            this.Health = 1;
            this.Speed = 4;
            this.IsTiger = false;
            this.IsDead = false;
            this.DeathCount = 0;
            this.ReloadSpeed = 75;
        }
        
    }

    class TigerTank : EnemyTank
    {
        public TigerTank(string picture, int k_x, int k_y) : base(picture, k_x, k_y)
        {
            this.Health = 3;
            this.Speed = 3;
            this.IsDead = false;
            this.DeathCount = 0;
            this.ReloadSpeed = 60;
        }

    }
}
