namespace RPG.Heroes
{
    using Players;

    public class Archer : BasePlayer
    {
        public Archer()
        {
            Strength = 2;
            Agility = 4;
            Intelligence = 0;
            Range = 2;  
        }

        public override string ToString()
        {
            return "#";
        }
    }
}

