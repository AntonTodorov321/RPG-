namespace RPG.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Player
    {
        public Player()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public int Range { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public int Damage { get; set; }

    }
}
