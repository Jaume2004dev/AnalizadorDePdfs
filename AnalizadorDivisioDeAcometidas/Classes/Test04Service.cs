using AnalizadorDivisioDeAcometidas.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace AnalizadorDivisiónDeAcometidas.Clases
{
    internal class Test04Service
    {
        private Test04AppSettings configuracion;

        public Test04Service(Test04AppSettings configuracionTest04)
        {
            this.configuracion = configuracionTest04;
        }

        public void Iniciar()
        {
            string rutaArchivo = Path.GetFullPath(configuracion.rutaPDF);

            EjecutarAnalizadorPdf(rutaArchivo);
        }

        static void EjecutarAnalizadorPdf(string rutaArchivo)
        {
            Dictionary<Palabra, Dictionary<Palabra, Palabra>> diccionario = new();
            string textoExtraído = ExtraerTexto(rutaArchivo);

            string[] categorias_string = ObtenerNombresCategorías();
            List<Palabra> categorias = new();
            List<Palabra> claves = new();
            List<Palabra> valores = new();

            ExtraerPalabras(rutaArchivo, claves, valores);
            FusionarPalabras(claves);
            FusionarPalabras(valores);

            LimpiarClaves(claves, categorias_string);
            IdentificarCategorías(claves, valores, categorias, diccionario, categorias_string);

            AsignarCategoría(claves, categorias);
            AsignarCategoría(valores, categorias);

            EmparejarClavesValor(claves, valores, diccionario);
            AgregarClavesResultantes(claves, diccionario);

            ImprimirDiccionario(diccionario);
        }

        static string ExtraerTexto(string ruta)
        {
            string texto = "";
            using var documento = PdfDocument.Open(ruta);
            foreach (var página in documento.GetPages())
            {
                texto += página.Text + Environment.NewLine;
            }
            return texto;
        }

        static string[] ObtenerNombresCategorías() => new string[]
        {
            "PCT -IC-(NPC)", "Dirección Finca:", "Dirección punto de acometida:", "Peticionario:",
            "Datos contacto:", "Datos tubería existente:", "Datos nueva acometida:", "Datos nuevo contador",
            "OBRAS NECESARIAS", "OBRAS COMPLEMENTARIAS NECESARIAS", "Observaciones de la inspección",
            "Observaciones para la contrata"
        };

        static void ExtraerPalabras(string ruta, List<Palabra> claves, List<Palabra> valores)
        {
            int númeroPágina = 0;
            using var documento = PdfDocument.Open(ruta);
            foreach (var página in documento.GetPages())
            {
                númeroPágina++;
                foreach (var palabra in página.GetWords())
                {
                    var pos = palabra.BoundingBox;
                    string texto = palabra.Text.Trim();
                    float altura = (float)(pos.Top - pos.Bottom);
                    float ancho = (float)(pos.Right - pos.Left);
                    var posición = new Vector2((float)pos.Left, (float)pos.Bottom);
                    bool esNegrita = (palabra.FontName ?? "").Equals("CIDFont+F2");
                    string[] palabrasIgnoradas = { ":", "/", "-", "er", "or" };

                    if (!palabrasIgnoradas.Contains(texto))
                    {
                        var palabraObj = new Palabra(texto, posición, altura, ancho, númeroPágina);
                        if (esNegrita)
                            valores.Add(palabraObj);
                        else
                            claves.Add(palabraObj);
                    }
                }
            }
        }

        static void FusionarPalabras(List<Palabra> palabras)
        {
            List<Palabra> aEliminar = new();
            foreach (var palabra in palabras)
            {
                foreach (var otra in palabras)
                {
                    if (palabra == otra) continue;
                    var delta = otra.posicion - (palabra.posicion + new Vector2(palabra.anchura, 0));
                    float maxDeltaX = (palabra.anchura / palabra.texto.Count()) * 2.5f;

                    if (Math.Abs(delta.Y) < (palabra.altura / 2) && (delta.X > 0) && (delta.X < maxDeltaX) && palabra.numeroPagina == otra.numeroPagina)
                    {
                        palabra.texto += " " + otra.texto;
                        palabra.anchura = (otra.posicion.X + otra.anchura) - palabra.posicion.X;
                        aEliminar.Add(otra);
                    }
                }
            }
            foreach (var eliminar in aEliminar) palabras.Remove(eliminar);
        }

        static void LimpiarClaves(List<Palabra> claves, string[] categorias)
        {
            claves.RemoveAll(k => k.texto.Split(' ').Length == 1 && !k.texto.Contains(":") && !categorias.Contains(k.texto));
        }

        static void IdentificarCategorías(List<Palabra> claves, List<Palabra> valores, List<Palabra> categorias, Dictionary<Palabra, Dictionary<Palabra, Palabra>> diccionario, string[] categorias_string)
        {
            foreach (var palabra in valores.ToList())
            {
                if (categorias_string.Contains(palabra.texto))
                {
                    categorias.Add(palabra);
                    valores.Remove(palabra);
                }
            }

            foreach (var palabra in claves.ToList())
            {
                if (categorias_string.Contains(palabra.texto))
                {
                    categorias.Add(palabra);
                    claves.Remove(palabra);
                }
            }

            categorias = categorias.OrderByDescending(c => c.posicion.Y).OrderBy(c => c.numeroPagina).ToList();

            foreach (Palabra categoria in categorias) diccionario[categoria] = new();
        }

        static void AsignarCategoría(List<Palabra> palabras, List<Palabra> categorias)
        {
            string[] categorias_str = ObtenerNombresCategorías();
            foreach (var palabra in palabras)
            {
                Palabra másCercana = null;

                Palabra categoriaEncimaPalabra = categorias.Where(cat => (cat.posicion.Y - palabra.posicion.Y > 0) && (cat.numeroPagina == palabra.numeroPagina)).ToList().OrderBy(cat => (cat.posicion.Y - palabra.posicion.Y)).ToList().FirstOrDefault();

                palabra.categoria = categoriaEncimaPalabra;
            }
        }

        static void EmparejarClavesValor(List<Palabra> claves, List<Palabra> valores, Dictionary<Palabra, Dictionary<Palabra, Palabra>> diccionario)
        {
            foreach (var valor in valores)
            {
                Palabra claveMásCercana = null;
                Vector2 mejorDistancia = default;

                foreach (var clave in claves)
                {
                    if (claveMásCercana == null) { claveMásCercana = clave; mejorDistancia = valor.posicion - clave.posicion; continue; }

                    var dist = valor.posicion - clave.posicion;
                    if (dist.X < mejorDistancia.X && Math.Abs(dist.Y) < clave.altura / 2 && dist.X > 0 && valor.numeroPagina == clave.numeroPagina || mejorDistancia.X < 0 || mejorDistancia.Y < 0)
                    {
                        claveMásCercana = clave;
                        mejorDistancia = dist;
                    }
                }

                if (Math.Abs(mejorDistancia.Y) < (valor.altura / 2) && valor.numeroPagina == claveMásCercana.numeroPagina)
                {
                    if (!diccionario[claveMásCercana.categoria].TryGetValue(claveMásCercana, out var existencia))
                    {
                        diccionario[claveMásCercana.categoria][claveMásCercana] = valor;
                    }
                    else
                    {
                        existencia.texto += " " + valor.texto;
                    }
                }
            }

            Palabra palabraDiccionario = null;
            foreach (var par in diccionario)
            {
                if (par.Value.Count == 0)
                {
                    par.Key.tieneValorMultilínea = true;
                    List<Palabra> clavesAEliminar = new List<Palabra>();
                    var palabras = claves.Concat(valores).ToList();

                    foreach (var palabra in palabras)
                    {
                        if (palabra.categoria == par.Key)
                        {
                            if (diccionario[par.Key].TryGetValue(par.Key, out palabraDiccionario))
                            {
                                palabraDiccionario.texto += "\n" + palabra.texto;
                            }
                            else
                            {
                                diccionario[par.Key][par.Key] = palabra;
                            }
                        }
                    }
                }
                else
                {
                    par.Key.tieneValorMultilínea = false;
                }
            }
        }

        static void AgregarClavesResultantes(List<Palabra> claves, Dictionary<Palabra, Dictionary<Palabra, Palabra>> diccionario)
        {
            foreach (var categoria in diccionario.Keys)
            {
                if (!categoria.tieneValorMultilínea)
                {
                    var existencia = diccionario[categoria].Keys;
                    var faltantes = claves.Where(k => k.categoria == categoria && !existencia.Contains(k));
                    foreach (var m in faltantes)
                    {
                        diccionario[categoria][m] = new Palabra();
                    }
                }
            }
        }

        static void ImprimirDiccionario(Dictionary<Palabra, Dictionary<Palabra, Palabra>> diccionario)
        {
            Console.WriteLine();
            foreach (var cat in diccionario)
            {
                Console.WriteLine($"\n\nCategoría: {cat.Key.texto}");
                foreach (var par in cat.Value)
                {
                    Console.WriteLine($"Clave: {par.Key.texto} | Valor: {par.Value.texto}");
                }
            }
        }
    }
}