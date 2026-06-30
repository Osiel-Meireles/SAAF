using System;
using System.IO;
using UglyToad.PdfPig;

namespace PdfSeeder
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdfPath = @"C:\Users\osiel\RiderProjects\Sakrus\Listar lotes.pdf";
            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                // Print the text of the first 2 pages
                for (int i = 1; i <= Math.Min(2, document.NumberOfPages); i++)
                {
                    var page = document.GetPage(i);
                    Console.WriteLine($"--- PAGE {i} ---");
                    Console.WriteLine(page.Text);
                }
            }
        }
    }
}
