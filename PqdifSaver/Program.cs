// See https://aka.ms/new-console-template for more information
using Gemstone.PQDIF.Logical;
using Microsoft.VisualBasic;
using PQDIF_Manager;




PqdifFile pqdifFile = await PqdifFile.LoadFromFileAsync(@"C:\Users\Jura\Desktop\TestBench\PSL_P3003845_2025-04-12_10Min_ClassA_PQDIF.pqd");
Console.WriteLine($"Loaded PQDIF file: {pqdifFile.FilePath}");
var e = Equipment.GetInfo(new Guid("06afbbc3-35a9-4840-96f9-3b64381bde51"));
var v = Vendor.GetInfo(new Guid("4e354a9b-2ad0-463f-9c08-52a8ff9d5149"));
