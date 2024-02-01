using System.Text;

using RPG;
using RPG.Data;

Console.OutputEncoding = Encoding.UTF8;

var dbContext = new DataContext();

Game game = new Game(); 
game.Start(dbContext);
