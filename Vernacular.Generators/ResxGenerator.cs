using System;
using System.Text;
using System.Resources;

namespace Vernacular.Generators
{
    public sealed class ResxGenerator : Generator
    {
        protected override void Generate ()
        {
            using (var resx = new ResXResourceWriter (Writer)) {
                foreach (var localized_string in Strings) {
                    foreach (var resource_string in GetResourceStrings (localized_string)) {
                        if (!HasResourceStringBeenGenerated (resource_string)) {
                            MarkResourceStringAsGenerated (resource_string);
                            resx.AddResource (new ResXDataNode (resource_string.Id, resource_string.Translated));
                        }
                    }
                }
            }
        }
    }
}