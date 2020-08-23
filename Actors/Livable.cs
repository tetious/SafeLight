namespace Safelight.Actors
{
    public class Livable
    {
        public int MaxHp { get; set; } = 1000;

        public int Hp { get; set; }

        public bool Alive => this.Hp > 0;
    }
}
