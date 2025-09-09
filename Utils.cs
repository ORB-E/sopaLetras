using System;

namespace MiProyecto {
    public static class Utils {
        // Función simplificada para Console.ReadLine
        public static string ReadLine() {
            return Console.ReadLine();
        }

        // Función para leer y convertir a entero
        public static int ReadInt() {
            return Convert.ToInt32(Console.ReadLine());
        }

        // Función para leer y convertir a double
        public static double ReadDouble() {
            return Convert.ToDouble(Console.ReadLine());
        }

        // Función simplificada para Console.WriteLine
        public static void WriteLine(string message) {
            Console.WriteLine(message);
        }
        public static void WriteLine(object obj) {
            Console.WriteLine(obj);
        }
    }
}