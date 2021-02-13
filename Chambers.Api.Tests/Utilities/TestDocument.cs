using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Chambers.Api.Tests.Utilities
{
    public enum TestDocumentType
    {
        Pdf,
        LargePdf,
        Word
    }

    public static class TestDocument
    {
        public static byte[] Get(TestDocumentType type)
        {
            string docName = GetDocumentName(type);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{typeof(TestDocument).Namespace}.Files.{docName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var bytes = ms.ToArray();
                return ms.ToArray();
            }
        }

        private static string GetDocumentName(TestDocumentType type)
        {
            switch (type)
            {
                case TestDocumentType.Pdf:
                    return "Test.pdf";
                case TestDocumentType.LargePdf:
                    return "Test_6mb.pdf";
                case TestDocumentType.Word:
                    return "Word.docx";
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
