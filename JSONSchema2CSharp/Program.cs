using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

using System;

namespace JSONSchema2CSharp
{
    class Program
    {
        static void Main(string[] cmd)
        {
            Console.WriteLine("JSONSchema to C# class");
            Devmasters.Args args = new Devmasters.Args(cmd, new string[] { "/schema" });

            string schema = "";
            string fn = "";
            string schemaSource = args["/schema"];
            if (schemaSource.StartsWith("http"))
            {
                schema = new System.Net.WebClient().DownloadString(schemaSource);
                fn = System.IO.Path.GetFileName(new Uri(schemaSource).LocalPath);
            }
            else
            {
                schema = System.IO.File.ReadAllText(schemaSource);
                fn = System.IO.Path.GetFileName(schemaSource);
            }
            if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(fn))
            { 
                Console.WriteLine("No schema available");
                return;
            }
            fn = fn + ".cs";
            var sch = JsonSchema.FromSampleJson(schema);
            var classGenerator1 = new CSharpGenerator(sch, new CSharpGeneratorSettings
            {
                ClassStyle = CSharpClassStyle.Poco,
            });
            var codeFile1 = classGenerator1.GenerateFile();

            //var classGenerator2 = new CSharpGenerator(sch, new CSharpGeneratorSettings
            //{
            //    ClassStyle = CSharpClassStyle.Inpc,
            //});
            //var codeFile2 = classGenerator2.GenerateFile();
            //var classGenerator3 = new CSharpGenerator(sch, new CSharpGeneratorSettings
            //{
            //    ClassStyle = CSharpClassStyle.Prism,
            //});
            //var codeFile3 = classGenerator3.GenerateFile();

            //var classGenerator4 = new CSharpGenerator(sch, new CSharpGeneratorSettings
            //{
            //    ClassStyle = CSharpClassStyle.Record,
            //});
            //var codeFile4 = classGenerator4.GenerateFile();

            System.IO.File.WriteAllText(fn, codeFile1);
        }
    }
}
