namespace RPG.Heroes
{
    using Players;

    public class Monster : BasePlayer
    {
        public Monster()
        {
            Strength = new Random().Next(1, 4);
            Agility = new Random().Next(1, 4);
            Intelligence = new Random().Next(1, 4);
            Range = 1;
        }

        public override string ToString()
        {
            return "\u25D9";
        }
    }
}
