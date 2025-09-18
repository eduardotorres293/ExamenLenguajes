// Importación de librerías necesarias
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; // Para manejo de archivos
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; // Para crear la interfaz gráfica
using System.Xml.Linq;

namespace Practica2 // Declaración del namespace del proyecto
{
    public partial class Form1 : Form // Clase principal del formulario
    {
        // Constructor del formulario
        public Form1()
        {
            InitializeComponent(); // Inicializa los componentes del formulario
            analizarToolStripMenuItem.Enabled = false; // Desactiva la opción "Analizar" al inicio
        }

        // Lista de palabras reservadas del lenguaje C
        private List<string> P_Reservadas = new List<string>()
        {
            "auto", "break", "case", "char", "const", "continue", "default", "do",
            "double", "else", "enum", "extern", "float", "for", "goto", "if", "include",
            "inline", "int", "long", "main", "register", "restrict", "return", "short",
            "signed", "sizeof", "static", "struct", "switch", "typedef", "union",
            "unsigned", "void", "volatile", "while", "_Alignas", "_Alignof",
            "_Atomic", "_Bool", "_Complex", "_Generic", "_Imaginary", "_Noreturn",
            "_Static_assert", "_Thread_local"
        };

        // Evento para abrir un archivo .c
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog VentanaAbrir = new OpenFileDialog(); // Instancia del diálogo para abrir archivo
            VentanaAbrir.Filter = "Texto|*.c"; // Filtro para archivos .c

            if (VentanaAbrir.ShowDialog() == DialogResult.OK) // Si el usuario selecciona un archivo
            {
                archivo = VentanaAbrir.FileName; // Guarda el nombre del archivo
                using (StreamReader Leer = new StreamReader(archivo)) // Lee el archivo
                {
                    richTextBox1.Text = Leer.ReadToEnd(); // Muestra el contenido en el richTextBox
                }
            }

            Form1.ActiveForm.Text = "Mi Compilador - " + archivo; // Actualiza el título del formulario
            analizarToolStripMenuItem.Enabled = true; // Habilita la opción "Analizar"
        }

        // Método para guardar el archivo actual
        private void guardar()
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";

            if (archivo != null) // Si ya existe un archivo cargado
            {
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text); // Guarda el contenido del richTextBox en el archivo
                }
            }
            else // Si no hay archivo cargado, se abre diálogo para guardar
            {
                if (VentanaGuardar.ShowDialog() == DialogResult.OK)
                {
                    archivo = VentanaGuardar.FileName;
                    using (StreamWriter Escribir = new StreamWriter(archivo))
                    {
                        Escribir.Write(richTextBox1.Text);
                    }
                }
            }

            Form1.ActiveForm.Text = "Mi Compilador - " + archivo; // Actualiza título con el nombre del archivo
        }

        // Evento al hacer clic en "Guardar"
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guardar(); // Llama al método guardar
        }

        // Evento al hacer clic en "Nuevo"
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear(); // Limpia el contenido del editor
            archivo = null; // Resetea el nombre del archivo
        }

        // Evento al hacer clic en "Guardar Como"
        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog VentanaGuardar = new SaveFileDialog();
            VentanaGuardar.Filter = "Texto|*.c";

            if (VentanaGuardar.ShowDialog() == DialogResult.OK)
            {
                archivo = VentanaGuardar.FileName;
                using (StreamWriter Escribir = new StreamWriter(archivo))
                {
                    Escribir.Write(richTextBox1.Text); // Guarda el contenido del editor en el nuevo archivo
                }
            }

            Form1.ActiveForm.Text = "Mi Compilador - " + archivo;
        }

        // Evento al hacer clic en "Salir"
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult Salir = MessageBox.Show(
                "¿Estás seguro que deseas salir?",
                "Confirmar salida",
                MessageBoxButtons.OKCancel);

            if (Salir == DialogResult.OK)
            {
                Application.Exit(); // Sale de la aplicación
            }
        }

        // Método para clasificar el tipo de caracter leído
        private char Tipo_caracter(int caracter)
        {
            if (caracter >= 65 && caracter <= 90 || caracter >= 97 && caracter <= 122) return 'l'; // Letra
            else if (caracter >= 48 && caracter <= 57) return 'd'; // Dígito
            else
            {
                switch (caracter)
                {
                    case 10: return 'n'; // Salto de línea
                    case 34: return '"'; // Comillas dobles
                    case 39: return 'c'; // Comilla simple
                    case 32: return 'e'; // Espacio
                    case 47: return '/'; // Posible comentario
                    default: return 's'; // Otro símbolo
                }
            }
        }

        // Método para analizar símbolos válidos
        private void Simbolo()
        {
            if (i_caracter == 33 ||
                i_caracter >= 35 && i_caracter <= 38 ||
                i_caracter >= 40 && i_caracter <= 45 ||
                i_caracter == 47 ||
                i_caracter >= 58 && i_caracter <= 62 ||
                i_caracter == 91 || i_caracter == 93 ||
                i_caracter == 94 || i_caracter == 123 ||
                i_caracter == 124 || i_caracter == 125)
            {
                elemento = ((char)i_caracter).ToString() + " Símbolo\n"; // Simbolo válido
            }
            else { Error(i_caracter); } // Si no es válido, lanza error
        }

        // Método para analizar cadenas (entre comillas dobles)
        private void Cadena()
        {
            do
            {
                i_caracter = Leer.Read();
                if (i_caracter == 10) Numero_linea++; // Aumenta número de línea si hay salto
            } while (i_caracter != 34 && i_caracter != -1); // Hasta cerrar la cadena
            if (i_caracter == -1) Error(-1); // Error si no se cierra la cadena
        }

        // Método para analizar caracteres (entre comillas simples)
        private void Caracter()
        {
            i_caracter = Leer.Read(); // Lee carácter
            i_caracter = Leer.Read(); // Lee comilla de cierre
            if (i_caracter != 39) Error(39); // Error si no cierra correctamente
        }

        // Muestra error léxico en la consola y cuenta el error
        private void Error(int i_caracter)
        {
            richTextBox2.AppendText("Error léxico " + (char)i_caracter + ", línea " + Numero_linea + "\n");
            N_error++;
        }

        // Analiza si es archivo de librería ".h"
        private void Archivo_Libreria()
        {
            i_caracter = Leer.Read();
            if ((char)i_caracter == 'h') { elemento = "Archivo Libreria\n"; i_caracter = Leer.Read(); }
            else { Error(i_caracter); }
        }

        // Verifica si el elemento es una palabra reservada
        private bool Palabra_Reservada()
        {
            if (P_Reservadas.IndexOf(elemento) >= 0) return true;
            return false;
        }

        // Analiza identificadores o palabras reservadas
        private void Identificador()
        {
            do
            {
                elemento = elemento + (char)i_caracter;
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'l' || Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.') { Archivo_Libreria(); } // Si es .h
            else
            {
                if (Palabra_Reservada()) elemento = "Palabra Reservada\n";
                else elemento = "Identificador\n";
            }
        }

        // Analiza número real
        private void Numero_Real()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');
            elemento = "Numero real\n";
        }

        // Analiza número entero o real
        private void Numero()
        {
            do
            {
                i_caracter = Leer.Read();
            } while (Tipo_caracter(i_caracter) == 'd');

            if ((char)i_caracter == '.') { Numero_Real(); }
            else
            {
                elemento = "Numero entero\n";
            }
        }

        // Evento cuando se modifica el texto del editor
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            analizarToolStripMenuItem.Enabled = true; // Activa botón analizar
        }

        // Analiza si es comentario (// o /* */)
        private bool Comentario()
        {
            i_caracter = Leer.Read();

            switch (i_caracter)
            {
                case 47: // Comentario de una línea
                    do
                    {
                        i_caracter = Leer.Read();
                    } while (i_caracter != 10);
                    return true;

                case 42: // Comentario multilínea
                    do
                    {
                        do
                        {
                            i_caracter = Leer.Read();
                            if (i_caracter == 10)
                            {
                                Numero_linea++; // Aumenta número de línea
                            }
                        } while (i_caracter != 42 && i_caracter != -1);

                        i_caracter = Leer.Read();
                    } while (i_caracter != 47 && i_caracter != -1);

                    if (i_caracter == -1)
                    {
                        Error(i_caracter);
                    }

                    i_caracter = Leer.Read(); // Continúa lectura
                    return true;

                default: return false; // No es comentario
            }
        }

        // Evento al hacer clic en "Analizar"
        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = ""; // Limpia consola de salida
            guardar(); // Guarda archivo actual

            elemento = ""; // Reinicia variable de elemento
            N_error = 0; // Reinicia contador de errores
            Numero_linea = 1; // Reinicia contador de líneas

            archivoback = archivo.Remove(archivo.Length - 1) + "back"; // Crea archivo de respaldo
            Escribir = new StreamWriter(archivoback);
            Leer = new StreamReader(archivo);

            i_caracter = Leer.Read(); // Lee primer carácter

            do
            {
                elemento = "";

                if ((char)i_caracter == '/')
                {
                    if (Comentario())
                    {
                        Escribir.Write("Comentario\n"); // Escribe que es un comentario
                        continue;
                    }
                }

                switch (Tipo_caracter(i_caracter))
                {
                    case 'l': Identificador(); Escribir.Write(elemento); break;
                    case 'd': Numero(); Escribir.Write(elemento); break;
                    case 's': Simbolo(); Escribir.Write(elemento); i_caracter = Leer.Read(); break;
                    case '"': Cadena(); Escribir.Write("Cadena\n"); i_caracter = Leer.Read(); break;
                    case 'c': Caracter(); Escribir.Write("Caracter\n"); i_caracter = Leer.Read(); break;
                    case 'n': i_caracter = Leer.Read(); Numero_linea++; break;
                    case 'e': i_caracter = Leer.Read(); break;
                    default: Error(i_caracter); break;
                }
            } while (i_caracter != -1); // Hasta fin de archivo

            richTextBox2.AppendText("Errores: " + N_error); // Muestra cantidad de errores
            Escribir.Close(); // Cierra escritor
            Leer.Close(); // Cierra lector
        }
    }
}
