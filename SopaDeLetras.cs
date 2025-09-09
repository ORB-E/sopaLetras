using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiProyecto {
    public class SopaDeLetras {
    private char[,] sopa;
    private Dictionary<string, string> palabrasDefiniciones;
    // Guarda las posiciones de cada palabra: palabra -> lista de posiciones (x, y)
    private Dictionary<string, List<(int x, int y)>> posicionesPalabras = new Dictionary<string, List<(int x, int y)>>();
        private int filas;
        private int columnas;
        private Random random = new Random();

        public SopaDeLetras(int filas, int columnas) {
            this.filas = filas;
            this.columnas = columnas;
            sopa = new char[filas, columnas];
            palabrasDefiniciones = new Dictionary<string, string>();
        }

        public void GenerarSopa(string archivo) {
            palabrasDefiniciones.Clear();
            posicionesPalabras.Clear();
            // Leer todas las palabras y definiciones del archivo
            var todasPalabras = new List<(string palabra, string definicion)>();
            foreach (var linea in File.ReadLines(archivo)) {
                var partes = linea.Split(',');
                if (partes.Length >= 2) {
                    var palabra = partes[0].Trim().ToUpper();
                    var definicion = partes[1].Trim();
                    if (!string.IsNullOrEmpty(palabra))
                        todasPalabras.Add((palabra, definicion));
                }
            }

            // Determinar cantidad de palabras según el tamaño de la sopa
            int cantidad = 15; // EASY por defecto
            if (filas >= 30 && columnas >= 30) cantidad = 20; // MEDIUM
            if (filas >= 50 && columnas >= 50) cantidad = 30; // HARD

            // Seleccionar aleatoriamente las palabras
            var seleccionadas = todasPalabras.OrderBy(x => random.Next()).Take(cantidad).ToList();
            foreach (var (palabra, definicion) in seleccionadas) {
                if (!palabrasDefiniciones.ContainsKey(palabra))
                    palabrasDefiniciones.Add(palabra, definicion);
            }

            // Inicializar sopa con letras aleatorias
            for (int i = 0; i < filas; i++)
                for (int j = 0; j < columnas; j++)
                    sopa[i, j] = (char)('A' + random.Next(0, 26));
            // Insertar palabras
            foreach (var palabra in palabrasDefiniciones.Keys) {
                InsertarPalabra(palabra);
            }
        }

        private void InsertarPalabra(string palabra) {
            // Solo horizontal y vertical para simplicidad
            bool insertada = false;
            for (int intento = 0; intento < 200 && !insertada; intento++) {
                int dir = random.Next(0, 2); // 0: horizontal, 1: vertical
                int x = random.Next(0, filas - (dir == 1 ? palabra.Length : 0));
                int y = random.Next(0, columnas - (dir == 0 ? palabra.Length : 0));
                bool puede = true;
                var posiciones = new List<(int x, int y)>();
                for (int k = 0; k < palabra.Length; k++) {
                    int xi = dir == 0 ? x : x + k;
                    int yj = dir == 0 ? y + k : y;
                    // Solo permite si la celda está vacía o coincide con la letra
                    if (sopa[xi, yj] != palabra[k] && char.IsLetter(sopa[xi, yj]) && sopa[xi, yj] != (char)('A' + random.Next(0, 26))) {
                        puede = false;
                        break;
                    }
                    posiciones.Add((xi, yj));
                }
                if (puede) {
                    for (int k = 0; k < palabra.Length; k++) {
                        int xi = dir == 0 ? x : x + k;
                        int yj = dir == 0 ? y + k : y;
                        sopa[xi, yj] = palabra[k];
                    }
                    posicionesPalabras[palabra] = posiciones;
                    insertada = true;
                }
            }
            // Si no se pudo insertar, eliminar el registro de posiciones
            if (!insertada && posicionesPalabras.ContainsKey(palabra))
                posicionesPalabras.Remove(palabra);
        }

        public void MostrarSopa() {
            // Obtener palabras encontradas si existen
            var encontradas = palabrasEncontradas ?? new HashSet<string>();
            for (int i = 0; i < filas; i++) {
                for (int j = 0; j < columnas; j++) {
                    bool esEncontrada = false;
                    foreach (var palabra in encontradas) {
                        if (posicionesPalabras.ContainsKey(palabra) && posicionesPalabras[palabra].Contains((i, j))) {
                            esEncontrada = true;
                            break;
                        }
                    }
                    if (esEncontrada) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(sopa[i, j] + " ");
                        Console.ResetColor();
                    } else {
                        Console.Write(sopa[i, j] + " ");
                    }
                }
                Console.WriteLine();
            }
        }

        public void Jugar() {
            Console.WriteLine("Ingresa las palabras encontradas (escribe 'salir' para terminar):");
            var encontradas = new HashSet<string>();
            // Solo palabras realmente insertadas en la sopa
            var palabrasEnSopa = posicionesPalabras.Keys.ToList();
            while (encontradas.Count < palabrasEnSopa.Count) {
                // Mostrar pista (definición) de una palabra no encontrada
                var siguiente = palabrasEnSopa.FirstOrDefault(p => !encontradas.Contains(p));
                if (siguiente != null && palabrasDefiniciones.ContainsKey(siguiente))
                    Console.WriteLine($"Pista: {palabrasDefiniciones[siguiente]}");
                string entrada = (Console.ReadLine() ?? "").ToUpper();
                if (entrada == "SALIR") break;
                if (palabrasEnSopa.Contains(entrada) && !encontradas.Contains(entrada)) {
                    encontradas.Add(entrada);
                    Console.WriteLine($"¡Correcto! ({encontradas.Count}/{palabrasEnSopa.Count})");
                    // Mostrar sopa con letras encontradas en verde
                    palabrasEncontradas = encontradas;
                    MostrarSopa();
                } else {
                    Console.WriteLine("Incorrecto o ya encontrada.");
                }
            }
            Console.WriteLine("Juego terminado. Palabras encontradas:");
            foreach (var palabra in encontradas)
                Console.WriteLine("- " + palabra);
            palabrasEncontradas = null;
        }

        // Variable para pasar palabras encontradas a MostrarSopa
        private HashSet<string>? palabrasEncontradas = null;
    }
}
