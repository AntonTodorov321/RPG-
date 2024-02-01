namespace RPG.Heroes
{
    using Players;

    public class Mage : BasePlayer
    {
        public Mage()
        {
            Strength = 2;
            Agility = 1;
            Intelligence = 3;
            Range = 3;
        }

        public override string ToString()
        {
            return "*";
        }
    }
}
