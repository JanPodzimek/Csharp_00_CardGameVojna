namespace Vojna;
public class Program {
    public static void Main(string[] args) {
        Player player1 = new Player();
        Player player2 = new Player();
        Dice dice = new Dice();
        Table table = new Table(dice, player1, player2);
        table.Game();
    }
}

