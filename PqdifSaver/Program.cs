// See https://aka.ms/new-console-template for more information
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




IFileVisitor fileVisitor = new BigMeasurementsFileVisitor();
string rootFolder = @"C:\Users\Jura\Desktop\P3003845"; //lokacija foldera

await fileVisitor.VisitDirectoryAsync(rootFolder);



