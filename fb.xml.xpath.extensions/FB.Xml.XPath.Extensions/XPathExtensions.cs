using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FB.Xml.XPath.Extensions
{
    public static class XPathExtensions
    {
        // Regex für die Bedingungen eines XElements
        private static Regex conditionMatch = new Regex(@"(.*?)\[([^\]]*)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex zur Extraktion der Bedingungs-XElemente und -Attribute
        private static Regex elementsAndAttributesMatch = new Regex(@"([@:.\w/_-]+)=\'([^\']+)\'", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Regex zur Aufsplittung des XPath
        private static Regex pathSplit = new Regex(@"/(?!((?![\[\]]).)*\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Dieses Zeichen bestimmt das Attributkennzeichen im XPath
        private static string attributeCharacter = "@";

        // Dieses Zeichen bestimmt den Namespace-Separator
        private static string namespaceSeparator = ":";

        public static void XPathAppendElement(this XNode node, string expression)
        {
            node.XPathAppendElement(expression, string.Empty, null);
        }
        public static void XPathAppendElement(this XNode node, string expression, string value)
        {
            node.XPathAppendElement(expression, value, null);
        }
        public static void XPathAppendElement(this XNode node, string expression, IXmlNamespaceResolver resolver)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver", "Es wurde keine NamespaceResolver angegeben.");
            }

            node.XPathAppendElement(expression, string.Empty, resolver);
        }
        public static void XPathAppendElement(this XNode node, string expression, string value, IXmlNamespaceResolver resolver)
        {
            #region Parameter-Check
            if (node == null) 
            { 
                throw new ArgumentNullException("node", "Die angegebene Node ist NULL."); 
            }
            if (node is XDocument && (node.Document == null || node.Document.Root == null)) 
            { 
                throw new InvalidOperationException("Es wird ein Root-Element benötigt."); 
            }
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException("xpath", "Es wurde kein XPath angegeben.");
            }
            #endregion Parameter-Check

            var pathElements = pathSplit.Split(expression)
                .Where(item => !string.IsNullOrEmpty(item)).ToArray();

            if (pathElements.Length > 0)
            {
                var currentElement = node as XElement;
                foreach (var pathElement in pathElements)
                {
                    // Hier wird der Elementname gesetzt, da er später überschrieben werden muss
                    var name = pathElement;

                    // Condition-Ausdruck
                    var conditionValue = string.Empty;

                    // Condition-Attribute des Elements
                    //var attributes = new Dictionary<string, string>();
                    var attributes = new Dictionary<string,string>(); 

                    // Condition-Elemente des Elements
                    var elements = new Dictionary<string, string>(); 

                    // Hat das Elemente eine Condition?
                    var conditionMatchResult = conditionMatch.Match(name);
                    if (conditionMatchResult.Success)
                    {
                        // Exakter Elementname
                        name = conditionMatchResult.Groups[1].Value;

                        // Condiotion-Ausdruck
                        conditionValue = conditionMatchResult.Groups[2].Value;

                        // Attribute und Elemente herraussuchen
                        var elementsAndAttributesMatchResult = elementsAndAttributesMatch.Matches(conditionValue);
                        if (elementsAndAttributesMatchResult.Count > 0)
                        {
                            var attributesAndElements = elementsAndAttributesMatchResult
                                .Cast<Match>()
                                    .Select(match =>
                                        new KeyValuePair<string, string>(
                                            match.Groups[1].Value,
                                            match.Groups[2].Value
                                        )
                                    );

                            // Filtern der Attribute
                            attributes = attributesAndElements
                                .Where(item => item.Key.StartsWith(XPathExtensions.attributeCharacter))
                                    .ToDictionary(kvp => kvp.Key.Replace(XPathExtensions.attributeCharacter, string.Empty), kvp => kvp.Value);

                            // Filtern der Elemente
                            elements = attributesAndElements
                                .Where(item => !item.Key.StartsWith(XPathExtensions.attributeCharacter))
                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        }
                    }

                    //Stellt fest, ob es das iterierte Element im currentElement gibt
                    string pathSelector = string.Empty;
                    if (string.IsNullOrEmpty(conditionValue))
                    {
                        pathSelector = name;
                    }
                    else
                    {
                        pathSelector = string.Concat(name, string.Format("[{0}]", conditionValue));
                    }

                    XElement selectedElement = null;
                    selectedElement = resolver != null
                        ? (currentElement ?? node).XPathSelectElement(pathSelector, resolver)
                        : (currentElement ?? node).XPathSelectElement(pathSelector);

                    // Ist das Element vorhanden, so wird es auf das aktuelle gesetzt.
                    // Ansonsten wird es neu erstellt
                    if (selectedElement == null)
                    {
                        selectedElement = XPathExtensions.GenerateNewElement(name, resolver);
                        currentElement.Add(selectedElement);
                    }

                    // Wenn das Element das Letzte in der Kette ist, dann wird dort der Wert gesetzt
                    if (pathElements[pathElements.Length - 1].Equals(pathSelector, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (elements.Any())
                        {
                            throw new InvalidOperationException("Um das Element zu generieren, darf am letzten Element des Pfades keine Bedingung mit Sub-Elementen vorhanden sein.");
                        }
                        selectedElement.SetValue(value);
                    }

                    // Hinzufügen der Attribute zum Element
                    foreach (var attribute in attributes)
                    {
                        selectedElement.SetAttributeValue(attribute.Key, attribute.Value);
                    }

                    // Hinzufügen der Sub-Elemente zum Element
                    foreach (var element in elements)
                    {
                        selectedElement.XPathAppendElement(element.Key, element.Value, resolver);
                    }

                    currentElement = selectedElement;
                }
            }
        }

        private static XElement GenerateNewElement(string name, IXmlNamespaceResolver resolver)
        {
            XName newElementName = null;
            if (name.Contains(XPathExtensions.namespaceSeparator))
            {
                if (resolver == null) throw new ArgumentNullException("resolver", "Es wurde keine NamespaceResolver angegeben.");

                var splittedName = name.Split(new [] { XPathExtensions.namespaceSeparator }, StringSplitOptions.None);

                var namespacePraefix = splittedName.Length > 1 ? splittedName[0] : string.Empty;
                var elementName = splittedName.Length > 1 ? splittedName[1] : splittedName[0];

                XNamespace nameSpace = resolver.LookupNamespace(namespacePraefix);
                if (string.IsNullOrEmpty(nameSpace.NamespaceName))
                {
                    throw new InvalidOperationException(string.Format("Es wurde kein Nampespace mit dem Präfix \"{0}\" gefunden.", namespacePraefix));
                }
                newElementName = nameSpace + elementName;
            }
            else
            {
                if(resolver != null)
                {
                    throw new XPathException("Das Element konnte nicht erstellt werden. Es fehlt ein entsprechender Namespace im Pfad.");
                }

                newElementName = name;
            }

            // Neues Element wird erstellt
            return new XElement(newElementName);
        }
    }
}
