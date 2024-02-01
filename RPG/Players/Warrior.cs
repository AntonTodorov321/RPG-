namespace RPG.Heroes
{
    using Players;

    public class Warrior : BasePlayer
    {
        public Warrior()
        {
            Strength = 3;
            Agility = 3;
            Intelligence = 0;
            Range = 1;
        }
        public override string ToString()
        {
            return "@";
        }
    }
}
