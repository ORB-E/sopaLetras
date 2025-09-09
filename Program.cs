using static MiProyecto.Utils;

namespace MiProyecto {
    class Program {
        static void Main(string[] args)
        {
            WriteLine("------------SOPA DE LETRAS------------");

            WriteLine(" [1] - EASY\n [2] - MEDIUM\n [3] - HARD\n [0] - Salir");
            string opcion = ReadLine();
            while (opcion != "0")
            {
                if (opcion == "1")
                {
                    SopaDeLetras sopa = new SopaDeLetras(20, 20);
                    sopa.GenerarSopa("easy.txt");
                    sopa.MostrarSopa();
                    sopa.Jugar();
                }
                else if (opcion == "2")
                {
                    SopaDeLetras sopa = new SopaDeLetras(30, 30);
                    sopa.GenerarSopa("medium.txt");
                    sopa.MostrarSopa();
                    sopa.Jugar();
                }
                else if (opcion == "3")
                {
                    SopaDeLetras sopa = new SopaDeLetras(50, 50);
                    sopa.GenerarSopa("hard.txt");
                    sopa.MostrarSopa();
                    sopa.Jugar();
                }
                else
                {
                    WriteLine("Opcion no valida");
                }

                WriteLine(" [1] - EASY (20x20)\n [2] - MEDIUM (30x30)\n [3] - HARD (50x50)\n [0] - Salir");
                opcion = ReadLine();
            }            

        }
    }
}