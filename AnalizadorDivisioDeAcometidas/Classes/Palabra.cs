using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorDivisioDeAcometidas.Classes
{
    internal class Palabra
    {
        public string texto { get; set; }
        public Vector2 posicion { get; set; }
        public float altura { get; set; }
        public float anchura { get; set; }
        public int numeroPagina { get; set; }
        public Palabra categoria { get; set; }
        public bool tieneValorMultilínea { get; set; }
        public bool estaEnNegrita { get; set; }

        public Palabra() { }
        public Palabra(string texto, Vector2 posición, float altura, float anchura, int numeroPagina, Palabra categoria = null) {
            this.texto = texto;
            this.posicion = posición;
            this.altura = altura;
            this.anchura = anchura;
            this.numeroPagina = numeroPagina;
            this.categoria = categoria;
        }

        public override bool Equals(object obj)
        {
            if (obj is Palabra other)
            {
                return texto == other.texto &&
                       posicion.Equals(other.posicion) &&
                       numeroPagina == other.numeroPagina;
            }
            return false;
        }
    }
}
