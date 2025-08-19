// See https://aka.ms/new-console-template for more information
using AnalizadorDivisioDeAcometidas.Classes;
using AnalizadorDivisiónDeAcometidas.Clases;
using System.ComponentModel.Design;
using System.Numerics;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Fonts;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace AnalizadorDivisioDeAcometidas.Classes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test04Bootstrapper test04Bootstrapper = new Test04Bootstrapper();
            Test04Service test04Service = new Test04Service(test04Bootstrapper.appSettings);
            test04Service.Iniciar();
        }
    }
}
