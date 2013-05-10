using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

// TODO: replace this with the type you want to import.
using TImport = System.String;

namespace CustomImporters
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".txt", ".rtf", DisplayName = "Text Importer", DefaultProcessor = "CustomImporters.TextProcessor")]
    public class textImporter : ContentImporter<TImport>
    {
        public override TImport Import(string filename, ContentImporterContext context)
        {
            StreamReader bin = new StreamReader(File.OpenRead(filename));
            TImport ret = bin.ReadToEnd();
            bin.Close();
            return ret;
        }
    }
}
