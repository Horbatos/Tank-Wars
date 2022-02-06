using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    /// <summary>  
    ///  Klasa za oruzje igraca i drugih tenkova   
    /// </summary> 
    class Weapon : Sprite
    {
        public Weapon(string picture, int xkor,int ykor): base(picture, xkor, ykor)
        {
            this.Damage = 1;
            this.Speed = 8;
        }

        private int _speed;

        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        private int _damage;

        public int Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }

        private bool _fired;

        public bool Fired
        {
            get { return _fired; }
            set { _fired = value; }
        }
        
    }
    /// <summary>  
    ///  Zlato, Stit i Zivot   
    /// </summary> 
    abstract class PickUp: Sprite
    {
        public PickUp(string picture, int xkor, int ykor) : base(picture, xkor, ykor)
        {

        }

    }
    /// <summary>  
    ///  Zlato koje igrac moze skupiti   
    /// </summary> 
    class Map : PickUp
    {
        public Map(string picture, int xkor, int ykor) : base(picture, xkor, ykor)
        {
            this.Value = 200;
        }
        public readonly int Value;
    }
    /// <summary>  
    ///  Stit koje igrac moze skupiti   
    /// </summary> 
    class Shield : PickUp
    {

        public bool IsEquiped;
        public Shield(string picture, int xkor, int ykor) : base(picture, xkor, ykor)
        {
            IsEquiped = false;
        }
    }
    /// <summary>  
    ///  Zivot koje igrac moze skupiti   
    /// </summary> 
    class Life : PickUp
    {
        public Life(string picture, int xkor, int ykor) : base(picture, xkor, ykor)
        {
            Heal = 1; ;
        }
        public readonly int Heal;
    }

}
